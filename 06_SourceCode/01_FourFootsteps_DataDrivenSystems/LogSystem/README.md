> **코드 소유 범위:** 이 폴더의 제출 코드는 지원자가 직접 작성하거나 직접 수정한 파일입니다. 원본 프로젝트의 Scene, Prefab, Manager 및 데이터 의존성은 생략되어 있습니다.

# Log Tracking System

학술 연구 실험을 위해 플레이어의 엔딩 결과와 게임 내 상태를  
Google Spreadsheet로 자동 수집하는 시스템입니다.

> 이 시스템은 학술저널 논문(제1저자, 2026.05) 실험 데이터 수집을 위해  
> 직접 설계하고 구현했습니다. 출시 빌드에는 포함되지 않습니다.

---

## 설계 의도

실험에서 수집해야 하는 데이터는 플레이어가 어떤 엔딩에 도달했는지, 어떤 퍼즐을 수집했는지, 책임감 점수가 얼마인지였습니다. 간단해 보이지만 세 가지 문제를 해결해야 했습니다.

첫째, **중복 전송**입니다. 같은 엔딩 씬을 여러 번 로드하거나 앱을 재시작하면 로그가 중복으로 쌓일 수 있습니다. Run 단위 가드를 클라이언트와 서버 양쪽에 모두 걸어 해결했습니다.

둘째, **로그 소실**입니다. 전송 중 네트워크가 끊기면 데이터가 사라집니다. `PlayerPrefs`에 큐를 직렬화해 보관하고, 재시작 후 자동으로 재전송하도록 설계했습니다.

셋째, **세이브 파일에서 데이터 추출**입니다. `GameManager.Variables`에 바로 접근하면 씬 전환 타이밍에 따라 값이 아직 로드되지 않을 수 있습니다. 세이브 파일(`GameData.json`)을 직접 읽어 값을 추출하는 방식을 택했습니다.

---

## 클래스별 역할

**`EndingLogQueueManager`** — 전송 큐 및 HTTP 통신

게임 전체에서 로그 전송을 담당하는 싱글턴입니다. `DontDestroyOnLoad`로 씬 전환에도 유지됩니다.

- **큐 직렬화/역직렬화** — `JsonUtility`는 `List<T>`를 직접 직렬화하지 못합니다. 배열 래퍼 클래스(`PendingLogWrapper`)로 감싸 `PlayerPrefs`에 JSON 문자열로 보관합니다. 앱을 종료해도 큐가 유지됩니다.
- **`FlushQueue`** — `isSending` 플래그로 코루틴 중복 실행을 방지합니다. 큐의 첫 번째 페이로드부터 순서대로 전송하고, 성공하면 큐에서 제거 후 저장합니다. 실패하면 `retryCountThisSession`을 증가시켜 세션 내 최대 3회까지만 재시도합니다. 무한 재시도로 사용자 체감에 영향을 주지 않기 위한 제한입니다.
- **`PostPayload`** — `UnityWebRequest`로 HTTP POST를 보냅니다. 응답 본문에 `"ok":true`가 포함되어 있는지 확인해 성공 여부를 판단합니다. 네트워크 에러와 서버 응답 에러를 별도로 처리합니다.
- **`GetOrCreateUuid`** — `PlayerPrefs`에 기기 고유 UUID를 저장합니다. 재설치하지 않는 한 동일 기기에서 동일 키가 유지됩니다.
- **`StartNewRun`** — 새 게임 시작 시 `Guid.NewGuid()`로 새 Run ID를 생성합니다. 같은 기기에서 여러 번 플레이해도 회차별로 구분됩니다.

---

**`EndingLogReporter`** — 엔딩 진입 시 페이로드 조립

- **Run 단위 중복 가드** — `SENT_ENDING_{runId}_{endingType}` 키를 `PlayerPrefs`에서 확인합니다. 이미 전송했으면 즉시 리턴합니다. 씬명만으로 가드하면 같은 기기에서 두 번째 플레이 때 동일 엔딩이 차단됩니다. Run ID를 포함한 키 방식으로 회차별로 독립 전송되도록 했습니다.
- **세이브 파일 직접 파싱** — `JsonFieldExtractor.TryGetString`, `TryGetInt`로 `GameData.json`에서 `PlayerName`, `YourCatName`, `ResponsibilityScore`를 추출합니다. `GameManager`를 거치지 않아 씬 전환 타이밍에 독립적입니다.
- **`eventId` 생성** — `{playerKey}_{runId}_{eventType}_{DateTime.UtcNow.Ticks}` 조합으로 고유 ID를 만듭니다. 서버에서 이 ID를 기준으로 중복 행 삽입을 차단합니다.

