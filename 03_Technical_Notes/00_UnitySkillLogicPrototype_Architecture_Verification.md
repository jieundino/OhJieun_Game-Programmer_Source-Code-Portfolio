# Unity Skill Logic Prototype — Architecture & Feature Verification

> **Project:** Unity Skill Logic Prototype  
> **Role:** Solo Developer / System Designer  
> **Contribution:** 스킬 시스템 요구사항 정의, 구조 설계, 구현·수정 및 기능 검증

## 1. 문서 목적

본 문서는 일반적인 전투 스킬 명세를 Unity C# 실행 구조로 변환한 과정과 기능 검증 결과를 정리합니다.

완성형 전투 시스템을 제작하기보다 다음 질문을 작은 프로토타입에서 확인하는 데 목적을 두었습니다.

> 스킬 설정 데이터와 플레이 중 상태를 분리하고, 서로 다른 스킬 규칙을 공통 실행 흐름 안에서 처리하면서 실패 조건에서도 상태 일관성을 유지할 수 있는가?

---

## 2. 구현 범위

- ScriptableObject 기반 스킬 설정 데이터
- 슬롯별 Runtime 쿨타임 상태
- 스킬 사용 가능 여부 확인
- 가장 가까운 적 자동 탐색
- 단일 대상 사거리 검증
- Normal / DoT / Area 스킬 실행
- Coroutine 기반 지속 피해
- 범위 탐색 대상 중복 제거
- Enemy Health와 사망 상태
- 이벤트 기반 Health Bar
- Cooldown UI와 범위 Indicator
- 카메라 기준 이동과 공격 방향 전환
- 정상·실패·경계 조건 검증

---

## 3. 요구사항 분해

스킬 사용 과정을 다음 단계로 분해했습니다.

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

| 단계 | 책임 | 주요 클래스 |
| --- | --- | --- |
| Skill Data | 피해량, 쿨타임, 사거리, DoT 값 정의 | `SOSkill` |
| Runtime State | 슬롯별 남은 쿨타임 관리 | `SkillRuntime` |
| Activation Validation | 슬롯, 사용 중 상태, 쿨타임 확인 | `PlayerSkillController` |
| Target Validation | 대상 탐색, 사망 상태, 거리 확인 | `SkillExecutor` |
| Effect Execution | Normal / DoT / Area 효과 적용 | `SkillExecutor`, `DotStatusEffect` |
| Feedback | 체력, 쿨타임, 범위, 애니메이션 표시 | Presenter, `SkillButton`, Animator |

---

## 4. 핵심 설계 결정

### 4.1 설정 데이터와 Runtime 상태 분리

`SOSkill`은 변경이 적은 스킬 설정만 보유하고, 플레이 중 변하는 남은 쿨타임은 `SkillRuntime`에서 관리했습니다.

ScriptableObject에 Runtime 상태를 저장하지 않아 여러 슬롯이나 캐릭터가 동일한 Skill Asset을 참조해도 쿨타임 상태가 공유되지 않습니다.

```text
SOSkill Asset
├─ SkillType
├─ Damage
├─ CoolTime
├─ Range
└─ DoT / Area Parameters
        │
        ▼
SkillRuntime Instance per Slot
├─ RemainingCooldown
├─ IsReady
└─ CooldownRatio
```

### 4.2 실행 성공 이후에만 쿨타임 적용

스킬 입력 직후 쿨타임을 시작하지 않고, `SkillExecutor`가 실행 성공을 반환한 경우에만 `StartCooldown()`을 호출했습니다.

```text
Input
→ Runtime.IsReady 확인
→ SkillExecutor.TryExecute()
   ├─ false: 상태 변화 없음
   └─ true: Cooldown + Animation + Feedback
```

이를 통해 단일 대상 스킬에서 대상이 없거나 사거리를 벗어난 요청이 플레이 상태에 영향을 주지 않도록 했습니다.

### 4.3 스킬 유형별 성공 조건 구분

모든 스킬에 동일한 성공 기준을 적용하지 않았습니다.

- Normal / DoT: 유효한 대상이 사거리 안에 있어야 성공
- Area: 플레이어 중심으로 스킬 자체가 발동하므로 적중 대상이 0명이어도 성공

Area Skill은 `적중 성공 여부`와 `스킬 발동 성공 여부`를 분리해 처리했습니다.

### 4.4 Physics 결과의 대상 단위 변환

`Physics.OverlapSphereNonAlloc`은 Collider 단위로 결과를 반환합니다. 하나의 Enemy가 Root Collider와 Hit Collider를 함께 가지면 동일 대상이 여러 번 탐색될 수 있습니다.

`HashSet<EnemyHealth>`를 사용해 Collider 결과를 실제 전투 대상 단위로 변환하고, Enemy당 피해를 한 번만 적용했습니다.

### 4.5 전투 상태와 UI 표현 분리

`EnemyHealth`는 Slider나 Canvas를 직접 참조하지 않고 상태 변경 이벤트만 발생시킵니다.

`EnemyHealthBarPresenter`가 이벤트를 구독해 Health Bar를 갱신함으로써 전투 규칙과 화면 표현의 책임을 분리했습니다.

---

## 5. 스킬별 실행 흐름

### 5.1 Normal Skill

```text
FindNearestTarget(skill.range)
→ Target 존재 여부 확인
→ 사망 상태 확인
→ Range 확인
→ TakeDamage()
→ Target 방향 회전
→ Cooldown / Animation
```

### 5.2 DoT Skill

```text
FindNearestTarget(skill.range)
→ Target Validation
→ DotStatusEffect 존재 여부 확인
→ Initial Damage
→ ApplyDot()
→ Interval-based Damage
→ Cooldown / Animation
```

