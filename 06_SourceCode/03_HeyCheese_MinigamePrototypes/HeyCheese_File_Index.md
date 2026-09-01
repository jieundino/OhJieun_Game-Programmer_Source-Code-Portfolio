# Hey Cheese! File Index

본 폴더에는 헤이치즈 프로젝트 중 지원자가 직접 작성한 미니게임 클라이언트 코드 가운데, 연구개발 관점의 구현 판단이 드러나는 핵심 파일만 선별하여 포함했습니다.

전체 프로젝트 코드가 아니라, 입력 처리, 상태 관리, 완료 조건 판정, 시간 흐름 처리, UI 피드백 구조를 보여줄 수 있는 파일 중심으로 정리했습니다.

## Included Files

| 파일 | 포함 이유 |
|---|---|
| `MiniGameManager.cs` | 미니게임 공통 시작/종료/복귀 흐름을 담당하는 기반 클래스 |
| `MiniGame4Manager.cs` | 터치 유지, 진행도 증가/감소, 랜덤 방해 이벤트, 재시작 조건을 포함한 상태 기반 로직 |
| `MiniGame2_2Manager.cs` | 할 일 목록, 진행도, 완료 기준을 Dictionary/List로 관리하는 상태 관리 로직 |
| `MiniGame3Manager.cs` | 달리기 미니게임의 카운트다운, 목표 Slider, 클리어 조건 관리 |
| `PlayerController.cs` | 터치 입력, 달리기 시작, Burst, 캐릭터 이동 연출 처리 |
| `ScrollingBackground.cs` | 달리기 상태와 배경 스크롤 속도 연결 |

## Omitted Files

아래 파일들은 실제 프로젝트에서는 사용되었지만, 본 제출 자료에서는 핵심 로직 중심으로 구성하기 위해 제외했습니다.

| 파일 | 제외 이유 |
|---|---|
| `FoodButton.cs` | 음식 버튼 클릭 이벤트 전달 중심의 단순 보조 코드 |
| `CharacterReaction.cs` | 선택 수에 따른 Sprite 변경 중심의 UI 피드백 코드 |
| `DirtyDishButton.cs` | 그릇 클릭 처리 및 진행도 증가 보조 코드 |
| `TrashButton.cs` | 쓰레기 클릭 처리 및 진행도 증가 보조 코드 |
| `HiddenCharacterButton.cs` | 숨은 캐릭터 클릭 처리 및 정답 표시 보조 코드 |
| `MiniGame1Manager.cs` | 숨바꼭질 미니게임 관리 코드이나, 제출 자료에서는 더 복합적인 상태 관리 예시를 우선 포함 |
| `MiniGame2_1Manager.cs` | 음식 선택 미니게임 관리 코드이나, 제출 자료에서는 ToDo 상태 관리 구조를 우선 포함 |
| `TouchIndicatorController.cs` | 입력 보조 유틸리티로, 필요 시 별도 QA 문서에서 설명 |
| `AlphaImage.cs` | Alpha 기반 터치 판정 보정 유틸리티로, 필요 시 별도 QA 문서에서 설명 |