---

**`JsonFieldExtractor`** — 경량 JSON 필드 추출 유틸

Newtonsoft.Json 없이 세이브 파일에서 특정 키의 값만 꺼내기 위한 정적 유틸 클래스입니다. 키 토큰을 찾아 콜론 이후 값을 추출합니다. `"123"` 형태의 문자열 숫자와 `123` 형태의 raw 숫자를 모두 처리합니다.

---

**`MemoryPuzzleStateExtractor`** — 퍼즐 상태 JSON 추출 및 정규화

`GameData.json`에서 `MemoryPuzzleStates`를 추출합니다. 세이브 시스템 버전에 따라 이 값이 객체(`{}`)로 저장될 수도, 이스케이프된 문자열(`"{\"0\":true}"`)로 저장될 수도 있어 두 케이스를 모두 처리합니다.

- **`SliceJsonObject`** — 중괄호 depth 카운팅으로 객체 블록을 잘라냅니다.
- **`SliceJsonString`** — 이스케이프 문자를 고려한 문자열 슬라이싱을 합니다.
- **`NormalizeObjectJson`** — 게임 내부의 0-based 인덱스를 논문 데이터의 퍼즐 번호(1-based)로 변환합니다.

---

**`ReportEndingOnStart`** — 씬 진입 트리거 컴포넌트

엔딩 씬의 GameObject에 붙이는 컴포넌트입니다. `Start()`에서 `EndingLogReporter.ReportEnding(endingId)`를 호출합니다. 어떤 엔딩인지는 Inspector에서 `endingId`를 설정합니다. 코드 수정 없이 엔딩 씬마다 다른 ID를 지정할 수 있습니다.

---

**`FourFootsteps_logTracking.js`** — Google Apps Script 서버

- `doPost` — API Key 인증 후 `eventId` 중복 검사를 통과한 페이로드만 Spreadsheet에 `appendRow`합니다.
- `isDuplicateEventId_` — B열(`eventId`) 전체 범위를 `getRange`로 가져와 선형 탐색합니다. 중복이면 행 삽입 없이 `{ok:true, duplicated:true}`로 응답합니다. 클라이언트가 전송 성공으로 인식해 큐에서 제거하도록 유도합니다.

---

## 전체 흐름

```
[엔딩 씬 로드]
│
ReportEndingOnStart.Start()
└─ EndingLogReporter.ReportEnding(endingId)
      │
      ├─ PlayerPrefs: "SENT_ENDING_{runId}_{scene}" 확인
      │     이미 전송됨 → return
      │
      ├─ JsonFieldExtractor: GameData.json 직접 파싱
      │     playerName, catName, responsibilityScore 추출
      │
      ├─ MemoryPuzzleStateExtractor: MemoryPuzzleStates 추출
      │     객체({}) or 이스케이프 문자열 → 정규화 JSON (0-based → 1-based)
      │
      ├─ EndingLogPayload 구성
      │     playerKey(UUID), runId, eventType, endingType,
      │     memoryPuzzleStatesJson, responsibilityScore
      │     eventId = {playerKey}_{runId}_{eventType}_{Ticks}
      │
      └─ EndingLogQueueManager.EnqueueAndSend(payload)
            │
            ├─ Enqueue: PlayerPrefs 큐에 직렬화 저장
            │     maxQueueSize(20) 초과 시 오래된 항목 제거
            │
            └─ FlushQueue() Coroutine
                  │  isSending 가드로 중복 실행 방지
                  │
                  while (queue.Count > 0)
                  ├─ PostPayload() → UnityWebRequest POST
                  │     Content-Type: application/json
                  │     응답 "ok":true 확인
                  │
                  ├─ 성공 → queue.RemoveAt(0), SaveQueue(), retryCount = 0
                  └─ 실패 → retryCount++
                           maxRetryPerSession(3) 초과 → break
                           (다음 앱 시작 시 Start()에서 FlushQueue 재호출)

[Google Apps Script]
doPost(e)
├─ API Key 인증 (공유 키 비교)
├─ eventId 필수값 확인
├─ isDuplicateEventId_(): B열 전체 선형 탐색
│     중복 → {ok:true, duplicated:true}  (클라이언트 큐 제거 유도)
└─ sheet.appendRow(row) → {ok:true}
```

