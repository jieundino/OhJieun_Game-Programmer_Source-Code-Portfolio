# Source Code Portfolio

Unity C# 기반 개인 기술 프로토타입과 팀 프로젝트에서
직접 설계·구현·수정한 주요 코드를 프로젝트와 시스템 단위로 정리했습니다.

## 1. 문제 / 목표

전체 Unity 프로젝트를 그대로 제출하기보다,
게임 프로그래머로서의 설계 판단과 문제 해결 과정을
빠르게 검토할 수 있도록 핵심 코드만 선별했습니다.

각 시스템 README는 다음 내용을 중심으로 정리합니다.

- 시스템이 필요한 이유
- 직접 담당한 범위
- 클래스와 책임 구조
- 실패·경계 조건 검증
- 현재 구현의 한계와 개선 방향

## 2. 담당 범위

- `00_UnitySkillLogicPrototype`은 개인 기술 프로토타입입니다.
- 공동 프로젝트는 지원자가 직접 작성하거나 직접 수정한 코드만 선별했습니다.
- 프로젝트별 기여 비중과 협업 경계는 `../01_Project_Contribution.md`에 정리했습니다.
- Scene, Prefab, 일부 Manager, 콘텐츠 데이터와 외부 패키지는 제출 범위에서 생략되어 있습니다.

## 3. 구조

| 프로젝트 | 핵심 시스템 | 주요 확인 포인트 |
| --- | --- | --- |
| Unity Skill Logic Prototype | Skill Data / Execution / Feedback / Control | Data → Runtime → Validation → Execution |
| &lt;필연과 우연&gt; | Action Point / Puzzle / Interaction / SFX | 상태 관리, 입력 예외, 자원 관리 |
| &lt;네 발자국&gt; | Dialogue / Event-Result / Save / Log | 데이터 기반 흐름, 저장 안정성 |
| &lt;Hey Cheese!&gt; | Mini-game Logic | 입력, 상태, 완료 조건, 모바일 검증 |

## 4. 예외 / 검증

대표적으로 다음 조건을 확인했습니다.

- 대상 없음, 사거리 초과, 사망 대상
- 반복 입력과 중복 실행
- DropZone 경계와 잘못된 배치
- Save/Load 실패와 상태 불일치
- 네트워크 전송 실패와 중복 로그
- AudioSource 채널 포화
- Coroutine 재진입과 Scene 전환 이후 상태 복원

## 5. 한계 및 개선 방향

본 Repository는 전체 Unity 프로젝트의 독립 실행본이 아니라
지원자의 구현과 문제 해결 과정을 검토하기 위한 선별 코드입니다.

일부 코드는 Scene, Prefab, 다른 Manager와 데이터에 의존하며,
자동화 테스트보다 실제 플레이와 수동 기능 검증의 비중이 높습니다.