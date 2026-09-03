> **코드 소유 범위:** 이 폴더의 제출 코드는 지원자가 직접 작성하거나 직접 수정한 파일입니다. 원본 프로젝트의 Scene, Prefab, Manager 및 데이터 의존성은 생략되어 있습니다.

# Sound System

BGM과 효과음(SFX) 재생을 통합 관리하는 사운드 플레이어입니다.  
제한된 오디오 채널 안에서 중요한 효과음이 유실되지 않도록 우선순위 기반 채널 관리 시스템을 설계했습니다.

---

## 설계 의도

Unity의 `AudioSource` 컴포넌트는 개수가 제한됩니다. 클릭음처럼 자주 발생하는 저우선순위 효과음이 채널을 모두 점유하면, 날짜 전환 연출음처럼 중요한 고우선순위 효과음이 재생되지 않습니다.

이를 해결하기 위해 `UISoundPlayer` 배열을 두 구간으로 나눴습니다. `[0 ~ reservedHighPriorityChannels)` 구간은 High 우선순위 전용으로 예약하고, `[reserved ~ end)` 구간은 Low 우선순위가 사용합니다. High 효과음은 일반 영역 채널을 강제로 선점(Voice Stealing)할 수 있지만, Low 효과음은 High 채널에 접근하지 못합니다.

---
## 담당 범위
> **Contribution:** 효과음 우선순위·채널 할당 구조 구현 및
> 반복 입력·채널 포화 상황 검증

---

## 주요 구현

**BGM 관리**

- `ChangeBGM` — 진행 중인 BGM을 먼저 페이드 아웃(`FadeTo(0, duration)`)한 뒤 새 클립으로 교체하고 페이드 인합니다.
- `FadeTo` — `Time.unscaledDeltaTime`을 사용해 게임이 일시정지 중에도 볼륨 전환이 진행됩니다.
- `SetMuteBGM` — 랩탑 화면 전환 등 특정 상황에서 BGM을 빠르게 음소거/복구합니다.

**우선순위 기반 효과음 재생 (`UISoundPlay`)**

1. **클릭 디바운스** — `Time.unscaledTime - lastClickTime < clickMinInterval`(50ms) 조건으로 너무 빠른 연속 클릭 효과음을 억제합니다. `unscaledTime`을 사용하는 것은 일시정지 중에도 시간 기준이 유지되어야 하기 때문입니다.
2. **우선순위 구간 결정** — `SfxPriority`에 따라 탐색 구간(`start`, `end`)을 결정합니다.
3. **라운드 로빈 빈 채널 탐색** — `uiSoundPlayerCursor`를 사용해 매번 다른 채널에서 시작해 공평하게 분산합니다. 특정 채널에만 재생이 집중되어 기존 효과음이 강제로 중단되는 문제를 방지합니다.
4. **보이스 스틸링 (High only)** — 빈 채널이 없는 경우, 일반 영역에서 재생 중인 채널 하나를 중단하고 High 요청을 재생합니다.

> 현재 구현은 일반 영역에서 재생 중인 사운드의 실제 Priority를
> 별도 상태로 저장하지 않습니다.
> 따라서 Voice Stealing은 <u>Low Priority를 정확히 찾아 교체</u>하는 구조가 아니라
> <u>High 요청이 일반 영역 채널을 회수할 수 있는 fallback</u>으로 설명합니다.

```
UISoundPlay(num, priority)
│
├─ [클릭 디바운스]   unscaledTime 간격 50ms 이내면 스킵
├─ [우선순위 구간]   High → [0, reserved) / Low → [reserved, end)
├─ [라운드 로빈]     uiSoundPlayerCursor 기반 빈 채널 탐색
│     빈 채널 있음 → PlayOnUISound() → return
└─ [보이스 스틸링]   High만 해당 — 일반 영역 채널 Stop() 후 High 재생
```

**`PlayOnUISound`**

클립 교체, `panStereo` 설정(마우스 클릭 위치 기반 좌우 패닝), 재생을 묶어서 처리합니다.

---

## 트러블슈팅

**클릭음 연속 발생 시 연출음 재생 불가**

오브젝트를 빠르게 연속 클릭하면 `UISoundPlayer` 배열의 모든 채널이 클릭음으로 점유됩니다. 날짜 전환 연출음 같은 중요 효과음이 빈 채널을 찾지 못해 재생되지 않았습니다.

세 가지를 순서대로 적용해 해결했습니다.

- 클릭 디바운스로 빠른 연속 클릭 효과음 자체를 억제
- 우선순위 채널 분리로 High 효과음 전용 구간 확보
- 보이스 스틸링으로 High 효과음이 일반 영역 채널을 강제 선점 가능

```csharp
// 클릭 디바운스
if (num == Sound_Click && Time.unscaledTime - lastClickTime < clickMinInterval) return;

// 보이스 스틸링 (High 전용)
if (prio == SfxPriority.High)
{
    // 일반 영역 채널 중 재생 중인 채널 탐색
    var src = UISoundPlayer[i];
    if (src.isPlaying)
    {
        src.Stop();              // Low 효과음 중단
        PlayOnUISound(src, num); // High 효과음 재생
        return;
    }
}
```

---

## 사용 기술

- `SfxPriority` Enum — 우선순위 채널 구간 설계
- `Time.unscaledTime` — 일시정지 독립적 디바운스/페이드 타이밍
- 라운드 로빈 인덱싱 (`uiSoundPlayerCursor`) — 채널 공평 분산
- `AudioSource.panStereo` — 클릭 위치 기반 좌우 패닝
- `AudioSource.volume` 코루틴 제어 — BGM 페이드 인/아웃
