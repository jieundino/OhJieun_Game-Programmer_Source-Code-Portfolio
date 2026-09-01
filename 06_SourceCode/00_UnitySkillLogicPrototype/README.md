# Unity Skill Logic Prototype

Unity C#으로 구현한 **전투 스킬 로직 기술 검증 프로토타입**입니다.

일반적인 전투 스킬 명세를 `Data → Runtime State → Validation → Execution → Feedback` 단계로 분해하고, Normal, DoT, Area 스킬의 실행 흐름과 예외 조건을 작은 Unity 환경에서 구현·검증했습니다.

본 폴더에는 전체 Unity 프로젝트가 아니라 포트폴리오 검토에 필요한 **직접 작성한 핵심 C# 코드**를 정리했습니다.

- [Architecture & Feature Verification 문서](../../04_Technical_Notes/00_UnitySkillLogicPrototype_Architecture_Verification.md)

<br>

## 📌 Project Overview

| 항목 | 내용 |
| :--- | :--- |
| **프로젝트명** | Unity Skill Logic Prototype |
| **프로젝트 유형** | Skill Feature Verification Prototype |
| **플랫폼** | PC / Unity Prototype |
| **엔진** | Unity |
| **개발 언어** | C# |
| **개발 기간** | 2026.07 |
| **개발 형태** | Solo Development |
| **담당 역할** | System Design / Client Programming / Feature Verification |

<br>

## 🎯 Prototype Goal

완성형 전투 시스템을 제작하는 것보다 다음 질문을 검증하는 데 목적을 두었습니다.

> **스킬 설정 데이터와 실행 중 상태를 분리하고, 서로 다른 스킬 규칙을 공통 실행 흐름 안에서 처리하면서 실패 조건에서도 상태 일관성을 유지할 수 있는가?**

이를 위해 스킬 시스템을 다음 단계로 분해했습니다.

```text
Skill Data
    ↓
Runtime State
    ↓
Activation Validation
    ↓
Target Search / Range Validation
    ↓
Skill Effect Execution
    ↓
Cooldown / Animation / UI Feedback
```

<br>

## 🙋‍♀️ My Role

본 프로젝트는 개인 기술 프로토타입으로 진행했으며 다음 영역을 직접 설계하고 구현했습니다.

- ScriptableObject 기반 스킬 데이터 구조 설계
- Skill Data와 Runtime State 분리
- 스킬 슬롯별 쿨타임 상태 관리
- 스킬 사용 요청과 실제 효과 실행 책임 분리
- 가장 가까운 적 자동 탐색 및 사거리 검증
- Normal / DoT / Area 스킬 실행 규칙 구현
- Coroutine 기반 DoT 처리
- 범위 공격 대상 중복 제거
- Enemy Health 및 사망 상태 구현
- 이벤트 기반 Enemy Health Bar 갱신
- Skill Cooldown UI 및 범위 Indicator 구현
- 카메라 기준 이동과 스킬 사용 방향 전환 구현
- 기능별 성공·실패·경계 조건 검증 시나리오 정의

<br>

## 🛠️ Tech Stack

| 분류 | 사용 기술 |
| :--- | :--- |
| **Engine / Language** | Unity, C# |
| **Data** | ScriptableObject |
| **Runtime State** | Plain C# Object |
| **Physics / Targeting** | `Physics.OverlapSphereNonAlloc`, `LayerMask`, `sqrMagnitude` |
| **Runtime Flow** | Coroutine, `Time.deltaTime` |
| **Data Structure** | `List`, `HashSet`, reusable `Collider[]` buffer |
| **UI** | Unity UGUI, `Slider`, `Image` |
| **Animation** | Animator, Animation Event |
| **Architecture** | Data / Runtime Separation, Controller / Executor Responsibility Separation |

<br>

## 📂 Code Structure

```text
UnitySkillLogicPrototype
├─ README.md
│
├─ SkillDataRuntimeSystem
│  ├─ README.md
│  ├─ SOSkill.cs
│  └─ SkillRuntime.cs
│
├─ SkillExecutionSystem
│  ├─ README.md
│  ├─ PlayerSkillController.cs
│  ├─ SkillExecutor.cs
│  └─ DotStatusEffect.cs
│
├─ CombatFeedbackSystem
│  ├─ README.md
│  ├─ EnemyHealth.cs
│  ├─ EnemyHealthBarPresenter.cs
│  ├─ EnemyHealthBarRoot.cs
│  ├─ SkillButton.cs
│  └─ SkillRangeIndicator.cs
│
└─ PlayerControlSystem
   ├─ README.md
   ├─ PlayerMovement.cs
   └─ FollowCamera.cs
```

<br>

## 🧩 Core Architecture

```text
SOSkill
  │  정적 스킬 설정 데이터
  ▼
SkillRuntime
  │  남은 쿨타임 등 플레이 중 상태
  ▼
PlayerSkillController
  │  입력 / 사용 가능 상태 / 성공 시 쿨타임 시작
  ▼
SkillExecutor
  ├─ Normal
  ├─ DoT
  └─ Area
       │
       ▼
EnemyHealth / DotStatusEffect
       │
       ▼
Health UI / Skill UI / Animation
```

<br>

## ⚔️ Implemented Skill Types

### Normal Skill

가장 가까운 유효 대상을 탐색하고, 대상이 사거리 안에 있는 경우 즉시 피해를 적용합니다.

```text
Target Search
→ Target Validation
→ Range Validation
→ Damage
```

유효한 대상이 없거나 사거리를 벗어나면 실행에 실패하고 쿨타임을 적용하지 않습니다.

### DoT Skill

