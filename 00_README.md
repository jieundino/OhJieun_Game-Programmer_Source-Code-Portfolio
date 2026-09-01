# Unity C# 연구개발 프로그래머 소스코드 포트폴리오

본 자료는 **마비노기 모바일 연구개발 프로그래머** 지원을 위해, Unity C# 프로젝트에서 신규 기능을 구조화하고 구현·검증한 경험을 정리한 소스코드 포트폴리오입니다.

개인 기술 프로토타입과 공동개발 프로젝트에서 지원자가 직접 설계·구현·수정한 코드를 선별하고, 각 코드의 설계 의도, 실행 흐름, 검증 결과, 문제 해결 과정과 현재 한계를 함께 정리했습니다.

- 지원자: 오지은
- 지원 직무: 마비노기 모바일 연구개발 프로그래머
- GitHub 포트폴리오: https://github.com/jieundino/GameProgrammer-Portfolio

---

## 1. 포트폴리오에서 확인할 수 있는 역량

- 일반적인 게임 기능 명세를 데이터, 런타임 상태, 검증 조건과 실행 규칙으로 분해
- ScriptableObject 설정 데이터와 플레이 중 Runtime State 분리
- 실행 성공 여부를 기준으로 쿨타임과 후속 상태 변화를 적용하는 흐름 설계
- 단일 대상, 지속 피해, 범위 공격처럼 서로 다른 규칙을 공통 실행 진입점에서 처리
- 데이터 기반 콘텐츠 구조와 공통 처리·예외 규칙 분리
- 상태 불일치, 입력 중복, 저장 실패, 자원 충돌 등 런타임 이슈 분석
- 정상 조건뿐 아니라 실패·경계 조건을 포함한 기능 검증 시나리오 정의
- 출시·전시·사용자 테스트 환경에서 기능 안정성 점검
- 구현 구조, 재현 조건, 수정 결과, 한계와 후속 개선 방향 문서화

---

## 2. 코드 소유 범위와 공동개발 기여

- `06_SourceCode/00_UnitySkillLogicPrototype`은 지원자가 단독으로 설계·구현한 개인 기술 프로토타입의 핵심 코드입니다.
- 나머지 공동개발 프로젝트 폴더에는 지원자가 직접 작성하거나 직접 수정한 파일만 선별했습니다.
- 일부 코드는 원본 Unity 프로젝트의 Scene, Prefab, Manager, CSV 데이터 및 외부 패키지에 의존하므로 단독 실행용 전체 프로젝트가 아니라 **코드 검토용 자료**입니다.
- 프로젝트별 전체 기여 비중, 개발 파트 기여 비중, 코드 소유 범위와 협업 경계는 `02_Project_Contribution.md`에 정리했습니다.

---

## 3. 권장 열람 순서

1. `01_Portfolio_Guide.pdf` — 전체 포트폴리오 요약 및 연구개발 직무 연결성
2. `06_SourceCode/00_UnitySkillLogicPrototype/README.md` — 대표 기술 프로토타입의 구조와 검증 결과
3. `03_SourceCode_Index.md` — 우선 검토 코드와 핵심 확인 지점
4. `02_Project_Contribution.md` — 프로젝트별 기여 비중, 직접 작성 범위, 협업 경계
5. `04_Technical_Notes` — 시스템별 설계 의도, 검증, 한계와 개선 방향
6. `05_Runtime_Issue_Analysis` — 재현 조건, 원인 분석, 수정과 검증 기록
7. `06_SourceCode` — 지원자가 직접 작성·수정한 선별 코드

---

## 4. 제출 자료 구성

```text
00_README.md
01_Portfolio_Guide.pdf
02_Project_Contribution.md
03_SourceCode_Index.md

04_Technical_Notes/
├─ 00_UnitySkillLogicPrototype_Architecture_Verification.md
├─ 01_FourFootsteps_CSV_Event_System.md
├─ 02_FourFootsteps_SaveLoad_System.md
├─ 03_Fate_ActionPointManager.md
├─ 04_Fate_SFX_Priority_Channel.md
└─ 05_HeyCheese_Minigame_Logic.md

05_Runtime_Issue_Analysis/
└─ Runtime_Issue_Analysis.md

06_SourceCode/
├─ 00_UnitySkillLogicPrototype/
├─ 01_FourFootsteps_DataDrivenSystems/
├─ 02_Fate_RuntimeSystems/
└─ 03_HeyCheese_MinigamePrototypes/
```

---

## 5. 프로젝트별 핵심 경험

| 프로젝트 | 역할 | 연구개발 관점 핵심 경험 | 결과 |
| --- | --- | --- | --- |
| **Unity Skill Logic Prototype** | Solo Developer / System Designer | 스킬 명세 구조화, 설정 데이터·Runtime State 분리, 실행 검증, 성공 기준과 예외 조건 정의 | Normal·DoT·Area 스킬 및 기능 검증 프로토타입 구현 |
| **네 발자국** | Main Client Programmer | CSV·ID 기반 이벤트 구조, 조건 판정과 결과 실행 분리, Atomic Write, 로그 재시도 구조 | STOVE 출시, 제1저자 연구성과 2건 (학술발표 1건, 학술지 논문 1편) |
| **필연과 우연** | Client / System Programmer | 상태 기반 행동력 시스템, 입력 예외 방어, 우선순위 기반 오디오 채널 정책 | STOVE·App Store 출시, BIC·Beaver Rocks 전시 |
| **헤이 치즈!** | Mini-game Client Programmer | 4종 미니게임 프로토타이핑, 공통 진행 흐름, Android 빌드 검증 | 사용자 30명 파일럿 테스트, 제2저자 논문 |

---

## 6. 검토 시 참고사항

본 포트폴리오는 기능의 최종 결과만 나열하지 않고 다음 내용을 함께 제공합니다.

- 왜 해당 구조가 필요했는지
- 요구사항을 어떤 상태와 실행 단계로 분해했는지
- 성공과 실패를 어떤 기준으로 구분했는지
- 어떤 예외 상황과 경계 조건을 방어했는지
- 어떤 방식으로 동작을 확인했는지
- 현재 구조에 어떤 한계가 있는지
- 후속 개선 시 어떤 방향을 적용할 수 있는지
