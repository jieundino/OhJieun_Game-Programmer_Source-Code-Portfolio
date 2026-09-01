# 프로젝트 기여 범위 및 코드 소유권

본 문서는 개인 및 공동개발 프로젝트에서 지원자가 담당한 범위, 제출 코드의 소유권과 협업 경계를 정리한 문서입니다.

기업 요구사항에 맞춰 프로젝트별로 다음 정보를 구분했습니다.

- 전체 프로젝트 기준 기여 비중
- 개발 파트 기준 기여 비중
- 제출 코드 중 지원자 직접 작성·수정 비중
- 직접 담당한 기능과 공동 작업 범위
- 제출 자료에서 제외한 영역

---

# 1. Unity Skill Logic Prototype

## 1.1 프로젝트 정보

| 항목 | 내용 |
| --- | --- |
| 개발 형태 | 개인 기술 프로토타입 |
| 담당 역할 | Solo Developer / System Designer |
| 담당 기간 | 2026.07 |
| 주요 기술 | Unity, C#, ScriptableObject, Coroutine, Physics Query, UGUI |
| 결과 | Normal·DoT·Area 스킬 실행 구조, 기능 검증 영상 및 기술 문서 작성 |

## 1.2 기여 비중

| 기준 | 비중 | 산정 근거 |
| --- | --- | --- |
| 전체 프로젝트 기준 | **100%** | 개인 기술 프로토타입으로 기획 범위 설정, 구조 설계, 구현과 검증을 단독 수행 |
| 개발 파트 기준 | **100%** | 제출한 시스템과 코드 전체를 직접 구현 |
| 본 제출 코드 기준 | **100%** | 제출한 C# 파일 전체를 지원자가 직접 작성 |

## 1.3 직접 담당한 기능

- 일반적인 전투 스킬 명세를 Data → Runtime State → Validation → Execution → Feedback 단계로 분해
- ScriptableObject 기반 스킬 설정 데이터 구조 설계
- Skill Asset과 슬롯별 Runtime 쿨타임 상태 분리
- 스킬 사용 요청, 실행 가능 상태 확인, 실제 효과 실행 책임 분리
- 가장 가까운 유효 대상 탐색과 사거리 검증
- Normal / DoT / Area 스킬 실행 규칙 구현
- Coroutine 기반 DoT 처리와 재적용 시 Refresh 정책 구현
- `Physics.OverlapSphereNonAlloc` 기반 범위 탐색
- 다중 Collider 대상의 중복 피해 방지
- Enemy Health와 사망 상태 구현
- 이벤트 기반 Enemy Health Bar 갱신
- Skill Cooldown UI와 범위 Indicator 구현
- 카메라 기준 이동과 스킬 성공 시 대상 방향 전환
- 정상·실패·경계 조건을 포함한 기능 검증 시나리오 작성

## 1.4 제출 코드

| 폴더 | 주요 파일 | 확인 가능한 내용 |
| --- | --- | --- |
| `SkillDataRuntimeSystem` | `SOSkill.cs`, `SkillRuntime.cs` | 정적 설정 데이터와 플레이 중 상태 분리 |
| `SkillExecutionSystem` | `PlayerSkillController.cs`, `SkillExecutor.cs`, `DotStatusEffect.cs` | 사용 가능 상태 검사, 대상 탐색, 실행 성공 기준, 효과 적용 |
| `CombatFeedbackSystem` | `EnemyHealth.cs`, Presenter 및 UI 파일 | 전투 상태와 화면 표현의 책임 분리 |
| `PlayerControlSystem` | `PlayerMovement.cs`, `FollowCamera.cs` | 카메라 기준 이동과 스킬 방향 전환 연동 |

## 1.5 코드 및 리소스 소유 범위

- 제출한 C# 코드는 지원자가 직접 작성했습니다.
- 캐릭터 모델, 애니메이션, 아이콘과 기타 외부 리소스는 코드 소유 범위에 포함하지 않습니다.
- 전체 Unity 프로젝트가 아니라 포트폴리오 검토에 필요한 핵심 코드와 설명 문서를 제출했습니다.
- 본 프로젝트의 목적은 완성형 전투 시스템 제작이 아니라 스킬 명세를 실행 가능한 구조로 변환하고 예외 조건까지 검증하는 기술 프로토타이핑입니다.

