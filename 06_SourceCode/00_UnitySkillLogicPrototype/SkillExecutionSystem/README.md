# Skill Execution System

스킬 사용 요청부터 Target 탐색, 조건 검증, 효과 적용, 쿨타임 시작까지의 실행 흐름을 관리하는 시스템입니다.

`PlayerSkillController`는 입력과 사용 상태를 관리하고, `SkillExecutor`는 실제 전투 판정과 효과 적용을 담당합니다.

---

## 설계 의도

스킬마다 입력 처리, 쿨타임 검사, 대상 탐색, 사거리 판정과 피해 적용을 모두 개별 구현하면 공통 조건이 반복되고 실행 흐름을 추적하기 어려워집니다.

이를 방지하기 위해 역할을 다음과 같이 분리했습니다.

- `PlayerSkillController` — 스킬 사용 요청, Runtime 조회, 사용 가능 여부 확인, 쿨타임 및 애니메이션 연결
- `SkillExecutor` — Target 탐색, 사거리 검증, SkillType별 효과 실행
- `DotStatusEffect` — 시간 기반 지속 피해 처리

핵심은 단순 입력 발생이 아니라 **각 스킬의 성공 조건을 만족해 실제 실행된 경우에만 쿨타임을 시작하는 것**입니다.

---
## 담당 범위

> **Contribution:** Skill Activation Pipeline, Target Validation,
> Normal·DoT·Area 실행 규칙과 실패·경계 조건 구현·검증

---

## 주요 구현

### `PlayerSkillController` — Skill Activation Pipeline

스킬 슬롯에 대응하는 `SkillRuntime`을 조회한 뒤 현재 스킬 사용 상태와 쿨타임을 확인합니다.

```csharp
public void TryActivateSkill(int skillIndex)
{
    SkillRuntime runtime = GetSkillRuntime(skillIndex);

    if (runtime == null || isUsingSkill)
        return;

    if (!runtime.IsReady)
        return;

    bool succeeded = ActivateSkill(runtime.SkillData);

    if (succeeded)
    {
        runtime.StartCooldown();
    }
}
```

`SkillExecutor`가 `false`를 반환하면 쿨타임과 애니메이션을 적용하지 않습니다.

### SkillType별 공통 실행 진입점

`TryExecute`에서 `SkillType`에 따라 실행 규칙을 분기합니다.

```text
TryExecute(skill)
├─ Normal → ExecuteNormal()
├─ DoT    → ExecuteDot()
└─ Area   → ExecuteArea()
```

### 스킬 유형별 성공 조건

| 스킬 유형 | 성공 조건 |
| --- | --- |
| Normal | 유효한 대상이 스킬 사거리 안에 존재 |
| DoT | 유효한 대상이 사거리 안에 존재하고 DoT 실행 컴포넌트 확인 |
| Area | 자기 중심 범위 스킬이 발동되면 성공, 적중 대상 수는 별도 결과 |

Area Skill에서는 `적중 0명`을 `스킬 발동 실패`로 처리하지 않습니다.

### 가장 가까운 적 자동 탐색

`Physics.OverlapSphereNonAlloc`과 재사용 `Collider[]` Buffer를 사용해 스킬 사거리 안의 적을 탐색합니다.

- `EnemyHealth`가 없는 Collider 제외
- 이미 사망한 대상 제외
- `sqrMagnitude`를 사용해 가장 가까운 대상 비교
- 탐색된 대상은 별도의 `ValidateTarget`에서 다시 검증

```csharp
private EnemyHealth FindNearestTarget(float searchRadius)
{
    int hitCount = Physics.OverlapSphereNonAlloc(
        attackOrigin.position,
        searchRadius,
        hitBuffer,
        enemyLayer);

    EnemyHealth nearestTarget = null;
    float nearestDistanceSqr = float.MaxValue;

    for (int i = 0; i < hitCount; i++)
    {
        EnemyHealth candidate =
            hitBuffer[i].GetComponentInParent<EnemyHealth>();

        if (candidate == null || candidate.IsDead)
            continue;

        float distanceSqr =
            (candidate.transform.position -
             attackOrigin.position).sqrMagnitude;

        if (distanceSqr >= nearestDistanceSqr)
            continue;

        nearestDistanceSqr = distanceSqr;
        nearestTarget = candidate;
    }

    return nearestTarget;
}
```

탐색 반경과 실제 스킬 사거리의 기준이 달라지지 않도록 해당 스킬의 `range`를 탐색 반경으로 사용합니다.

### Target Validation

실제 스킬 효과를 적용하기 전 다음 조건을 확인합니다.

- 탐색된 대상이 존재하는가
- 대상이 사망하지 않았는가
- 대상이 해당 스킬의 사용 가능 거리 안에 있는가

거리 판정은 제곱 거리 상태에서 비교해 불필요한 제곱근 계산을 피했습니다.

### Normal Skill

가장 가까운 유효 대상에게 즉시 피해를 적용합니다.

```text
FindNearestTarget(skill.range)
→ ValidateTarget()
→ TakeDamage()
```

실행 성공 시 `PlayerMovement.FaceTargetInstant()`를 호출해 플레이어가 공격 대상 방향으로 회전합니다.

### DoT Skill

