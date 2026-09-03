> **코드 소유 범위:** 이 폴더의 제출 코드는 지원자가 직접 작성하거나 직접 수정한 파일입니다. 원본 프로젝트의 Scene, Prefab, Manager 및 데이터 의존성은 생략되어 있습니다.

# Puzzle Systems

반짇고리 퍼즐 시스템 구현 코드입니다.  
플레이어는 드래그 앤 드롭으로 비즈를 올바른 위치에 배치해 퍼즐을 클리어할 수 있습니다.

---

## 설계 의도

퍼즐 오브젝트(`SewingBoxPuzzle`)와 드래그 가능한 비즈 오브젝트(`SewingBoxBead`)를 역할에 따라 분리했습니다. `SewingBoxPuzzle`은 정답 관리와 정답 판정만 담당하고, `SewingBoxBead`는 드래그·드롭·이동 애니메이션만 담당합니다.

정답 테이블은 `Dictionary<int, int>`로 관리합니다. 비즈 번호(key)에 대한 정답 열(column) 위치(value)를 저장해, 정답 확인 시 순서대로 비교하면 됩니다.

---
## 담당 범위
> **Contribution:** 반짇고리 Drag & Drop,
> DropZone·Row Constraint와 Dictionary 기반 정답 판정 구현·검증

---

## 주요 구현

**SewingBoxBead — 드래그 앤 드롭**

Unity UI 이벤트 시스템(`IBeginDragHandler`, `IDragHandler`, `IEndDragHandler`)을 구현합니다.

- `OnDrag` — `RectTransformUtility.ScreenPointToLocalPointInRectangle`로 로컬 좌표를 계산합니다. x축은 고정하여 비즈가 열(column) 방향으로만 움직이도록 제한합니다.
- `GetClosestDropZone` — 모든 드롭존을 순회하며, `RectTransformUtility.WorldToScreenPoint`로 월드 좌표를 스크린 좌표로 변환한 뒤 `Vector2.Distance`로 가장 가까운 드롭존을 탐색합니다.
- `GetValidDropZone` — Row 1에서는 두 비즈가 같은 열에 겹치지 않도록 허용 열을 강제로 조정합니다. 비즈 1은 비즈 2보다 왼쪽, 비즈 2는 비즈 1보다 오른쪽에만 배치할 수 있습니다.
- `SmoothMoveToParent` — `Vector3.Lerp`로 비즈를 목표 드롭존 위치까지 부드럽게 이동시킨 뒤, 이동 완료 후 `SetParent`로 부모를 교체합니다. 이동 완료 전 부모를 바꾸면 좌표계가 달라져 위치가 튀는 문제가 있어, 이동 마지막에만 처리합니다.

**SewingBoxPuzzle — 정답 판정**

- `settingBeadsAnswer` — 비즈 번호별 정답 열 위치를 Dictionary에 초기화합니다.
- `CheckBeadsAnswer` — 모든 비즈의 `currentPositionNumber`를 정답 테이블과 비교합니다. 하나라도 틀리면 즉시 `false`를 반환합니다.
- `isBeadsCorrect` 플래그로 정답 후 중복 판정을 방지합니다.

---

## 구조 다이어그램

```
SewingBoxPuzzle
├─ BeadsAnswer (Dictionary<int, int>)  — 비즈 번호 : 정답 열
├─ CompareBeads()
│     ├─ ProhibitInput()               — 무한 입력 방지
│     ├─ isBeadsCorrect 가드
│     └─ CheckBeadsAnswer()            — 전체 비즈 정답 비교
└─ EventManager.CallEvent("EventSewingBoxB")

SewingBoxBead  (IBeginDragHandler, IDragHandler, IEndDragHandler)
├─ OnBeginDrag()  — originalParent 저장, ProhibitInput()
├─ OnDrag()       — 로컬 좌표 계산, x축 고정
├─ OnEndDrag()
│     ├─ GetClosestDropZone()   — 거리 기반 최근접 탐색
│     ├─ GetValidDropZone()     — 행 제약 조건 검사
│     └─ SmoothMoveToParent()  — Lerp 보간 이동 후 SetParent
└─ currentPositionNumber        — 열(column) 위치 상태 관리
```

---

## 트러블슈팅

**비즈 겹침 문제 — 드롭존 제약 조건 부재**

처음에는 `GetClosestDropZone`만으로 드롭존을 결정했습니다. 빠르게 드래그하거나 특정 순서로 드롭하면 두 비즈가 같은 드롭존에 겹쳐서 배치되는 문제가 있었습니다.

`GetValidDropZone`을 별도로 추가해, Row 1의 비즈는 서로의 열 위치를 비교한 뒤 허용 열을 강제로 결정하도록 수정했습니다.

```csharp
RectTransform GetValidDropZone(RectTransform target)
{
    if (beadRow != 1) return target;

    int targetCol      = ParseColumn(target.name);
    int otherCol       = FindBeadColumn(otherBeadNumber);
    int allowedCol     = -1;

    if (beadNameNumber == 1 && targetCol >= otherCol)
        allowedCol = otherCol - 1;   // 비즈1은 비즈2 왼쪽에만
    else if (beadNameNumber == 2 && targetCol <= otherCol)
        allowedCol = otherCol + 1;   // 비즈2는 비즈1 오른쪽에만

    if (allowedCol != -1)
    {
        foreach (var zone in dropZones)
            if (ParseColumn(zone.name) == allowedCol) return zone;
        return null;
    }
    return target;
}
```

**SetParent 타이밍 문제**

`SmoothMoveToParent`에서 이동 중간에 `SetParent`를 호출하면 좌표계가 바뀌어 비즈 위치가 순간적으로 튀었습니다. Lerp 루프가 끝난 뒤 최종 위치를 정확히 설정하고, 그 다음에 `SetParent`를 호출하도록 순서를 수정했습니다.

```csharp
IEnumerator SmoothMoveToParent(RectTransform targetParent, float duration)
{
    // ...Lerp 루프...
    rectTransform.position = endPos;          // 1. 최종 위치 확정
    transform.SetParent(targetParent);        // 2. 부모 교체
    rectTransform.localPosition = Vector3.zero;
}
```

---
## 한계 및 개선 방향
- DropZone을 순회해 최근접 위치를 찾는 구조
- Row1의 특수 배치 제약이 코드에 직접 포함
- DropZone 이름 파싱에 위치 규칙이 의존할 가능성
- 다양한 화면 비율의 자동 입력 테스트 미구현
### 개선 방향
- IPlacementRule 같은 Validation Strategy로 배치 규칙 분리
---

## 사용 기술

- Unity UI 이벤트 시스템 (`IBeginDragHandler`, `IDragHandler`, `IEndDragHandler`)
- `RectTransformUtility.ScreenPointToLocalPointInRectangle` — 스크린 → 로컬 좌표 변환
- `RectTransformUtility.WorldToScreenPoint` — 월드 → 스크린 좌표 변환 (드롭존 거리 계산)
- `Vector3.Lerp` + Coroutine — 비즈 이동 보간 애니메이션
- `Dictionary<int, int>` — 정답 테이블 관리