---

## 트러블슈팅

**동일 회차 중복 로그 전송**

엔딩 씬이 여러 번 로드되면 `ReportEnding`이 재호출되어 같은 회차 로그가 중복으로 Spreadsheet에 쌓였습니다.

처음에는 씬명만으로 가드했습니다(`SENT_ENDING_{sceneName}`). 이렇게 하면 같은 기기에서 두 번째 플레이 때 동일 엔딩에 도달해도 전송이 영구히 차단되는 문제가 생겼습니다. Run ID를 포함한 키(`SENT_ENDING_{runId}_{sceneName}`)로 바꿔 회차별로 독립 전송되도록 수정했습니다. 서버의 `isDuplicateEventId_`가 두 번째 방어선 역할을 합니다.

```csharp
// EndingLogReporter.cs — Run 단위 가드
string sentKey = $"{SENT_ENDING_PREFIX}{runId}_{scene}";
if (PlayerPrefs.GetInt(sentKey, 0) == 1) return;
// ...
PlayerPrefs.SetInt(sentKey, 1);
PlayerPrefs.Save();
```

**네트워크 불안정 시 로그 소실**

전송 실패 시 그대로 폐기하면 실험 데이터가 손실됩니다. `PlayerPrefs`에 큐를 직렬화해 저장하고, `FlushQueue`가 전송 성공 후에만 큐에서 제거하도록 했습니다. 앱을 재시작해도 `Start()`에서 `FlushQueue`가 자동 호출되어 미전송 항목이 재전송됩니다.

**`JsonUtility`로 `List<T>` 직렬화 불가**

`JsonUtility.ToJson(list)`는 지원되지 않습니다. `PendingLogWrapper { EndingLogPayload[] items }` 래퍼 클래스로 감싸 직렬화하는 방식으로 우회했습니다.

```csharp
// EndingLogQueueManager.cs
private void SaveQueue(List<EndingLogPayload> list)
{
    var wrapper = new PendingLogWrapper { items = list.ToArray() };
    string json = JsonUtility.ToJson(wrapper);
    PlayerPrefs.SetString(QUEUE_KEY, json);
    PlayerPrefs.Save();
}
```

**`MemoryPuzzleStates` 저장 형식 불일치**

세이브 시스템 버전에 따라 `MemoryPuzzleStates`가 객체(`{}`)로 저장될 때도 있고, 이스케이프된 문자열(`"{\"0\":true}"`)로 저장될 때도 있었습니다. `MemoryPuzzleStateExtractor`에서 첫 번째 비공백 문자가 `{`이면 객체로, `"`이면 문자열로 분기해 두 케이스를 모두 처리합니다.

---

## 수집 데이터 항목

| 컬럼 | 설명 |
|------|------|
| `timestamp` | 서버 기록 시각 |
| `eventId` | 중복 방지용 고유 ID (`playerKey_runId_eventType_ticks`) |
| `playerKey` | 익명 기기 UUID |
| `runId` | 플레이 회차 구분자 (`Guid`) |
| `eventType` | 로그 종류 (`Ending` 등) |
| `playerName` | 플레이어 입력 이름 |
| `catName` | 고양이 이름 |
| `endingType` | 도달한 엔딩 종류 |
| `memoryPuzzleStatesJson` | 퍼즐 수집 상태 (정규화 JSON, 1-based 인덱스) |
| `responsibilityScore` | 책임감 점수 |

---

## 사용 기술

- `UnityWebRequest` + Coroutine — 비동기 HTTP POST 통신
- `PlayerPrefs` — 큐 직렬화 영속 저장, Run 단위 중복 가드
- `JsonUtility` + 배열 래퍼 클래스 — `List<T>` 직렬화 우회
- `System.Guid` — Run ID 및 익명 UUID 생성
- `DateTime.UtcNow.Ticks` — 고유 `eventId` 생성
- Google Apps Script (`doPost`) — API Key 인증, `eventId` 중복 검사, Spreadsheet 행 삽입
- 수동 JSON 파싱 (`JsonFieldExtractor`, `MemoryPuzzleStateExtractor`) — 외부 라이브러리 없이 세이브 파일 필드 추출
