> **코드 소유 범위:** 이 폴더의 제출 코드는 지원자가 직접 작성하거나 직접 수정한 파일입니다. 원본 프로젝트의 Scene, Prefab, Manager 및 데이터 의존성은 생략되어 있습니다.

# Interaction Systems

게임 내 모든 상호작용 오브젝트의 공통 구조와 개별 동작 구현 코드입니다.
`EventObject`를 공통 베이스로, `Chair`·`Drawers` 등 개별 오브젝트가 확장하는 구조입니다.

---

## 설계 의도

게임에는 수십 개의 상호작용 오브젝트가 있습니다. 각 오브젝트마다 클릭 처리, 이벤트 호출, 조사창 분기를 개별 구현하면 코드 중복이 심해집니다.

`EventObject`를 공통 베이스로 설계해, 클릭 시 이벤트 호출 흐름을 단일 진입점으로 통합했습니다. 개별 오브젝트(`Chair`, `Drawers`)는 `EventObject`를 상속하고, ResultManager에서 동작을 주입할 수 있도록 `IResultExecutable` 인터페이스를 구현합니다.

---
## 담당 범위
> **Contribution:** 공통 상호작용 진입점,
> Chair·Drawers 상태 변화, Result 연동 및 Save/Load 복원 구현·수정

---

## 주요 구현

**EventObject (Base)**

- `OnMouseDown` — `isInquiry` 플래그에 따라 분기합니다. `isInquiry == true`면 `Event_Inquiry`(조사창)를 호출하고, `false`면 `eventId` 이벤트를 직접 호출합니다.
- `Awake`에서 `GameManager.AddEventObject(this)`로 자동 등록합니다.
- `SetCurrentLockObjectCanvasGroup` — 대화 중에는 CanvasGroup의 `blocksRaycasts`를 `false`로 설정해 대화 도중 잠금 패널이 클릭되지 않도록 합니다.

**Chair**

- `Awake`에서 `ResultManager.RegisterExecutable($"Chair{sideNum}", this)`로 이름 기반 등록을 합니다. ResultManager가 `"Chair1"` 같은 이름으로 직접 실행 요청을 보낼 수 있습니다.
- `ExecuteAction` — `ChairMoved` 게임 변수를 읽어 이동 방향(원위치 ↔ 이동 위치)을 결정하고, `MoveChair` 코루틴을 시작합니다.
- `MoveChair` — `Vector2.Lerp`로 `moveDuration`(0.3초) 동안 부드럽게 이동합니다. 이동 중에는 `isMoving = true`, 완료 후 `false`로 상태를 관리합니다.
- `OnEnable` — 씬 복귀 또는 세이브 로드 시 `ChairMoved` 변수를 읽어 의자 위치를 즉시 복원합니다.

**Drawers**

- 윗칸(`isUpDrawer == true`)과 아랫칸을 독립 인스턴스로 분리해, 각각 독립적으로 열림/닫힘 상태를 관리합니다.
- `showDrawersInSide` — 열림/닫힘 상태에 따라 사이드별 오브젝트를 활성/비활성 전환합니다.
- `ExecuteActionMoveDrawer` — `UpDrawerMoved` / `DownDrawerMoved` 변수를 읽어 이동 방향을 결정하고, Lerp 코루틴으로 서랍을 이동시킨 뒤 `InverseVariable`로 상태를 토글합니다.

---

## 구조 다이어그램

```
EventObject  (Base)
├─ eventId, sideNum, isInquiry
├─ OnMouseDown()
│     ├─ isInquiry true  → Event_Inquiry (조사창)
│     └─ isInquiry false → eventId 이벤트 직접 호출
└─ GameManager.AddEventObject(this)

Chair  extends EventObject, IResultExecutable
├─ ResultManager.RegisterExecutable("Chair{sideNum}", this)
├─ ExecuteAction()  → ChairMoved 변수 기반 방향 결정
├─ MoveChair()      → Vector2.Lerp 이동 (isMoving 가드)
└─ OnEnable()       → 세이브 상태 위치 복원

Drawers  extends EventObject, IResultExecutable
├─ ResultManager.RegisterExecutable("{type}{parentObjectName}Drawers", this)
├─ ExecuteAction() → ToggleDoors()
│     ├─ showDrawersInSide()           — 사이드별 오브젝트 전환
│     └─ ExecuteActionMoveDrawer()     — Lerp 이동 + InverseVariable
└─ isUpDrawer로 윗칸/아랫칸 독립 관리
```

---

## 트러블슈팅

**의자 이동 중 중복 클릭 시 목표 위치 재설정 문제**

`MoveChair` 코루틴이 실행 중인데 다시 클릭하면 `ExecuteAction`이 재호출되어 목표 위치가 바뀌고, 의자가 중간에 멈추거나 반대 방향으로 이동했습니다. `isMoving` 플래그와 `GameManager.GetIsBusy()` 체크를 `OnMouseDown`에 추가해 이동 중에는 입력을 차단했습니다.

```csharp
public new void OnMouseDown()
{
    bool isBusy = GameManager.Instance.GetIsBusy();
    if (isMoving || isBusy) return;
    base.OnMouseDown();
}
```

**세이브 후 재로드 시 의자 위치 초기화**

`Awake`에서 `originalPosition`을 `rectTransform.anchoredPosition`으로 초기화하는데, 세이브 로드 후 씬이 다시 열리면 항상 초기 위치로 돌아갔습니다. `OnEnable`에서 `ChairMoved` 게임 변수를 읽어 이미 이동된 상태라면 `movedPositions[sideNum]`을 즉시 적용하도록 수정했습니다.

```csharp
private void OnEnable()
{
    chairMoved = (bool)GameManager.Instance.GetVariable("ChairMoved");
    rectTransform.anchoredPosition = chairMoved ? movedPositions[sideNum] : originalPosition;
}
```
---
## 한계 및 개선 방향
- GameManager의 변수 이름과 Result 등록 문자열에 의존
- 오브젝트 종류 증가 시 상속 클래스와 문자열 ID 관리 복잡도 증가
- 개별 MonoBehaviour에 이동·상태·표현 책임이 함께 존재
- 공통 State Restore Interface와 ID Registry 도입 검토

---

## 사용 기술

- 상속 구조 (`EventObject` 베이스) — 공통 클릭 처리 단일화
- `IResultExecutable` 인터페이스 — ResultManager와 오브젝트 간 의존성 역전
- `Vector2.Lerp` + Coroutine — 부드러운 이동 애니메이션
- `GameManager` 변수 연동 — 세이브/로드 대응 상태 복원
