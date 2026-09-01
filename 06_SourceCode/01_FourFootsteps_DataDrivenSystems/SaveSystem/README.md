> **코드 소유 범위:** 이 폴더의 제출 코드는 지원자가 직접 작성하거나 직접 수정한 파일입니다. 원본 프로젝트의 Scene, Prefab, Manager 및 데이터 의존성은 생략되어 있습니다.

# Save System

게임 데이터의 저장·로드·초기화를 담당하는 시스템입니다.  
강제 종료 상황에서도 데이터가 파손되지 않도록 Atomic Write 방식을 설계했고,  
버전 간 타입 불일치를 자동으로 복구하는 Fallback 로직을 구현했습니다.

---

## 설계 의도

처음에는 `File.WriteAllText`로 직접 저장했습니다. 저장 도중 게임이 강제 종료되면 파일이 빈 상태로 남아 세이브 데이터 전체가 손실됩니다. 실제로 테스트 중 이 문제가 발생해 Atomic Write 방식으로 전환했습니다.

타입 보존도 문제였습니다. `Dictionary<string, object>`를 그대로 직렬화하면 역직렬화 시 타입 정보가 손실되어 `int`가 `long`으로, `Dictionary<int, bool>`이 `string`으로 돌아오는 경우가 생겼습니다. 변수 값과 타입 정보를 분리해 두 개의 JSON으로 저장하는 방식으로 해결했습니다.

---

## 주요 구현

**Atomic Write**

임시 파일(`.tmp`)에 먼저 쓰고, `File.Replace`로 원자적으로 교체합니다. 교체 전 기존 파일은 `.bak`으로 백업됩니다. `File.Replace`가 실패하면(일부 환경에서 발생) `File.Delete` → `File.Move`로 폴백합니다.

```csharp
private static void AtomicWrite(string path, string content)
{
    string tmp = path + ".tmp";
    File.WriteAllText(tmp, content);      // 1. 임시 파일에 쓰기
    try {
        File.Replace(tmp, path, path + ".bak", true);  // 2. 원자적 교체 + 백업
    } catch {
        if (File.Exists(path)) File.Delete(path);
        File.Move(tmp, path);             // 3. 폴백
    }
}
```

**타입 정보 보존 직렬화 (`SaveData`)**

`variablesToJson`에는 변수 값을 문자열로 저장하고, `variablesTypeToJson`에는 각 변수의 타입명(`"int"`, `"bool"`, `"dict:int-bool"`)을 함께 저장합니다. 로드 시 타입 정보를 먼저 읽어 변환합니다.

`Dictionary<int, bool>` 타입은 `JsonConvert.SerializeObject`로 JSON 문자열(`{"0":true,"1":false,...}`)로 저장하고, `typeVariables`에 `"dict:int-bool"`로 표시합니다.

**`NormalizeType` — 레거시 타입명 정규화**

구버전에서 `System.Int32`, `System.Boolean` 같은 전체 타입명으로 저장된 경우를 처리합니다. switch문으로 모든 레거시 타입명을 내부 타입 키(`"int"`, `"bool"`)로 변환합니다.

**`ApplySavedGameData` — 레거시 세이브 복구**

`MemoryPuzzleStates`가 `string` 타입으로 잘못 저장된 구버전 세이브를 자동으로 복구합니다. `GameManager`의 현재 초기값(`Dictionary<int, bool>`)을 복사해 덮어씁니다.

```csharp
if (loadedVars.TryGetValue("MemoryPuzzleStates", out object memVal) && memVal is string)
{
    loadedVars["MemoryPuzzleStates"] = new Dictionary<int, bool>(defaultDict);
    Debug.Log("[SaveManager] Fixed legacy MemoryPuzzleStates.");
}
```

**Lock 동기화**

`SaveGameData`는 `lock(_saveLock)` 블록 안에서 실행됩니다. 로그 시스템 등 별도 스레드에서 저장이 호출될 때 데이터 경합을 방지합니다.

**초기화 흐름**

- `SaveInitGameData` — 게임 시작 직후 1회, 현재 `GameManager.Variables`를 `InitData`로 보관합니다.
- `LoadInitGameData` — 엔딩 이후 새 게임 시작 시, `InitData`로 변수를 초기화하고 저장합니다.
- `CreateNewGameData` — 기존 세이브 파일 삭제 후 `InitData` 기반으로 새 파일을 생성합니다.

