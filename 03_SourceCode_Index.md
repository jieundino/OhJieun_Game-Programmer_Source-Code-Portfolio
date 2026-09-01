# Source Code Index

본 문서는 면접관과 코드 리뷰어가 핵심 구현을 빠르게 확인할 수 있도록 소스코드의 권장 열람 순서와 확인 지점을 정리합니다.

## 공통 안내

- `00_UnitySkillLogicPrototype`은 지원자가 단독으로 설계·구현한 개인 기술 프로토타입입니다.
- 공동개발 프로젝트 폴더에는 지원자가 직접 작성하거나 직접 수정한 코드만 선별했습니다.
- 전체 Unity 프로젝트가 아니므로 일부 Scene, Prefab, Manager, CSV 및 외부 라이브러리에 의존합니다.
- 각 폴더의 README에서 원본 프로젝트 의존성, 생략 범위와 핵심 설계 의도를 확인할 수 있습니다.

---

## 권장 코드 검토 순서

| 우선순위 | 시스템 | 주요 파일 | 확인할 내용 |
| --- | --- | --- | --- |
| 1 | Skill Data / Runtime | `SOSkill.cs`, `SkillRuntime.cs` | 정적 설정 데이터와 슬롯별 플레이 상태 분리 |
| 2 | Skill Activation Pipeline | `PlayerSkillController.cs`, `SkillExecutor.cs` | 사용 가능 상태 검사, 실행 결과 반환, 성공 시 쿨타임 적용 |
| 3 | Target / Status Handling | `SkillExecutor.cs`, `DotStatusEffect.cs`, `EnemyHealth.cs` | 대상 탐색, 사거리 검증, DoT, 사망 대상 필터링, 중복 피해 방지 |
| 4 | Combat Feedback Integration | Presenter 및 UI 파일 | 전투 상태와 UI 표현 책임 분리, Runtime 기반 쿨타임 표시 |
| 5 | Event / Result Pipeline | `EventManager.cs`, `ResultManager.cs` | 데이터 기반 조건 판정, 실행 모드와 결과 처리 |
| 6 | Save Reliability | `SaveManager.cs` | Atomic Write, 타입 보존, 백업·fallback 복구 |
| 7 | Log Retry | `EndingLogQueueManager.cs`, `EndingLogReporter.cs` | 로그 식별, 중복 방지, 전송 실패 재시도 |
| 8 | ActionPoint State Flow | `ActionPointManager.cs`, Room별 클래스 | 공통 상태 흐름과 콘텐츠별 예외 분리 |
| 9 | SFX Channel Policy | `SoundPlayer.cs` | Low/High 채널 범위, Round-robin, Voice Stealing |
| 10 | Puzzle Validation | `SewingBoxBead.cs`, `SewingBoxPuzzle.cs` | 좌표 변환, 유효 DropZone, Row Constraint |
| 11 | Mini-game Prototype | `MiniGameManager.cs`, 개별 Manager | 공통 흐름, 입력·시간·완료 조건 |

---

# 1. Unity Skill Logic Prototype

다음 순서로 검토하면 설정 데이터가 실제 스킬 실행과 상태 변화로 연결되는 흐름을 확인할 수 있습니다.

```text
SOSkill
→ SkillRuntime
→ PlayerSkillController
→ SkillExecutor
→ EnemyHealth / DotStatusEffect
→ Health Bar / Cooldown UI / Animation
```

## SkillDataRuntimeSystem

- `SOSkill.cs`
  - ScriptableObject 기반 스킬 설정 데이터
  - Normal / DoT / Area 타입과 피해량, 사거리, 쿨타임, 지속 피해 값 정의
- `SkillRuntime.cs`
  - 슬롯별 `RemainingCooldown` 관리
  - `IsReady`, `CooldownRatio`, `StartCooldown()`, `Tick()` 제공
  - 동일한 Skill Asset을 참조하더라도 플레이 상태가 공유되지 않도록 분리

## SkillExecutionSystem

