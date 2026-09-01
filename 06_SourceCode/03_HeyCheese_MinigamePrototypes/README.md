# 헤이 치즈! — Selected Source Code

본 폴더는 지원자가 직접 구현한 **미니게임 프로토타입과 상태 기반 진행 코드**를 선별한 자료입니다.

## 코드 소유 범위

- 제출 파일 작성·수정 비중: 100%
- 얼굴 인식, AR, TTS 등 다른 팀원의 구현은 포함하지 않음
- Scene, Prefab, UI 연결과 일부 데이터에 의존하는 코드 검토용 자료
- 프로젝트 전체 기여 비중은 상위 `02_Project_Contribution.md` 참고

## 포함 파일

| 파일 | 확인 가능한 내용 |
| --- | --- |
| `MiniGameManager.cs` | 공통 시작·완료·복귀 흐름 |
| `MiniGame2_2Manager.cs` | List/Dictionary 기반 진행 상태와 완료 조건 |
| `MiniGame3Manager.cs` | 카운트다운, Slider와 클리어 조건 |
| `MiniGame4Manager.cs` | 터치 유지, 진행도, 랜덤 방해와 재시작 |
| `PlayerController.cs` | 터치 입력과 이동 처리 |
| `ScrollingBackground.cs` | 플레이 상태와 배경 이동 연결 |

## 연구개발 관점

- 서로 다른 규칙을 작은 기능 단위로 구현
- 공통 흐름과 개별 완료 조건 분리
- Android 환경의 실제 터치·UI·상태 흐름 검증
- 사용자 테스트 결과와 검증 한계 문서화

상세 설명은 `04_Technical_Notes/05_HeyCheese_Minigame_Logic.md`를 참고합니다.