## 1.6 대표적인 설계 및 검증

- 동일한 Skill Asset을 여러 슬롯에서 참조해도 쿨타임 상태가 공유되지 않도록 Runtime 객체 분리
- 단일 대상 스킬에서 대상 없음 또는 사거리 초과 시 실행 실패와 쿨타임 미적용
- 자기 중심 범위 스킬은 적중 대상이 없어도 스킬 자체가 실행되므로 쿨타임 적용
- 사망한 대상을 신규 Target 후보에서 제외
- 하나의 Enemy가 여러 Collider를 보유해도 `EnemyHealth` 기준으로 피해 1회 적용
- DoT 재적용 시 기존 Coroutine을 종료하고 새로운 지속 시간으로 갱신
- 실제 쿨타임 상태와 UI 표시가 동일한 Runtime 값을 참조하도록 구성

---

# 2. 네 발자국

## 2.1 프로젝트 정보

| 항목 | 내용 |
| --- | --- |
| 개발 형태 | 4인 공동개발 |
| 담당 역할 | Main Client Programmer / Initial Concept Planning |
| 담당 기간 | 2025.03 ~ 2026.02 |
| 주요 기술 | Unity, C#, CSV, Dictionary, Newtonsoft.Json, UnityWebRequest, PlayerPrefs |
| 결과 | STOVE 출시, 학술발표대회·학술지 논문 제1저자 |

## 2.2 기여 비중

| 기준 | 비중 | 산정 근거 |
| --- | --- | --- |
| 전체 프로젝트 기준 | **약 45%** | 기획·개발·아트 등 전체 업무를 포함한 실제 역할 분담 기준 |
| 개발 파트 기준 | **약 60%** | 클라이언트 시스템 설계, 구현, 유지보수, 테스트 범위 기준 |
| 본 제출 코드 기준 | **100%** | 제출한 파일은 지원자가 직접 작성하거나 직접 수정한 코드만 선별 |

## 2.3 직접 담당한 기능

- CSV 기반 대사·선택지·이벤트·결과 데이터 구조
- `DialogueManager`와 Dialogue Queue
- `EventManager / ResultManager` 실행 파이프라인
- AND / OR 조건 판정과 Instant / Sequential 실행 모드
- 선택·조사 결과와 멀티엔딩 분기 상태 관리
- Newtonsoft.Json 기반 Save/Load
- Atomic Write, `.bak` 백업과 레거시 복구
- 사용자 행동 로그 수집, 중복 방지와 Retry Queue
- 출시·실험 빌드 기능 점검
- 시스템 구조와 사용자 테스트 결과 문서화

## 2.4 제출 코드

| 폴더 | 주요 파일 | 확인 가능한 내용 |
| --- | --- | --- |
| `DialogueSystem` | `DialogueManager.cs`, `KoreanJosa.cs` | Queue 기반 대사 요청 순서 제어 |
| `EventResultSystem` | `EventManager.cs`, `ResultManager.cs` | 데이터 기반 조건 판정과 결과 실행 |
| `SaveSystem` | `SaveManager.cs` | Atomic Write, 타입 보존, fallback 복구 |
| `LogSystem` | Queue/Reporter/Types 파일 | 로그 식별, 실패 보존과 재전송 |

## 2.5 협업 경계

- 아트 리소스와 캐릭터 애니메이션은 다른 팀원이 담당했습니다.
- 일부 콘텐츠 데이터는 팀 기획 논의를 바탕으로 공동 작성했습니다.
- 팀 저장소 전체 코드가 아니라 지원자가 직접 작성·수정한 클라이언트 시스템만 제출했습니다.
- 프로젝트 전체 기술 요소와 지원자 직접 구현 범위를 혼동하지 않도록 폴더 README에 의존성과 생략 범위를 명시했습니다.