단일 대상과 지속 피해 실행 조건을 모두 검증한 뒤 효과를 적용합니다.

```text
FindNearestTarget(skill.range)
→ ValidateTarget()
→ DotStatusEffect Validation
→ Initial Damage
→ ApplyDot()
→ Interval-based Damage
```

모든 전제 조건을 확인한 이후에 피해를 적용해 `피해는 적용됐지만 실행 결과는 실패`하는 부분 성공 상태를 방지했습니다.

동일 대상에게 DoT가 다시 적용되면 기존 Coroutine을 중단하고 새로운 DoT를 시작하는 Refresh 방식입니다.

### Area Skill

플레이어 주변의 일정 반경을 탐색하고 모든 유효 대상에게 피해를 적용합니다.

Physics 탐색은 Collider 단위로 결과를 반환하므로 하나의 Enemy가 여러 Collider를 가진 경우 중복 피해가 발생할 수 있습니다.

이를 방지하기 위해 `HashSet<EnemyHealth>`로 전투 대상 단위의 중복을 제거했습니다.

```csharp
int hitCount = Physics.OverlapSphereNonAlloc(
    areaCenter,
    skill.areaRadius,
    hitBuffer,
    enemyLayer);

uniqueTargets.Clear();

for (int i = 0; i < hitCount; i++)
{
    EnemyHealth target =
        hitBuffer[i].GetComponentInParent<EnemyHealth>();

    if (target == null || target.IsDead)
        continue;

    uniqueTargets.Add(target);
}

foreach (EnemyHealth target in uniqueTargets)
{
    target.TakeDamage(skill.damage);
}
```

적중 대상이 0명이어도 자기 중심 Area Skill의 발동은 성공으로 반환하며 결과 메시지에 적중 수를 기록합니다.

---

## 구조 다이어그램

```text
Player Input / SkillButton
        │
        ▼
PlayerSkillController.TryActivateSkill()
├─ SkillRuntime 조회
├─ isUsingSkill 검사
├─ IsReady 검사
└─ ActivateSkill()
        │
        ▼
SkillExecutor.TryExecute()
├─ Normal
│    └─ Target Search → Validation → Damage
├─ DoT
│    └─ Target Search → Validation → Component Check → DoT
└─ Area
     └─ Self Activation → NonAlloc Search → Deduplication → Damage
        │
        ▼
Execution Result
├─ false → Cooldown / Animation 미적용
└─ true  → StartCooldown() + Animation
```

---

## 예외 / 검증

### 단일 대상 실행 실패 시 상태 변화 차단

대상이 없거나 사거리 밖이면 Effect, Cooldown과 Animation을 적용하지 않습니다.

### 자기 중심 Area Skill의 성공 조건

Area Skill은 적중 대상 유무와 관계없이 발동 가능한 스킬로 정의했습니다.
따라서 적중 대상이 0명이어도 Cooldown과 Animation을 적용합니다.

이를 통해 `Target-dependent Skill`과 `Self-activated Skill`의
성공 조건을 구분했습니다.

### 범위 공격의 중복 피해 방지

하나의 Enemy가 여러 Collider를 가지고 있어도
`HashSet<EnemyHealth>`에 한 번만 등록되므로
Enemy 단위로 피해가 한 번만 적용되는지 확인했습니다.

### 사망 대상 제외

Target Search와 Area Skill 양쪽에서 `IsDead`를 검사해
사망한 Enemy가 새로운 공격 대상에 포함되지 않도록 했습니다.

### DoT 부분 성공 방지

DoT 실행에 필요한 컴포넌트와 대상 조건을 모두 확인한 뒤
피해를 적용해, 피해만 적용되고 실행 결과는 실패하는
부분 성공 상태가 발생하지 않는지 확인했습니다.

---

## 한계 및 개선 방향

- 현재 `SkillType` switch문으로 실행 규칙을 분기하므로 SkillType이 증가하면 `SkillExecutor`가 비대해질 수 있음
- Target 탐색과 Effect Execution 책임이 `SkillExecutor`에 함께 존재
- 고정 크기 `Collider[]` Buffer보다 많은 Collider가 탐색되면 일부 결과가 포함되지 않을 수 있음
- DoT는 기존 효과를 갱신하는 Refresh 정책만 지원하며 Stack 또는 독립 중첩 정책은 미지원

### 개선 방향

- Target 탐색 규칙을 `ITargetingRule`과 같은 독립 책임으로 분리
- Damage, DoT, Heal, Buff 등을 `ISkillEffect` 단위로 분리
- 전투 규모에 따라 Collider Buffer 크기 조정 또는 탐색 정책 보완
- Unity Test Framework 기반 실패·경계 조건 자동 검증

---

## 사용 기술

- `Physics.OverlapSphereNonAlloc` — 재사용 Buffer 기반 주변 대상 탐색
- `LayerMask` — Enemy Layer 필터링
- `Vector3.sqrMagnitude` — 제곱 거리 기반 대상 비교
- `HashSet<EnemyHealth>` — 다중 Collider 대상 중복 제거
- Coroutine — 시간 기반 DoT 처리
- Animator / Animation Event — 스킬 실행 상태와 종료 시점 연결
- `out` Parameter — 실행 결과 메시지와 Target Transform 반환
