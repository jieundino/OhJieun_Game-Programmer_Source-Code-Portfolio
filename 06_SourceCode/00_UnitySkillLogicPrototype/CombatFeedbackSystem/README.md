# Combat Feedback System

Enemy의 체력·사망 상태와 Health Bar, Skill Cooldown UI, 범위 Indicator를 연결하는 전투 피드백 시스템입니다.

전투 상태를 UI에서 직접 수정하지 않고 이벤트와 Presenter를 통해 상태와 화면 표현의 책임을 분리했습니다.

---

## 설계 의도

스킬 실행 로직이 체력바나 스킬 버튼 UI를 직접 수정하면 전투 규칙과 화면 표현이 강하게 결합됩니다.

체력 처리 방식을 변경할 때 UI 코드까지 함께 수정하거나, UI가 없는 테스트 환경에서도 전투 로직이 UI 컴포넌트에 의존하는 문제가 발생할 수 있습니다.

이를 방지하기 위해 다음과 같이 역할을 분리했습니다.

- `EnemyHealth` — 체력과 사망 상태 관리
- `EnemyHealthBarPresenter` — EnemyHealth 이벤트 구독 및 Health Bar 갱신
- `EnemyHealthBarRoot` — 런타임 Health Bar의 공통 부모 제공
- `SkillButton` — SkillRuntime 상태를 Skill UI에 반영
- `SkillRangeIndicator` — 스킬 적용 범위 시각화

---
## 담당 범위
> **Contribution:** Enemy Health·Death 상태, 이벤트 기반 Health Bar,
> Skill Cooldown UI와 Range Indicator 구현·검증

---
## 주요 구현

### `EnemyHealth` — 전투 상태 관리

`EnemyHealth`는 CurrentHealth와 MaxHealth를 관리하고 피해 적용 결과에 따라 이벤트를 발생시킵니다.

```csharp
public float MaxHealth => maxHealth;
public float CurrentHealth { get; private set; }
public bool IsDead => CurrentHealth <= 0f;

public event Action<float, float> HealthChanged;
public event Action Died;
```

`TakeDamage`에서는 이미 사망했거나 유효하지 않은 피해 요청을 차단하고 체력이 변경되면 `HealthChanged` 이벤트를 호출합니다.

체력이 0 이하가 되면 보유한 Collider를 비활성화하고 `Died` 이벤트와 사망 애니메이션을 실행합니다.

### `EnemyHealthBarPresenter` — 이벤트 기반 UI 갱신

Presenter는 시작 시 Health Bar Prefab을 생성한 뒤 `EnemyHealth` 이벤트를 구독합니다.

```csharp
enemyHealth.HealthChanged += OnHealthChanged;
enemyHealth.Died += OnEnemyDied;
```

- `HealthChanged` — Current / Max 비율을 계산해 Slider 값 갱신
- `Died` — Health Bar 비활성화
- `OnDestroy` — 이벤트 구독 해제 및 생성한 UI 제거

EnemyHealth가 직접 Slider를 수정하지 않고 상태 변경 사실만 이벤트로 전달합니다.

### World Position → Screen Position

Enemy 머리 위의 월드 좌표를 카메라 기준 Screen Position으로 변환해 Health Bar 위치를 갱신합니다.

```text
Enemy World Position + Offset
            ↓
Camera.WorldToScreenPoint()
            ↓
Health Bar Screen Position
```

`screenPosition.z <= 0`이면 Enemy가 카메라 뒤에 있다고 판단해 Health Bar를 숨깁니다.

### `SkillButton` — Cooldown UI

SkillButton은 슬롯 인덱스에 대응하는 `SkillRuntime`을 조회하고 `CooldownRatio`를 Image의 `fillAmount`에 반영합니다.

```csharp
private void Update()
{
    if (runtime == null)
        return;

    imgCool.fillAmount = runtime.CooldownRatio;
}
```

SkillButton이 별도의 쿨타임을 계산하지 않고 Runtime 상태를 그대로 사용해 실제 사용 가능 상태와 UI 표시의 기준을 통일했습니다.

### `SkillRangeIndicator` — 적용 범위 시각화

스킬 실행 시 원형 Indicator를 활성화하고 `radius * 2`를 지름으로 사용해 크기를 조정합니다.

짧은 간격으로 재호출되는 경우 기존 Hide Coroutine을 중단하고 새로운 표시 시간으로 갱신해 이전 호출이 새 Indicator를 먼저 숨기지 않도록 했습니다.

---

## 구조 다이어그램

```text
SkillExecutor
├─ EnemyHealth.TakeDamage()
└─ SkillRangeIndicator.ShowCircle()
          │
          ▼
EnemyHealth
├─ CurrentHealth / MaxHealth
├─ HealthChanged(current, max)
└─ Died()
          │ Event
          ▼
EnemyHealthBarPresenter
├─ Slider 갱신
├─ WorldToScreenPoint 위치 추적
├─ 카메라 뒤 UI 숨김
└─ 사망 시 Health Bar 비활성화

SkillRuntime
└─ CooldownRatio
          │
          ▼
SkillButton
└─ Image.fillAmount 갱신
```

---

## 검증 및 고려 사항

### 전투 상태와 UI 의존성 분리

`EnemyHealth`는 UI 컴포넌트를 참조하지 않습니다. Health Bar가 없는 테스트 씬에서도 체력과 사망 로직을 독립적으로 사용할 수 있습니다.

### 사망 후 UI 처리

Enemy 사망 시 `Died` 이벤트를 통해 Health Bar를 숨기고 Presenter가 파괴될 때 이벤트 구독을 해제해 불필요한 참조가 남지 않도록 했습니다.

### 카메라 뒤의 UI 숨김

`WorldToScreenPoint` 결과의 z값을 확인해 카메라 뒤에 위치한 Enemy의 Health Bar가 화면 반대편에 표시되지 않도록 했습니다.

### 실제 쿨타임과 UI 기준 통일

Skill UI가 별도 타이머를 관리하지 않고 `SkillRuntime.CooldownRatio`를 사용하므로 내부 사용 가능 상태와 표시 상태가 서로 다른 기준을 사용하지 않습니다.

### 현재 개선 가능 지점

`SkillButton`은 매 Frame Runtime 상태를 조회합니다. 현재 스킬 슬롯 수에서는 단순한 구현을 우선했지만 스킬 수가 증가하는 경우 Runtime 상태 변경 이벤트를 추가해 필요한 시점에만 UI를 갱신하는 방식과 비교할 수 있습니다.

---
## 한계 및 개선 방향

- SkillButton은 매 Frame Runtime 상태를 조회
- Enemy별 Health Bar를 런타임 생성하므로 적 수가 많아지면 Pooling 검토 필요
- 체력 외 Shield, Buff/Debuff 등의 상태 표현 구조는 미구현
- UI 규모가 증가하면 Presenter 생성·수명주기를 별도 UI Manager로 분리 가능

---

## 사용 기술

- C# Event (`Action<float, float>`, `Action`) — 전투 상태 변경 전달
- Unity UGUI `Slider` — Enemy Health 비율 표시
- Unity UGUI `Image.fillAmount` — Skill Cooldown 표시
- `Camera.WorldToScreenPoint` — 월드 좌표 기반 UI 위치 추적
- Runtime Prefab Instantiation — Enemy별 Health Bar 생성
- Coroutine — Skill Range Indicator 표시 시간 관리
- Presenter Pattern — 전투 상태와 UI 표현 책임 분리