## 2.6 대표적인 문제 해결

- 대사 동시 호출 충돌 → Dialogue Queue 도입
- 결과 실행 순서 충돌 → Instant / Sequential 모드 분리
- 저장 중 파일 손상 → Atomic Write와 백업 적용
- 복합 타입 역직렬화 실패 → 전용 직렬화와 fallback 복구
- 로그 중복·소실 → RunID/eventId와 Retry Queue 적용
- UnityEvent 씬 경계 참조 → ScriptableObject Command 구조로 개선

---

# 3. 필연과 우연

## 3.1 프로젝트 정보

| 항목 | 내용 |
| --- | --- |
| 개발 형태 | 6인 공동개발 |
| 담당 역할 | Client / System Programmer |
| 담당 기간 | 2024.03 ~ 2026.01 |
| 주요 기술 | Unity, C#, UI Event System, Coroutine, AudioSource |
| 결과 | STOVE·App Store 출시, BIC·Beaver Rocks 전시 |

## 3.2 기여 비중

| 기준 | 비중 | 산정 근거 |
| --- | --- | --- |
| 전체 프로젝트 기준 | **약 20%** | 기획·개발·아트·사운드 등 전체 업무를 포함한 실제 역할 분담 기준 |
| 개발 파트 기준 | **약 45%** | 퍼즐·행동력·상호작용·사운드 시스템과 QA 범위 기준 |
| 본 제출 코드 기준 | **100%** | 제출한 파일은 지원자가 직접 작성하거나 직접 수정한 코드만 선별 |

## 3.3 직접 담당한 기능

- 반짇고리 드래그 앤 드롭 퍼즐
- UI 좌표 변환, 최근접 DropZone과 Row Constraint 판정
- Dictionary 기반 퍼즐 정답 상태 관리
- `ActionPointManager` 기반 행동력·날짜 진행
- Room별 행동력 규칙을 하위 클래스로 분리
- 상호작용 오브젝트와 단서 획득 로직
- SFX 우선순위, 예약 채널, Round-robin과 Click Debounce
- Room1·Room2·멀티엔딩 진행 QA
- 출시·전시 환경의 입력·상태·사운드 문제 개선
- 시퀀스 수정과 확인 항목 Notion 문서화

## 3.4 제출 코드

| 폴더 | 주요 파일 | 확인 가능한 내용 |
| --- | --- | --- |
| `ActionPointSystem` | `ActionPointManager.cs`, Room별 클래스 | 공통 상태 흐름과 예외 규칙 분리 |
| `PuzzleDragAndDropSystem` | `SewingBoxBead.cs`, `SewingBoxPuzzle.cs` | 입력 좌표, 유효성 판정, 정답 관리 |
| `InteractionObjectSystem` | `EventObject.cs`, 개별 오브젝트 | 공통 상호작용 진입점과 개별 동작 |
| `SFXPriorityChannelSystem` | `SoundPlayer.cs` | 한정된 채널의 우선순위 할당과 포화 대응 |

## 3.5 협업 경계

- 전체 스토리, 아트, 사운드 리소스 제작과 일부 클라이언트 기능은 다른 팀원이 담당했습니다.
- 제출 코드는 지원자가 직접 구현하거나 직접 수정한 기능으로 한정했습니다.
- Scene·Prefab·데이터와 다른 Manager에 대한 의존성은 각 폴더 README에 명시했습니다.

## 3.6 대표적인 문제 해결

- 빠른 연속 클릭의 행동력 중복 차감 → 상태 기반 요청 차단
- 비정상 DropZone 배치 → 좌표 변환·최근접 탐색·행 제약 검사
- Room별 예외 누적 → 추상 클래스와 하위 규칙 분리
- 반복 클릭음의 채널 점유 → Low/High 정책, 예약 채널과 Voice Stealing
- 저장 복원 이후 상태·UI 불일치 → 날짜 인덱스와 오브젝트 상태 복원 보완

---

# 4. 헤이 치즈!