DoT 재적용 시 기존 Coroutine을 중단하고 새로운 지속 시간으로 시작하는 Refresh 정책을 사용했습니다.

### 5.3 Area Skill

```text
Player-centered Area Activation
→ OverlapSphereNonAlloc
→ EnemyHealth 추출
→ 사망 대상 제외
→ HashSet 중복 제거
→ 범위 내 대상 피해
→ Cooldown / Animation
```

적중 대상이 없어도 자기 중심 스킬 동작 자체는 실행된 것으로 처리합니다.

---

## 6. 기능 검증

| 검증 시나리오 | 기대 동작 |
| --- | --- |
| 사용 가능한 단일 대상 스킬 실행 | 피해 적용 후 쿨타임 시작 |
| 쿨타임 중 동일 스킬 재사용 | 실행 차단 |
| 단일 대상 스킬에서 대상 없음 | 실행 실패 / 쿨타임 미적용 |
| 단일 대상이 사거리 밖 | 실행 실패 / 쿨타임 미적용 |
| 사망한 대상 존재 | 신규 Target 후보에서 제외 |
| Normal Skill | 가장 가까운 유효 대상에게 즉시 피해 |
| DoT Skill | 초기 피해 후 설정 간격에 따라 지속 피해 |
| 동일 대상에게 DoT 재적용 | 기존 DoT 중단 후 지속 시간 갱신 |
| Area Skill에서 범위 내 적 존재 | 모든 유효 대상에게 피해 |
| Area Skill에서 적중 대상 없음 | 스킬 발동 성공 / 적중 0명 / 쿨타임 적용 |
| 하나의 적에 여러 Collider 존재 | `EnemyHealth` 기준 피해 1회 |
| 스킬 실행 성공 | 대상 방향 회전과 Animation 실행 |
| 쿨타임 진행 | SkillButton의 Cooldown UI 갱신 |
| 적 체력 변화 | Health Bar 비율 갱신 |
| 적 사망 | Collider와 Health Bar 비활성화 |
| 카메라 뒤에 적 위치 | Health Bar 숨김 |

---

## 7. 구현 중 확인한 문제와 수정

### 7.1 DoT 부분 성공 가능성

DoT 컴포넌트 존재 여부를 피해 적용 이후에 확인하면 피해는 적용됐지만 실행 결과는 실패가 되는 부분 성공 상태가 발생할 수 있었습니다.

모든 실행 전제 조건을 먼저 확인한 뒤 피해와 지속 효과를 적용하도록 순서를 변경했습니다.

### 7.2 Area Skill 문서와 코드의 성공 조건 불일치

초기 검증표에서는 모든 스킬의 대상 없음 조건을 실패로 표현했지만, 자기 중심 Area Skill은 대상 유무와 관계없이 발동하도록 구현했습니다.

단일 대상 스킬과 자기 중심 범위 스킬의 성공 조건을 문서에서 분리했습니다.

### 7.3 Indicator 재호출 시 이전 Coroutine 영향

Indicator가 짧은 간격으로 재호출되면 이전 Hide Coroutine이 새 표시를 먼저 비활성화할 수 있었습니다.

재호출 시 기존 Coroutine을 중단하고 새로운 표시 시간으로 갱신하도록 수정했습니다.

### 7.4 탐색 반경과 스킬 사거리의 기준

고정 탐색 반경이 스킬 사거리보다 작으면 실제 사거리 안의 대상을 찾지 못할 수 있습니다.

단일 대상 탐색은 해당 스킬의 `range`를 기준으로 수행하도록 수정해 탐색 조건과 검증 조건의 기준을 통일했습니다.

---

## 8. 현재 한계

- 새로운 `SkillType` 추가 시 `SkillExecutor`의 분기 로직 수정 필요
- Targeting과 Effect Execution이 `SkillExecutor`에 함께 존재
- DoT는 Refresh 정책만 지원
- Buff / Debuff / Stack 기반 상태 효과 미구현
- 자원 소비, 캐스팅, Line of Sight 조건 미구현
- SkillButton은 매 Frame Runtime 상태를 조회
- 자동화된 EditMode / PlayMode Test 대신 수동 기능 검증 중심

---

## 9. 후속 개선 방향

### Targeting Rule 분리

```text
ITargetingRule
├─ NearestTarget
├─ SelfArea
├─ DirectionalArea
└─ GroundTarget
```

### Skill Effect 조합

```text
ISkillEffect
├─ DamageEffect
├─ DotEffect
├─ HealEffect
├─ BuffEffect
└─ KnockbackEffect
```

하나의 Skill Asset이 여러 Effect를 조합하도록 변경하면 새로운 스킬 추가 시 `SkillExecutor`의 switch문 수정 범위를 줄일 수 있습니다.

### 검증 자동화

- 쿨타임 시작·종료 테스트
- 대상 없음 / 사거리 초과 테스트
- 다중 Collider 중복 피해 테스트
- DoT Tick 수와 Refresh 테스트
- Area Skill 0 Hit 성공 정책 테스트

현재 수동 검증 시나리오를 Unity Test Framework 기반 PlayMode Test로 전환할 수 있습니다.

---

## 10. 사용 기술

- Unity, C#
- ScriptableObject
- Plain C# Runtime Object
- `Physics.OverlapSphereNonAlloc`
- `LayerMask`, `sqrMagnitude`
- `List`, `HashSet`, reusable `Collider[]` buffer
- Coroutine
- C# Event / Presenter Pattern
- Unity UGUI
- Animator / Animation Event