---

## 구조 다이어그램

```
SaveManager
├─ SaveGameData()
│     ├─ lock(_saveLock)                      — 멀티스레드 경합 방지
│     ├─ new SaveData(GameManager.Variables)
│     │     ├─ dict:int-bool → JsonConvert.SerializeObject → JSON 문자열
│     │     ├─ int / bool    → ToString()
│     │     └─ string        → 그대로
│     │     variablesToJson + variablesTypeToJson 분리 저장
│     └─ AtomicWrite(path, json)
│           ├─ File.WriteAllText(tmp)          — 임시 파일 쓰기
│           ├─ File.Replace(tmp, path, bak)    — 원자적 교체
│           └─ 실패 시 File.Delete + File.Move — 폴백
│
├─ ApplySavedGameData()
│     ├─ File.ReadAllText → JsonConvert.DeserializeObject<SaveData>
│     ├─ NormalizeType()  — System.Int32 등 레거시 타입명 정규화
│     ├─ 타입별 역직렬화
│     │     ├─ dict:int-bool → JsonConvert.DeserializeObject<Dictionary<int,bool>>
│     │     │     실패 또는 빈 값 → new Dictionary<int, bool>() 폴백
│     │     ├─ int  → int.TryParse, 실패 시 0
│     │     └─ bool → bool.TryParse, 실패 시 false
│     ├─ MemoryPuzzleStates가 string → dict<int,bool> 자동 복구
│     └─ GameManager.Variables = loadedVars
│
├─ SaveInitGameData()   — 시작 시 초기값 스냅샷 (1회)
├─ LoadInitGameData()   — 엔딩 후 InitData로 초기화
└─ CreateNewGameData()  — 세이브 파일 삭제 후 InitData 기반 재생성
```

---

## 트러블슈팅

**강제 종료 시 세이브 파일 파손**

`File.WriteAllText`는 쓰기 도중 중단되면 파일이 빈 상태로 남습니다. 테스트 중 Task Manager로 프로세스를 강제 종료했을 때 세이브 파일이 0바이트가 되는 것을 확인했습니다. Atomic Write 도입 후 동일 조건에서 기존 `.bak` 파일이 보존되어 데이터 손실이 없었습니다.

**`MemoryPuzzleStates` 역직렬화 에러**

구버전에서 `Dictionary<int, bool>` 타입이 `"System.Collections.Generic.Dictionary\`2[...]"` 문자열로 잘못 저장되는 버그가 있었습니다. 신버전에서 이 값을 역직렬화하면 `ArgumentException`이 발생했습니다. `ApplySavedGameData`에서 값이 `string` 타입인 경우를 감지하고 자동으로 빈 `Dictionary<int, bool>`로 교체하는 복구 로직을 추가했습니다.

**세이브/로드 반복 시 점수 중복 합산**

`ResponsibilityScore`를 누적 방식으로 저장하다 보니, 동일 이벤트를 다시 플레이하면 점수가 이미 높은 값에서 시작했습니다. 점수를 저장된 상태값으로부터 매번 재계산하는 구조로 바꾸고, 새 게임 시작 시 `LoadInitGameData`로 초기화하도록 수정했습니다.

**`SaveVariable` 부분 저장의 위험성**

처음에는 변경된 변수 하나만 파일에서 찾아 교체하는 부분 저장을 시도했습니다. 파일 읽기 → 파싱 → 특정 값 교체 → 다시 쓰기 과정에서 타입 정보가 손실되거나 다른 변수가 오염되는 문제가 있었습니다. 안전을 위해 `SaveVariable`은 내부적으로 전체 저장(`SaveGameData`)을 호출하도록 통합했습니다.

---

## 사용 기술

- `Newtonsoft.Json` (`JsonConvert`) — `Dictionary<int, bool>` 복합 타입 직렬화/역직렬화
- Atomic Write (`File.Replace`) — 저장 중 강제 종료 시 데이터 파손 방지
- `lock` 키워드 — 멀티스레드 저장 경합 방지
- 타입 정보 분리 저장 (`variablesTypeToJson`) — 역직렬화 타입 손실 방지
- `Application.persistentDataPath` — 플랫폼별 저장 경로 자동 처리
