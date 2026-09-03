> **코드 소유 범위:** 이 폴더의 제출 코드는 지원자가 직접 작성하거나 직접 수정한 파일입니다. 원본 프로젝트의 Scene, Prefab, Manager 및 데이터 의존성은 생략되어 있습니다.

# Event & Result System

게임 내 모든 이벤트와 결과 처리를 CSV 데이터 기반으로 관리하는 시스템입니다.  
이벤트 ID → 조건(Condition) 검사 → 결과(Result) 실행의 파이프라인 구조입니다.

---

## 설계 의도

게임에는 수백 개의 이벤트가 있습니다. 각 이벤트마다 코드를 작성하면 기획 변경 시마다 개발자가 코드를 수정해야 합니다.

`events.csv`와 `results.csv` 파일에 이벤트 정의와 결과 정의를 담고, 런타임에 파싱해 `Dictionary`로 관리하는 데이터 주도 설계를 채택했습니다. 기획자가 CSV만 수정하면 새 이벤트를 추가하거나 기존 이벤트를 바꿀 수 있고, 코드 수정 없이 동작합니다.

Result는 `ExecuteResultCoroutine`의 switch문으로 처리합니다. 대사 시작, 변수 증감, 페이드, 씬 이동 등 다양한 동작을 하나의 진입점에서 관리합니다. 씬별로 다른 오브젝트 동작이 필요한 경우 `IResultExecutable` 인터페이스로 오브젝트가 스스로 등록하고, ResultManager는 이름으로 호출합니다.

---
## 담당 범위
> **Contribution:** CSV·ID 기반 이벤트 구조,
> AND/OR 조건 판정, Instant/Sequential 실행 파이프라인 설계·구현 및 검증

---

## 주요 구현

**EventManager — CSV 파싱 및 이벤트 호출**

- `ParseEvents` — `events.csv`를 줄 단위로 파싱합니다. 하나의 EventID에 여러 EventLine(조건 집합 + 결과 집합)이 있을 수 있어, 이미 등록된 ID면 `AddEventLine`으로 추가합니다.
- `CallEvent(eventID)` — EventLine을 순서대로 확인합니다. 조건이 없으면 무조건 실행, `AND`/`OR` Logic에 따라 `CheckConditions_AND` / `CheckConditions_OR`을 호출합니다. 조건을 만족한 첫 번째 EventLine의 Result를 실행하고 즉시 반환합니다.
- **실행 모드 선택** — Result가 1개거나 모드가 `Instant`면 각 Result를 독립적인 코루틴으로 병렬 실행합니다. `Sequential`이면 `ExecuteResultsSequentially`로 하나씩 순차 실행합니다.
- **Function-wrapped Result** — `Result_StartDialogue`, `Result_Increment` 처럼 파라미터가 포함된 ID는 CSV에 등록하지 않고 코드에서 직접 처리합니다. `IsFunctionWrappedResult`로 이 패턴을 식별합니다.

**ResultManager — 결과 실행**

- `ExecuteResultCoroutine(resultID)` — switch문으로 Result ID를 처리합니다. 코루틴이기 때문에 `Result_StartDialogue`에서는 `while (isDialogueActive) yield return null`로 대사가 끝날 때까지 기다립니다. `Result_GetMemoryPuzzle`에서는 퍼즐 애니메이션이 완료될 때까지 대기합니다.
- `RegisterExecutable` / `InitializeExecutableObjects` — 씬 오브젝트(`Chair`, `Drawers` 등)가 Awake에서 자신을 등록합니다. 씬 전환 시 `InitializeExecutableObjects`로 전체 해제합니다.
- `MoveToRoomCoroutine` — `_isMovingRoom` 플래그로 중복 실행을 방지합니다. 페이드 아웃 → 플레이어/카메라 위치 이동 → 페이드 인 순서로 처리합니다.

---

## 구조 다이어그램

```
events.csv / results.csv
│
├─ EventManager.ParseEvents()
│     └─ events (Dictionary<string, GameEvent>)
│           EventLine: Logic + Conditions + Results + ExecutionMode
│
└─ ResultManager.ParseResults()
      └─ results (Dictionary<string, Result>)

CallEvent(eventID)
│
└─ EventLine 순회
      ├─ 조건 없음 → ExecuteResults()
      ├─ AND → CheckConditions_AND() → true → ExecuteResults(), return
      └─ OR  → CheckConditions_OR()  → true → ExecuteResults(), return

ExecuteResults(results, mode)
├─ Instant    → 각 Result 독립 코루틴 (병렬)
└─ Sequential → ExecuteResultsSequentially() (순차 yield)

ExecuteResultCoroutine(resultID)
├─ Result_StartDialogue* → StartDialogue() + while(isActive) 대기
├─ Result_Increment*     → IncrementVariable()
├─ Result_FadeIn/Out     → UIManager.OnFade() yield
├─ Result_GetMemoryPuzzle → ExecuteAction() + while(isPuzzleMoving) 대기
└─ Result_MoveToRoom*    → MoveToRoomCoroutine() (페이드 + 위치 이동)
```

---

## 트러블슈팅

**AND/OR 조건 처리 누락으로 잘못된 이벤트 분기**

초기에는 조건 목록을 순서대로 확인하다가 첫 번째 `true` 조건에서 결과를 실행했습니다. AND 조건인 경우에도 첫 번째 조건만 충족하면 실행되는 문제가 있었습니다. `CheckConditions_AND`와 `CheckConditions_OR`를 분리 구현하고, Logic 필드를 기반으로 분기하도록 수정했습니다.

```csharp
if (logic == "AND")
{
    if (CheckConditions_AND(conditions)) ExecuteResults(results, executionMode);
}
else if (logic == "OR")
{
    if (CheckConditions_OR(conditions)) ExecuteResults(results, executionMode);
}
```

**씬 전환 후 이전 씬 오브젝트 참조 잔류**

`ResultManager`는 `DontDestroyOnLoad`라 씬이 바뀌어도 유지됩니다. 이전 씬에서 등록된 `executableObjects`가 남아 있어, 씬 전환 후 해당 Result를 실행하면 NullReference가 발생했습니다. `RoomManager.Awake`에서 `InitializeExecutableObjects()`를 호출해, 씬 로드 시 이전 오브젝트 참조를 초기화하도록 했습니다.

**존재하지 않는 Result ID 참조 시 이벤트 전체 중단**

CSV에 오타로 잘못된 Result ID를 입력하면 `results` Dictionary에서 키를 찾지 못해 예외가 발생하거나 이벤트 전체가 조용히 실패했습니다. 키가 없으면 임시 Result 객체를 생성하고 `Debug.LogWarning`을 출력하도록 수정해, 나머지 Result는 정상 실행되고 문제 ID는 로그로 확인할 수 있게 했습니다.

---
## 한계 및 개선 방향
- 단순 CSV Split은 quoted comma 필드에 취약
- 존재하지 않는 ID와 중복 ID를 런타임 전에 검증하기 어려움
- Result 유형 증가 시 switch 분기 확대
- Sequential 실행의 취소/인터럽트 정책이 명시적이지 않음

---

## 사용 기술

- CSV 파싱 (`TextAsset.text.Split('\n')`) — 데이터 주도 이벤트 설계
- `Dictionary<string, GameEvent>` — O(1) 이벤트 조회
- Unity Coroutine (`IEnumerator`, `yield return`) — 비동기 결과 순차 실행
- `IResultExecutable` 인터페이스 — ResultManager와 씬 오브젝트 간 의존성 역전
- `DontDestroyOnLoad` + 싱글턴 — 씬 전환 간 이벤트 데이터 유지
