> **코드 소유 범위:** 이 폴더의 제출 코드는 지원자가 직접 작성하거나 직접 수정한 파일입니다. 원본 프로젝트의 Scene, Prefab, Manager 및 데이터 의존성은 생략되어 있습니다.

# Dialogue System

게임 내 모든 대사, 선택지, 컷씬 전환을 담당하는 대화 관리 시스템입니다.  
비동기 이벤트 환경에서 대사가 충돌하지 않도록 Queue 기반으로 설계했습니다.

---

## 설계 의도

이벤트가 연쇄적으로 발생하는 게임에서는 대사가 진행 중일 때 다른 이벤트가 새 대사를 요청하는 상황이 자주 생깁니다. 처음에는 요청이 들어오면 즉시 교체했는데, 기존 대사가 갑자기 끊기거나 두 대사가 섞이는 문제가 있었습니다.

`Queue<string> dialogueQueue`를 도입해, 대사 진행 중이면 큐에 적재하고 현재 대사가 끝나면 자동으로 다음 대사를 시작하도록 했습니다. 이렇게 하면 어디서 `StartDialogue`가 호출되든 순서가 보장됩니다.

---

## 주요 구현

**대화 흐름 관리**

- `StartDialogue` — `isDialogueActive`가 `true`이면 큐에 추가하고 즉시 리턴합니다. `false`이면 `isDialogueActive = true`를 세운 뒤 첫 번째 라인을 `DisplayDialogueLine`으로 출력합니다.
- `DisplayDialogueLine` — 대화 타입(`DialogueType`)에 따라 플레이어 말풍선/NPC 말풍선/일반 대화창 중 하나를 활성화하고, 텍스트·이미지·스피커 이름을 세팅한 뒤 `TypeSentence` 코루틴을 시작합니다.
- `TypeSentence` — 한 글자씩 `typeSpeed` 간격으로 출력하는 타자기 효과입니다. `isFast` 모드에서는 전체 문장을 즉시 출력합니다.
- 대사가 모두 끝나면 큐를 확인해 다음 대사가 있으면 자동으로 이어서 시작합니다.

**다양한 대화 출력 모드**

`DialogueType` Enum으로 대화 UI 세트를 관리합니다.

- `PLAYER_TALKING` — 일반 대화창
- `PLAYER_BUBBLE` — 플레이어 말풍선 (3D 위치 기반 화면 좌표 추적)
- `NPC_BUBBLE` — NPC 말풍선 (동일 방식)
- 내적 독백, 컷씬 등 추가 모드

말풍선은 `LateUpdate`에서 `Camera.main.WorldToScreenPoint(transform.position + bubbleOffset)`으로 항상 캐릭터 위에 고정됩니다.

**선택지 시스템**

`DisplayChoices`로 선택지 버튼을 동적으로 생성합니다. 선택지가 활성화된 동안에는 Update의 스페이스바 입력을 차단해, 선택 전 대사가 임의로 넘어가지 않도록 합니다.

**한국어 조사 자동 치환 (KoreanJosa)**

플레이어 이름(`{PlayerName}`) 같은 변수 뒤에 붙는 조사가 이름 받침 여부에 따라 달라집니다. 유니코드 기반 종성 판별 알고리즘과 정규식(`Regex.Replace`)으로 `{PlayerName}이/가` 형태의 패턴을 실시간으로 처리합니다.

---

## 구조 다이어그램

```
DialogueManager
├─ dialogues (Dictionary<string, Dialogue>)
├─ choices   (Dictionary<string, Choice>)
├─ dialogueQueue (Queue<string>)           — 대사 충돌 방지
│
├─ StartDialogue(id)
│     ├─ isDialogueActive true  → Enqueue(id), return
│     └─ false → isDialogueActive = true → DisplayDialogueLine()
│
├─ DisplayDialogueLine()
│     ├─ DialogueType 기반 UI 세트 활성화
│     ├─ 텍스트/이미지/스피커 세팅
│     └─ StartCoroutine(TypeSentence())
│
├─ TypeSentence()  — 타자기 효과 코루틴
│     ├─ isFast → 전체 문장 즉시 출력
│     └─ 기본 → typeSpeed 간격 한 글자씩 출력
│
├─ DisplayChoices() → 선택지 버튼 동적 생성
│     └─ OnChoiceSelected() → 선택 이벤트 처리
│
└─ LateUpdate()
      ├─ PLAYER_BUBBLE → WorldToScreenPoint 위치 추적
      └─ NPC_BUBBLE    → WorldToScreenPoint 위치 추적
```

---

## 트러블슈팅

**대사 도중 새 대사 호출 시 기존 대화 끊김**

대사 진행 중 이벤트가 중첩으로 발생하면 `StartDialogue`가 재호출되어 기존 대사가 중단됐습니다. `Queue`를 도입해 진행 중이면 큐에 적재하고, 대사 종료 후 자동으로 이어서 실행하도록 수정했습니다.

```csharp
public void StartDialogue(string dialogueID)
{
    if (isDialogueActive)
    {
        dialogueQueue.Enqueue(dialogueID);
        return;
    }
    isDialogueActive = true;
    // ...
}
```

**선택지 활성 중 스페이스바로 대사가 넘어가는 문제**

`Update`에서 스페이스바 입력으로 대사를 넘기는 로직이 선택지가 표시된 상태에서도 동작했습니다. `choicesContainer[dialogueType.ToInt()].childCount > 0` 조건을 추가해, 선택지가 하나라도 있으면 키 입력을 처리하지 않도록 했습니다.

```csharp
private void Update()
{
    if (!isDialogueActive || choicesContainer[dialogueType.ToInt()].childCount > 0)
        return;
    if (Input.GetKeyDown(KeyCode.Space))
        OnDialoguePanelClick();
}
```

---

## 사용 기술

- `Queue<string>` — 대사 충돌 방지 순차 관리
- `TextMeshProUGUI` + Coroutine — 타자기 효과 텍스트 렌더링
- `Camera.main.WorldToScreenPoint` — 말풍선 3D→화면 좌표 추적
- `System.Text.RegularExpressions.Regex` — 한국어 조사 자동 치환
- `DontDestroyOnLoad` + 싱글턴 — 씬 전환 간 대사 데이터 유지
