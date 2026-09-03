# Skill Data & Runtime System

스킬의 **정적 설정 데이터**와 플레이 중 변경되는 **Runtime 상태**를 분리한 구조입니다.  
ScriptableObject는 스킬 명세를 관리하고, 별도의 C# Runtime 객체는 남은 쿨타임을 관리합니다.

---

## 설계 의도

스킬의 피해량, 사거리, 재사용 대기시간과 같은 값은 설정 단계에서 정의되지만, 남은 쿨타임은 플레이 중 계속 변경됩니다.

두 종류의 값을 하나의 ScriptableObject에서 관리하면 Asset 데이터와 플레이 상태의 책임이 섞이고, 여러 슬롯이나 캐릭터가 같은 Skill Asset을 참조할 때 Runtime 상태가 공유될 수 있습니다.

이를 방지하기 위해 다음과 같이 역할을 분리했습니다.

- `SOSkill` — 변경이 적은 정적 스킬 설정 데이터
- `SkillRuntime` — 플레이 중 독립적으로 변경되는 쿨타임 상태

---

## 담당 범위

> **Project:** Unity Skill Logic Prototype  
> **Role:** Solo Developer / System Designer  
> **Contribution:** 스킬 설정 데이터와 슬롯별 Runtime 상태 구조 설계·구현 및 쿨타임 상태 검증

직접 작성:
- `SOSkill.cs`
- `SkillRuntime.cs`
---

## 주요 구현

### `SOSkill` — ScriptableObject 기반 스킬 데이터

`CreateAssetMenu`를 사용해 Unity Editor에서 스킬 Asset을 생성할 수 있도록 구성했습니다.

| 필드 | 역할 |
| --- | --- |
| `skillType` | Normal / Dot / Area 구분 |
| `coolTime` | 재사용 대기시간 |
| `animationName` | 실행할 Animator State 이름 |
| `icon` | Skill UI 아이콘 |
| `damage` | 기본 피해량 |
| `range` | 단일 대상 스킬 사용 가능 거리 |
| `areaRadius` | 범위 공격 반경 |
| `dotDamagePerTick` | DoT 1회당 피해량 |
| `dotDuration` | DoT 지속 시간 |
| `dotInterval` | DoT 피해 발생 간격 |

동일한 SkillType에서는 코드 수정 없이 Asset 데이터 값을 변경하여 피해량, 쿨타임, 사거리 등이 다른 스킬 Variant를 구성할 수 있습니다.

### `SkillRuntime` — 플레이 중 상태 관리

`SkillRuntime`은 원본 `SOSkill`을 참조하면서 현재 남은 쿨타임을 독립적으로 관리합니다.

- `RemainingCooldown` — 현재 남은 재사용 대기시간
- `IsReady` — 스킬 사용 가능 여부
- `CooldownRatio` — Skill UI에 사용할 쿨타임 비율
- `StartCooldown` — Skill Data의 `coolTime`으로 쿨타임 시작
- `Tick` — `deltaTime`만큼 남은 쿨타임 감소

```csharp
public class SkillRuntime
{
    public SOSkill SkillData { get; }

    public float RemainingCooldown { get; private set; }

    public bool IsReady => RemainingCooldown <= 0f;

    public float CooldownRatio =>
        SkillData.coolTime > 0f
            ? RemainingCooldown / SkillData.coolTime
            : 0f;

    public void StartCooldown()
    {
        RemainingCooldown = Mathf.Max(0f, SkillData.coolTime);
    }

    public void Tick(float deltaTime)
    {
        if (RemainingCooldown <= 0f)
            return;

        RemainingCooldown = Mathf.Max(
            0f,
            RemainingCooldown - deltaTime);
    }
}
```

---

## 구조 다이어그램

```text
SOSkill Asset
├─ SkillType
├─ Damage
├─ CoolTime
├─ Range
├─ AreaRadius
├─ DoT Parameters
├─ AnimationName
└─ Icon
        │
        │ Reference
        ▼
SkillRuntime
├─ SkillData
├─ RemainingCooldown
├─ IsReady
├─ CooldownRatio
├─ StartCooldown()
└─ Tick(deltaTime)
```

---

## 핵심 설계 결정

### ScriptableObject에는 설정 데이터만 저장

ScriptableObject는 여러 객체가 동일한 Asset을 참조할 수 있습니다.
남은 쿨타임을 Asset에 직접 저장하면 하나의 슬롯에서 변경된 상태가
다른 슬롯에도 공유될 수 있습니다.

따라서 `SOSkill`에는 정적 설정 데이터만 저장하고,
각 슬롯은 별도의 `SkillRuntime` 인스턴스를 가지도록 구성했습니다.

### Runtime 상태를 UI의 단일 기준으로 사용

Skill UI가 별도의 쿨타임 타이머를 계산하지 않고
`SkillRuntime.CooldownRatio`를 직접 사용하도록 구성했습니다.

이를 통해 실제 스킬 사용 가능 상태와 UI 표시가
서로 다른 값을 기준으로 동작하지 않도록 했습니다.

---
## 예외 / 검증

- 동일한 `SOSkill` Asset을 여러 슬롯이 참조해도 각 슬롯의 쿨타임이 독립적으로 유지되는지 확인
- `Tick()` 반복 호출 후 `RemainingCooldown`이 0 아래로 내려가지 않는지 확인
- `coolTime == 0`일 때 `CooldownRatio`가 0으로 반환되는지 확인
- `RemainingCooldown <= 0`인 경우에만 `IsReady`가 `true`가 되는지 확인
- 실제 사용 가능 상태와 Skill UI가 동일한 `SkillRuntime` 값을 기준으로 동작하는지 확인

---

## 한계 및 개선 방향

- 현재 Runtime 상태는 쿨타임 중심이며 자원·충전 횟수·Stack 상태는 포함하지 않음
- 새로운 실행 방식의 SkillType을 추가하는 경우 SkillExecutor의 분기 로직도 함께 확장해야 함
- 슬롯 규모가 커지면 Runtime 생성과 수명주기를 전담하는 컨테이너 분리 검토

---

## 사용 기술

- Unity `ScriptableObject` — 스킬 설정 데이터 Asset 관리
- `CreateAssetMenu` — Unity Editor에서 Skill Asset 생성
- Plain C# Object — Runtime 상태 분리
- `Mathf.Max` — 쿨타임 값의 음수 방지
- Expression-bodied Property — `IsReady`, `CooldownRatio` 상태 제공
