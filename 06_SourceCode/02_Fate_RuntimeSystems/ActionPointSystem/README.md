> **코드 소유 범위:** 이 폴더의 제출 코드는 지원자가 직접 작성하거나 직접 수정한 파일입니다. 원본 프로젝트의 Scene, Prefab, Manager 및 데이터 의존성은 생략되어 있습니다.

# Action Point System

행동력(하트)과 날짜 전환 시스템입니다.  
공통 로직을 추상 클래스로 정의하고, 방(Room)별 고유 규칙을 상속 구조로 확장했습니다.

---

## 설계 의도

방마다 행동력 규칙이 다릅니다. Room1은 기본 소모 구조, Room2는 회복제 아이템과 곰인형 이벤트로 행동력 최대치가 바뀝니다.

처음에는 하나의 클래스에서 `if (isRoom2)` 분기로 처리했는데, 규칙이 늘어날수록 코드가 복잡해졌습니다. 그래서 공통 로직(`CreateHearts`, `DecrementActionPoint`, `RefillHeartsOrEndDay`, `nextMorningDay`)을 추상 메서드로 정의하고, 방별 클래스에서 오버라이드하는 구조로 리팩터링했습니다. 이후 Room2에 회복제·곰인형 규칙을 추가할 때도 Room1 코드를 건드리지 않고 확장할 수 있었습니다.

---
## 담당 범위
> **Contribution:** 행동력·날짜 진행 시스템 설계·구현 및
> 반복 입력·상태 예외 검증

---

## 주요 구현

**ActionPointManager (Abstract)**

- `CreateActionPointsArray(int actionPointsPerDay)` — 2D 배열로 날짜별 행동력 값을 미리 계산해 저장합니다. 날짜가 바뀔 때마다 계산하는 대신, 인덱스로 O(1)에 조회합니다.
- 날짜 전환 애니메이션 — 기어 회전, 페이지 넘김, 페이드 효과를 코루틴으로 타임라인을 구성합니다. `Quaternion.Slerp`으로 페이지가 자연스럽게 뒤집히고, `ControlImagesAlpha`로 기어 UI가 페이드 인/아웃됩니다.
- `TakeRest()` — 침대 오브젝트에서 휴식 시 남은 행동력을 강제로 소진하고 다음날로 전환합니다.

**Room1ActionPointManager**

- `DecrementActionPoint()` — 행동력 1 소모 시 하트 파괴 애니메이션을 트리거하고, 모두 소진되면 귀가 이벤트(`EventRoom1HomeComing`)를 호출합니다.
- Room2 진입 전 `InitActionPointVariables()`로 날짜·행동력 변수를 초기화합니다.

**Room2ActionPointManager**

- `EatEnergySupplement()` — 회복제 아이템 사용 시 현재 하트를 전부 제거하고, `isEatenEnergySupplement` 플래그를 세운 뒤 `CreateHearts()`를 다시 호출해 하트 2개를 추가합니다.
- 곰인형 이벤트 발생 시 `actionPointsPerDay`를 5에서 7로 바꾸고 `actionPointsArray`를 재생성합니다.

---

## 구조 다이어그램

```
ActionPointManager  (Abstract)
├─ CreateActionPointsArray()      — 날짜별 행동력 2D 배열 사전 계산
├─ CreateHearts()                 — abstract
├─ DecrementActionPoint()         — abstract
├─ RefillHeartsOrEndDay()         — abstract
├─ nextMorningDay()               — abstract
├─ StartNextDayUIChange()         — 날짜 전환 애니메이션 타임라인
│     ├─ ScaleAndMovingNextDayUI()  — DayUI 확대/축소 (Lerp)
│     ├─ TurnNextDayUIBack()        — 페이지 넘김 (Quaternion.Slerp)
│     └─ StartRotateGearsAndClockHands() — 기어 회전 + Alpha 페이드
└─ TakeRest()                     — 강제 날짜 전환 (침대 오브젝트 연동)

Room1ActionPointManager  extends ActionPointManager
└─ 기본 로직: 행동력 소진 → 귀가 이벤트 → 엔딩 or 다음날
└─ InitActionPointVariables(): Room2 진입 전 변수 초기화

Room2ActionPointManager  extends ActionPointManager
└─ EatEnergySupplement(): 하트 +2 회복
└─ 곰인형 이벤트: actionPointsPerDay 5 → 7, 배열 재생성
```

---

## 트러블슈팅

**퍼즐 연속 클릭 시 행동력 중복 감소**

퍼즐 오브젝트를 빠르게 반복 클릭하면 `DecrementActionPoint()`가 중복 호출되어 하트가 한 번에 2~3개 감소했습니다. `heartParent.transform.childCount < 1` 조건으로 하트가 이미 없는 상태에서는 즉시 리턴하도록 방어 처리했습니다.

```csharp
public override void DecrementActionPoint()
{
    if (heartParent.transform.childCount < 1)
        return;
    // ...
}
```

**대화·조사 중 날짜 전환 연출 중복 실행**

대사 출력 중 행동력이 0이 되면 `RefillHeartsOrEndDay()`가 즉시 호출되어 날짜 전환 연출과 대사가 겹쳤습니다. `isDialogueActive`와 `isInvestigating` 플래그를 확인한 뒤, 두 상태 중 하나라도 활성이면 `refillHeartsOrEndDayState = true`로 실행을 예약하고 상태가 해제될 때 실행하도록 수정했습니다.

```csharp
if (actionPoint % actionPointsPerDay == 0)
{
    bool isDialogueActive = DialogueManager.Instance.isDialogueActive;
    bool isInvestigating  = RoomManager.Instance.GetIsInvestigating();
    if (!isDialogueActive && !isInvestigating)
        RefillHeartsOrEndDay();
    else
        refillHeartsOrEndDayState = true;
}
```

**날짜 전환 애니메이션 재진입 문제**

`TurnNextDayUIBack()` 코루틴이 완료되기 전에 다시 호출되면 페이지가 두 번 넘겨지는 문제가 있었습니다. `_isTurningBack` 플래그를 추가해 코루틴 진입 시 즉시 가드하고, 완료 후 해제하도록 처리했습니다.

```csharp
protected IEnumerator TurnNextDayUIBack()
{
    if (_isTurningBack) yield break;
    _isTurningBack = true;
    // ...
    _isTurningBack = false;
}
```

---
## 한계 및 개선 방향
- 내부 행동력 검증에 하트 UI 상태가 일부 포함되어 Model/View 결합
- Room 증가 시 상속 클래스와 override 범위 확대 가능
- Coroutine 취소 정책이 분산
- 향후 행동력 State Model + Event 기반 UI,
  Room Strategy 또는 State Machine 구조 검토

---

## 사용 기술

- 추상 클래스 + 상속 구조로 공통/개별 로직 분리
- Unity Coroutine으로 날짜 전환 애니메이션 타임라인 구성
- `Quaternion.Slerp` — 페이지 넘김 회전 보간
- `Vector3.Lerp` — DayUI 위치·스케일 보간
- `Image.color.a` 코루틴 제어 — 기어 UI 페이드 인/아웃
- UIManager 연동 — 하트 UI, 날짜 텍스트, 기어·시계 이미지 상태 제어