## 4.1 프로젝트 정보

| 항목 | 내용 |
| --- | --- |
| 개발 형태 | 4인 공동개발 |
| 담당 역할 | Mini-game Client Programmer |
| 담당 기간 | 2025.03 ~ 2025.07 |
| 주요 기술 | Unity, C#, Touch Input, Coroutine, UGUI |
| 결과 | Android 파일럿 테스트 빌드, JCCT 논문 제2저자 |

## 4.2 기여 비중

| 기준 | 비중 | 산정 근거 |
| --- | --- | --- |
| 전체 프로젝트 기준 | **약 20%** | 기획·개발 등 전체 역할 분담 기준 |
| 개발 파트 기준 | **약 25%** | 담당 미니게임 구현과 테스트 빌드 대응 범위 기준 |
| 본 제출 코드 기준 | **100%** | 제출한 파일은 지원자가 직접 작성하거나 직접 수정한 코드만 선별 |

## 4.3 직접 담당한 기능

- 숨바꼭질, 음식 선택 / 식탁 정리, 달리기, 춤추기 등 4종 미니게임
- 모바일 터치 입력과 오브젝트 상호작용
- 미니게임 시작·진행·완료·재시작·복귀 흐름
- 미니게임별 진행 및 완료 조건
- Coroutine 기반 카운트다운, 제한 시간과 이동 연출
- Slider, Text, Sprite 기반 상태 피드백
- Android 테스트 빌드 기능 점검
- 사용자 테스트 결과 정리와 논문 작성 참여

## 4.4 제출 코드

| 파일 | 확인 가능한 내용 |
| --- | --- |
| `MiniGameManager.cs` | 미니게임 공통 시작·완료·복귀 흐름 |
| `MiniGame2_2Manager.cs` | List/Dictionary 기반 작업 상태와 완료 조건 |
| `MiniGame3Manager.cs` | 카운트다운, 진행 Slider와 클리어 조건 |
| `MiniGame4Manager.cs` | 터치 유지, 진행도, 랜덤 방해와 재시작 |
| `PlayerController.cs` | 터치 입력, 이동과 Burst 처리 |
| `ScrollingBackground.cs` | 플레이 상태와 배경 스크롤 연결 |

## 4.5 협업 경계

- 얼굴 인식, 표정 인식, AR, TTS 등 프로젝트 전체 기술 요소 중 지원자가 직접 구현하지 않은 영역은 제출 코드와 기여 범위에서 제외했습니다.
- 아트 리소스와 전체 에피소드 기획은 팀 협업 결과입니다.
- 제출 자료는 지원자가 담당한 미니게임 클라이언트 코드만 포함합니다.

## 4.6 대표적인 검증 경험

- 실제 Android 기기에서 입력, 진행 상태와 완료 흐름 확인
- 반복 조작과 상태 초기화 확인
- 일반 초등학생 30명 파일럿 테스트 결과를 바탕으로 난이도와 피드백 검토
- 목표 사용자와 테스트 집단의 차이를 연구 한계로 명시

---

# 5. 전체 기여 범위 요약

| 프로젝트 | 개발 형태 | 핵심 담당 영역 | 제출 코드 작성 비중 |
| --- | --- | --- | --- |
| Unity Skill Logic Prototype | 개인 프로젝트 | 스킬 데이터·상태·검증·실행·피드백 구조 | 100% |
| 네 발자국 | 공동개발 | 데이터 기반 이벤트, Save/Load, 로그 안정성 | 100% |
| 필연과 우연 | 공동개발 | 상태 기반 진행, 퍼즐 입력, 상호작용, SFX 채널 | 100% |
| 헤이 치즈! | 공동개발 | 미니게임 입력, 조건 판정, 상태 피드백 | 100% |

제출 코드 작성 비중 100%는 **ZIP에 선별해 포함한 파일 기준**입니다. 공동개발 프로젝트 전체 코드 기여율을 의미하지 않으며, 프로젝트 전체 및 개발 파트 기여 비중은 각 프로젝트 표에 별도로 제시했습니다.
