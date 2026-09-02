# Unity C# Game Programmer Source Code Portfolio

**오지은 | Unity C# Game Programmer**

Unity C# 기반 개인 기술 프로토타입과 팀 게임 프로젝트에서
직접 설계·구현·수정한 주요 소스코드를 선별한 포트폴리오입니다.

게임 기능을 단순히 동작시키는 데서 끝내지 않고,
**요구사항 → 데이터/상태 구조 → 조건 검증 → 실행 → 피드백**의 흐름으로 나누어 구현하고,
반복 입력, 상태 불일치, 저장 실패, 자원 충돌과 같은 런타임 문제를
재현·분석·수정·검증해 온 과정을 함께 정리했습니다.

공동 프로젝트는 지원자가 직접 작성하거나 수정한 코드만 선별했으며,
프로젝트별 역할, 기여 비중, 코드 소유 범위와 협업 경계는
[`01_Project_Contribution.md`](./01_Project_Contribution.md)에서 확인할 수 있습니다.

---

## Featured Projects

| 프로젝트 | 형태 / 역할 | 주요 기술 | 핵심 구현 및 문제 해결 |
| --- | --- | --- | --- |
| **Unity Skill Logic Prototype** | 개인 / Solo Developer | Unity, C#, ScriptableObject, Coroutine, Physics Query, UGUI | 스킬 설정 데이터와 Runtime State 분리, 실행 조건 검증, Normal·DoT·Area 실행 구조 |
| **&lt;필연과 우연&gt;** | 6인 / Client · System Programmer | Unity, C#, UI Event System, Coroutine, AudioSource | 퍼즐, 행동력·날짜 진행, SFX 우선순위 채널, 입력·상태 예외 대응 |
| **&lt;네 발자국&gt;** | 4인 / Main Client Programmer | Unity, C#, CSV, Dictionary, Newtonsoft.Json, UnityWebRequest | 데이터 기반 이벤트, Dialogue Queue, Save/Load 안정화, 로그 재시도 |
| **&lt;Hey Cheese!&gt;** | 4인 / Mini-game Client Programmer | Unity, C#, Touch Input, Coroutine, UGUI | 4종 미니게임, 모바일 입력·완료 조건, 사용자 테스트 |

### 주요 결과

- **<필연과 우연>** — STOVE · App Store 출시, BIC · Beaver Rocks 전시
- **<네 발자국>** — STOVE 출시, 학술발표 및 학술지 논문 제1저자
- **<Hey Cheese!>** — 초등학생 30명 파일럿 테스트, JCCT 논문 제2저자
- **Unity Skill Logic Prototype** — 개인 설계·구현 및 정상·실패·경계 조건 검증

---

## Core Skills

### Game System Design

- 기능 요구사항을 데이터, Runtime State, Validation, Execution 단계로 분해
- 설정 데이터와 플레이 중 변경되는 상태를 분리해 관리
- 공통 실행 흐름과 콘텐츠별 예외 규칙 분리
- 기능 실행 성공 여부에 따라 후속 상태와 피드백을 적용

### Data & State Management

- CSV·ID 기반 데이터 중심 이벤트 구조
- `Dictionary`, `Queue`, `HashSet`을 목적에 맞게 활용
- Save/Load 데이터와 Runtime State의 일관성 관리
- 선택·이벤트·멀티엔딩 상태를 데이터와 연결

### Runtime Stability

- 반복 입력과 중복 실행 방어
- 저장·복원 과정의 상태 불일치 분석
- Coroutine 및 Scene 수명주기 관련 문제 확인
- 한정된 AudioSource 자원의 우선순위 배분
- 정상 조건뿐 아니라 실패·경계 조건을 포함한 검증

### Documentation & Collaboration

- 문제 상황과 재현 조건을 구체화
- 원인, 수정 내용, 확인 결과를 문서로 기록
- 공동 프로젝트의 직접 구현 범위와 협업 경계를 명시
- 구조적 한계와 후속 개선 방향을 구분해 정리

---

## Recommended Review Order

코드 리뷰 시 아래 순서로 확인하면
개인 프로토타입의 설계부터 실제 팀 프로젝트의 문제 해결 과정까지 빠르게 파악할 수 있습니다.

### 1. Unity Skill Logic Prototype

[`06_SourceCode/00_UnitySkillLogicPrototype/README.md`](./06_SourceCode/00_UnitySkillLogicPrototype/README.md)

개인 기술 프로토타입으로,
스킬 명세를 다음 단계로 구조화했습니다.

```text
Skill Data
    ↓
Runtime State
    ↓
Validation
    ↓
Execution
    ↓
Feedback