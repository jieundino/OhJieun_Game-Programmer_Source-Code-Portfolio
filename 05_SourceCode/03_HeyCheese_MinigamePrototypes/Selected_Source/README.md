# <Hey Cheese!> — Selected Mini-game Source

## 문제 / 목표

전체 프로젝트 코드보다 게임 프로그래밍 판단이 드러나는
입력 처리, 상태 관리, 시간 흐름과 완료 조건 중심의 핵심 파일을 선별했습니다.

## 담당 범위

> **Project:** &lt;Hey Cheese!&gt;  
> **Role:** Mini-game Client Programmer  
> **Contribution:** 담당 미니게임의 클라이언트 로직 직접 구현 및 Android 테스트 빌드 검증

본 폴더의 파일은 지원자가 직접 작성한 코드입니다.
얼굴 인식, AR, TTS 등 다른 팀원의 구현은 포함하지 않습니다.

## 구조

| 파일 | 책임 |
| --- | --- |
| `MiniGameManager.cs` | 공통 시작·완료·복귀 |
| `MiniGame2_2Manager.cs` | 할 일 목록과 상태 기반 완료 조건 |
| `MiniGame3Manager.cs` | 카운트다운, Slider, 클리어 조건 |
| `MiniGame4Manager.cs` | 터치 유지, 진행도, 랜덤 방해와 재시작 |
| `PlayerController.cs` | 모바일 터치 입력과 이동 |
| `ScrollingBackground.cs` | 달리기 상태와 배경 스크롤 |

## 예외 / 검증

- 재시작 후 이전 상태가 남지 않는지 확인
- 일부 조건만 완료했을 때 클리어되지 않는지 확인
- 카운트다운과 플레이 상태의 순서 확인
- 터치 시작·유지·종료가 현재 상태와 올바르게 연결되는지 확인
- 내부 진행도와 UI 표시가 일치하는지 확인
- Android 실제 기기에서 입력과 완료 흐름 확인

## 한계 및 개선 방향

- Scene / Prefab / UI 연결과 일부 보조 코드는 제출 자료에서 생략
- 개별 Manager에 입력·상태·UI 책임이 함께 존재
- Coroutine 취소 정책과 공통 State 구조가 통일되어 있지 않음
- 자동화 테스트보다 실제 기기·사용자 검증 중심
- 공통 상태 전환 API와 입력 추상화, Debug Panel 추가 가능