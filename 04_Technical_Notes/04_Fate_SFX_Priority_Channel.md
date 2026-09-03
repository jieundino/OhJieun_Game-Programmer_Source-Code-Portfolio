# 필연과 우연 — SFX Priority Channel System

> **Project:** &lt;필연과 우연&gt;  
> **Role:** Client / System Programmer  
> **Contribution:** 효과음 우선순위·채널 할당 구조 구현 및 채널 포화 상황 검증

> **Low / High Priority · Reserved Channels · Saturation Fallback**

## 1. 문서 목적

반복 클릭음이 한정된 `AudioSource` 채널을 점유해 중요한 연출음이 유실되는 문제를 해결하기 위해 구현한 SFX 채널 정책을 정리합니다.

---

## 2. 문제 정의

모든 효과음을 동일한 채널에서 선착순 처리하면 반복 클릭음이 전체 채널을 점유할 수 있었습니다. 이 경우 날짜 전환과 이벤트 피드백처럼 우선 전달되어야 하는 사운드가 재생되지 않을 수 있었습니다.

---

## 3. 실제 구현 정책

현재 코드의 우선순위는 `Low / High` 2단계입니다.

| Priority | 접근 범위 | 포화 시 처리 |
| --- | --- | --- |
| `Low` | 일반 채널 영역 | 빈 채널이 없으면 요청 종료 |
| `High` | 예약 채널을 포함한 전체 영역 | 빈 채널이 없으면 일반 영역에서 Voice Stealing 시도 |

- `[0 .. reservedHighPriorityChannels-1]`: High 전용 예약 영역
- `[reservedHighPriorityChannels .. end]`: Low가 사용하는 일반 영역
- High는 전체 영역을 탐색할 수 있음

---

## 4. 재생 흐름

```text
SFX 요청
→ 클릭음이면 Debounce 확인
→ Low / High 우선순위 확인
→ 접근 가능한 채널 범위 계산
→ Round-robin으로 빈 채널 탐색
→ 빈 채널에서 재생
→ High이며 빈 채널이 없으면 일반 영역 Voice Stealing
```

---

## 5. 핵심 구현

### 5.1 예약 채널

Low 요청은 High 전용 영역에 접근하지 못하도록 시작 인덱스를 제한했습니다.

### 5.2 Round-robin 탐색

마지막 탐색 위치 이후부터 순환해 특정 AudioSource에 요청이 집중되지 않도록 했습니다.

### 5.3 Voice Stealing

High 요청에 빈 채널이 없으면 일반 영역에서 재생 중인 채널 하나를 중단하고 High 요청을 재생합니다.

> 현재 구현은 일반 영역에 재생 중인 사운드의 실제 Priority를 별도 상태로 저장하지 않습니다. 따라서 문서에서는 “일반 영역 채널을 교체한다”고 표현하며, “항상 Low 사운드만 교체한다”고 과장하지 않습니다.

### 5.4 Click Debounce

동일 클릭음의 마지막 재생 시간을 기록하고 `Time.unscaledTime` 기준 최소 간격보다 빠른 요청을 차단했습니다.

### 5.5 BGM 정리

BGM 변경 시 기존 Fade Coroutine을 중단하고 Fade-out, Clip 교체, Fade-in 순서로 처리했습니다. `Time.unscaledDeltaTime`을 사용해 일시정지 중에도 볼륨 전환이 유지되도록 했습니다.

---

## 6. 검증 항목

| 항목 | 확인 내용 |
| --- | --- |
| Low 요청 | 일반 영역에서만 재생되는가 |
| High 요청 | 예약 영역을 포함한 채널을 사용할 수 있는가 |
| 반복 클릭 | 최소 간격 내 클릭음 요청이 차단되는가 |
| Round-robin | 채널 사용이 한 AudioSource에 편중되지 않는가 |
| 채널 포화 | High 요청이 일반 영역을 교체해 재생되는가 |
| TimeScale | 일시정지와 무관하게 Debounce와 Fade가 동작하는가 |

---

## 7. 구현 결과

- 반복 클릭음의 채널 과점유를 줄였습니다.
- High 요청을 위한 예약 경로를 확보했습니다.
- 빈 채널 탐색을 분산했습니다.
- 포화 상황에서 중요 요청을 재생할 fallback을 구성했습니다.

---

## 8. 한계 및 개선 방향

### 한계

- 일반 영역에서 재생 중인 실제 Priority를 추적하지 않음
- Voice Stealing 시 재생 중인 사운드가 즉시 중단됨
- 동일 Priority 내부의 중요도와 카테고리별 동시 재생 제한이 없음

### 개선 방향

```csharp
ChannelState
- AudioSource Source
- SfxPriority CurrentPriority
- int ClipId
- float StartedAt
```

- 채널별 현재 Priority 추적
- 요청보다 낮은 Priority만 교체
- 남은 재생 시간과 카테고리를 교체 기준에 포함
- 짧은 Fade-out 적용
- 채널 상태를 확인할 수 있는 런타임 디버그 뷰

---

## 9. 게임 프로그래밍 역량

한정된 런타임 자원에서 요청의 중요도, 접근 범위와 포화 시 fallback 정책을 정의했습니다. 실제 구현과 문서의 표현을 일치시키고, 현재 코드의 한계를 다음 상태 모델 개선안으로 구체화했습니다.