유효한 단일 대상과 `DotStatusEffect` 실행 조건을 먼저 확인한 뒤 초기 피해와 지속 피해를 적용합니다.

```text
Target Search
→ Range Validation
→ DotStatusEffect Validation
→ Initial Damage
→ Interval-based Damage
```

동일 대상에게 다시 적용하면 기존 DoT Coroutine을 종료하고 새로운 DoT를 시작하는 **Refresh 방식**으로 동작합니다.

### Area Skill

플레이어 주변의 일정 반경을 탐색하고 범위 안에 있는 모든 유효 대상에게 피해를 적용합니다.

```text
Self-centered Skill Activation
→ OverlapSphereNonAlloc
→ Collider Search
→ EnemyHealth Extraction
→ Duplicate Removal
→ Area Damage
```

Physics 탐색 결과가 `Collider` 단위로 반환되는 점을 고려해 `HashSet<EnemyHealth>`로 실제 전투 대상 단위의 중복을 제거했습니다.

Area Skill은 자기 중심으로 발동하는 스킬이므로 적중 대상이 0명이어도 스킬 자체는 성공한 것으로 처리하고 쿨타임을 적용합니다.

<br>

## 📚 Implemented Systems

### 1. Skill Data & Runtime System

스킬의 정적 설정 데이터와 플레이 중 변경되는 쿨타임 상태를 분리했습니다.

- `SOSkill` — SkillType, Damage, Cooldown, Range, Area, DoT 설정
- `SkillRuntime` — RemainingCooldown, IsReady, CooldownRatio

[Skill Data & Runtime System 상세 보기](./SkillDataRuntimeSystem/README.md)

### 2. Skill Execution System

스킬 사용 요청, 대상 탐색, 조건 검증, 효과 적용과 쿨타임 시작 흐름을 관리합니다.

- 단일 대상 스킬은 실제 실행 성공에만 쿨타임 적용
- 자기 중심 Area Skill은 적중 0명과 발동 실패를 구분
- 가장 가까운 적 자동 탐색
- 사망 상태 및 사거리 검증
- Normal / DoT / Area 실행
- 범위 공격 대상 중복 제거

[Skill Execution System 상세 보기](./SkillExecutionSystem/README.md)

### 3. Combat Feedback System

전투 상태와 UI 표현을 이벤트 및 Presenter 구조로 연결했습니다.

- Enemy Health / Death State
- Event-based Health Bar Update
- Skill Cooldown UI
- Skill Range Indicator

[Combat Feedback System 상세 보기](./CombatFeedbackSystem/README.md)

### 4. Player Control System

카메라 방향을 기준으로 이동하고, 스킬 사용 시 대상 방향으로 회전하는 플레이어 제어 구조입니다.

- Camera-relative Movement
- Run / Jump
- Mouse Camera Rotation
- Face Target on Skill Execution

[Player Control System 상세 보기](./PlayerControlSystem/README.md)

<br>

## ✅ Feature Verification

정상 실행뿐 아니라 실패 및 경계 조건에서 상태가 일관되게 유지되는지 확인했습니다.

| 검증 시나리오 | 기대 동작 |
| --- | --- |
| 사용 가능한 단일 대상 스킬 | 즉시 실행 후 쿨타임 시작 |
| 쿨타임 중 재사용 | 실행 차단 |
| 단일 대상 스킬에서 대상 없음 | 실행 실패 / 쿨타임 미적용 |
| 단일 대상이 사거리 밖 | 실행 실패 / 쿨타임 미적용 |
| 사망한 대상 | Target 후보에서 제외 |
| Normal Skill | 단일 대상 즉시 피해 |
| DoT Skill | 초기 피해 후 설정 간격에 따라 지속 피해 |
| DoT 재적용 | 기존 효과를 중단하고 지속 시간 갱신 |
| Area Skill | 범위 내 유효한 모든 대상 피해 |
| Area Skill에서 적중 대상 없음 | 스킬 발동 성공 / 적중 0명 / 쿨타임 적용 |
| 하나의 적에 여러 Collider 존재 | `EnemyHealth` 기준 1회만 피해 |
| 스킬 실행 성공 | 대상 방향 회전 및 애니메이션 실행 |
| 쿨타임 진행 | SkillButton의 Cooldown UI 갱신 |
| 적 체력 변화 및 사망 | Health Bar 갱신 및 비활성화 |
| 카메라 뒤에 적 위치 | Health Bar 숨김 |

<br>

## 🎬 Demo Video

- [Combat Logic & UI Feedback](https://youtu.be/COhs8v4ZTNY?si=X1HxetiwKpY-we-C)
- [Feature Verification](https://youtu.be/QM03HsZIwEM?si=Y25z8ThtY_cNN9pa)

<br>

## 🔬 Current Limitations

현재 프로토타입은 스킬 명세를 데이터와 실행 규칙으로 구조화하는 최소 범위에 집중했습니다.

- 새로운 `SkillType` 추가 시 `SkillExecutor`의 분기 로직 수정 필요
- Targeting과 Effect Execution이 `SkillExecutor`에 함께 존재
- DoT는 Refresh 방식만 지원
- Buff / Debuff / Stack 기반 상태 효과 미구현
- 자원 소비, 캐스팅, Line of Sight 조건 미구현
- SkillButton은 Update에서 Runtime 상태를 조회해 UI 갱신
- 자동화된 EditMode / PlayMode Test 대신 수동 기능 검증 중심

후속 단계에서는 Targeting Rule과 Skill Effect를 독립 실행 단위로 분리하고, 여러 Effect를 조합할 수 있는 구조와 Unity Test Framework 기반 자동 검증을 검토할 예정입니다.
