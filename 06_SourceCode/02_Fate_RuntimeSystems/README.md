# 필연과 우연 — Selected Source Code

본 폴더는 지원자가 직접 설계·구현·수정한 **상태 기반 진행, 입력 판정과 오디오 자원 관리 코드**를 선별한 자료입니다.

## 코드 소유 범위

- 제출 파일 작성·수정 비중: 100%
- 공동개발 프로젝트 전체 코드가 아니라 지원자의 담당 범위만 포함
- Scene, Prefab, Constants, GameManager와 데이터에 의존하는 코드 검토용 자료
- 프로젝트 전체 기여 비중은 상위 [`../../01_Project_Contribution.md`](../../01_Project_Contribution.md) 참고

## 핵심 확인 지점

| 폴더 | 핵심 확인 지점 |
| --- | --- |
| `ActionPointSystem` | 공통 상태 흐름과 Room별 규칙 분리, 입력 중복 방어 |
| `PuzzleDragAndDropSystem` | 좌표 변환, 유효성 판정, 경계 입력 처리 |
| `InteractionObjectSystem` | 상호작용 공통 진입점과 개별 동작 확장 |
| `SFXPriorityChannelSystem` | 한정된 채널의 우선순위 정책과 포화 fallback |

## 권장 열람 순서

1. `ActionPointSystem/ActionPointManager.cs`
2. `ActionPointSystem/Room1ActionPointManager.cs`
3. `ActionPointSystem/Room2ActionPointManager.cs`
4. `SFXPriorityChannelSystem/SoundPlayer.cs`
5. `PuzzleDragAndDropSystem/SewingBoxBead.cs`

## 주요 기술적 의사결정

- 요청 실행 전 현재 상태 검증
- 공통 처리와 콘텐츠별 예외 분리
- 거리 판정과 배치 규칙의 단계적 검증
- Low/High Priority와 예약 채널
- 반복 입력과 Scene 수명주기 이슈 대응

## 현재 구조의 한계

- 행동력 상태와 UI 상태의 결합
- Room 증가 시 상속 계층 확대 가능
- SFX 일반 채널의 실제 Priority 미추적

후속 개선 방향은 `04_Technical_Notes`에 정리했습니다.
