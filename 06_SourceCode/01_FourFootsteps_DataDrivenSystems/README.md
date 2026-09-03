# 네 발자국 — Selected Source Code

본 폴더는 지원자가 직접 설계·구현·수정한 **데이터 기반 클라이언트 시스템 코드**를 선별한 자료입니다.

## 코드 소유 범위

- 제출 파일 작성·수정 비중: 100%
- 공동개발 프로젝트 전체 코드가 아니라 지원자의 담당 범위만 포함
- Scene, Prefab, CSV 원본, 일부 Manager와 외부 패키지에 의존하는 코드 검토용 자료
- 프로젝트 전체 기여 비중은 상위 [`../../01_Project_Contribution.md`](../../01_Project_Contribution.md) 참고

## 핵심 확인 지점

| 폴더 | 핵심 확인 지점 |
| --- | --- |
| `EventResultSystem` | 요구사항을 CSV·ID와 조건/결과 실행기로 구조화 |
| `DialogueSystem` | 동시 대사 요청을 Queue로 순서 제어 |
| `SaveSystem` | Atomic Write, 타입 보존, 레거시·손상 데이터 복구 |
| `LogSystem` | 실행 결과 식별, 중복 방지, 실패 보존과 재시도 |

## 권장 열람 순서

1. `EventResultSystem/EventManager.cs`
2. `EventResultSystem/ResultManager.cs`
3. `SaveSystem/SaveManager.cs`
4. `LogSystem/EndingLogQueueManager.cs`
5. `DialogueSystem/DialogueManager.cs`

## 주요 기술적 의사결정

- 콘텐츠 데이터와 실행 코드 분리
- 조건 판정과 실제 결과 적용 책임 분리
- Instant / Sequential 실행 정책
- 저장 실패 경로와 fallback 구성
- 네트워크 실패 시 로그 삭제가 아닌 재처리

## 현재 구조의 한계

- CSV 참조 무결성 검증이 런타임 중심
- Result 유형 증가 시 분기 확대 가능
- Save migration 책임이 Manager에 집중될 수 있음

후속 개선 방향은 `04_Technical_Notes`에 정리했습니다.