- `PlayerSkillController.cs`
  - 스킬 슬롯 입력과 Runtime 조회
  - 현재 스킬 사용 상태와 쿨타임 검사
  - `SkillExecutor`가 성공을 반환한 경우에만 쿨타임 시작
  - 스킬 성공 시 대상 방향 전환과 Animation 연결
- `SkillExecutor.cs`
  - SkillType별 공통 실행 진입점
  - 가장 가까운 유효 대상 탐색과 사거리 검증
  - 단일 피해, 지속 피해, 자기 중심 범위 공격 실행
  - `Physics.OverlapSphereNonAlloc`과 재사용 Buffer 사용
  - `HashSet<EnemyHealth>` 기반 다중 Collider 대상 중복 제거
- `DotStatusEffect.cs`
  - Coroutine 기반 간격별 지속 피해
  - 재적용 시 기존 Coroutine을 중단하고 새 지속 시간으로 갱신

## CombatFeedbackSystem

- `EnemyHealth.cs`
  - 체력과 사망 상태 관리
  - `HealthChanged`, `Died` 이벤트 제공
- `EnemyHealthBarPresenter.cs`
  - EnemyHealth 이벤트 구독
  - World Position을 Screen Position으로 변환해 Health Bar 갱신
- `SkillButton.cs`
  - `SkillRuntime.CooldownRatio`를 UI에 반영
- `SkillRangeIndicator.cs`
  - 스킬 반경을 지름으로 변환해 원형 Indicator 표시

## PlayerControlSystem

- `PlayerMovement.cs`
  - 카메라 Forward / Right 기준 이동
  - 스킬 성공 시 대상 방향으로 즉시 회전
- `FollowCamera.cs`
  - 우클릭 기반 yaw / pitch 회전과 Pitch Clamp

---

# 2. Four Footsteps — Data-driven Systems

## EventResultSystem

- `EventManager.cs`
  - CSV 파싱과 Event ID 등록
  - AND / OR 조건 판정
  - Instant / Sequential 모드 선택
  - 조건을 만족한 EventLine의 Result 실행
- `ResultManager.cs`
  - Result ID 유형 판정
  - 대사, 변수, 페이드, 씬 이동과 오브젝트 동작 실행
  - Function-wrapped Result 처리

## SaveSystem

- `SaveManager.cs`
  - Atomic Write와 `.bak` 백업
  - 값과 타입 정보 분리 저장
  - 레거시 데이터와 복합 타입 복구
  - 저장·복원 이후 UI 및 게임 상태 동기화

## LogSystem

- `EndingLogQueueManager.cs`
  - PlayerPrefs 기반 전송 대기 큐
  - 성공 응답 이후 큐 제거
- `EndingLogReporter.cs`
  - RunID와 eventId 기반 로그 구성
- `MemoryPuzzleStateExtractor.cs`
  - 저장 파일에서 테스트 데이터 추출

---

# 3. Fate — Runtime Systems

## ActionPointSystem

- `ActionPointManager.cs`
  - 공통 행동력 처리와 날짜 전환 연출
- `Room1ActionPointManager.cs`, `Room2ActionPointManager.cs`
  - Room별 규칙과 예외 처리

## PuzzleDragAndDropSystem

- `SewingBoxBead.cs`
  - 드래그 입력, 좌표 변환, DropZone 탐색, Row Constraint
- `SewingBoxPuzzle.cs`
  - Dictionary 기반 정답 상태 관리

## SFXPriorityChannelSystem

- `SoundPlayer.cs`
  - Low/High 2단계 Priority
  - High 예약 채널
  - Round-robin 탐색
  - High 요청의 Voice Stealing
  - `Time.unscaledTime` Click Debounce

---

# 4. Hey Cheese — Mini-game Prototypes

- `MiniGameManager.cs`: 공통 시작·완료·재시작·복귀 흐름
- `MiniGame2_2Manager.cs`: 할 일 목록과 완료 상태 관리
- `MiniGame3Manager.cs`: 카운트다운, Slider와 클리어 판정
- `MiniGame4Manager.cs`: 터치 유지, 진행도 변화와 랜덤 방해
- `PlayerController.cs`: 모바일 입력과 이동 처리
- `ScrollingBackground.cs`: 달리기 상태와 배경 이동 연결
