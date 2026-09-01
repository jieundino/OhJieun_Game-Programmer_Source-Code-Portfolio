# Player Control System

카메라 방향을 기준으로 플레이어를 이동시키고, 마우스 입력으로 카메라를 회전하는 기본 3D 조작 시스템입니다.  
스킬 실행에 성공한 경우 플레이어가 자동으로 공격 대상 방향을 바라보도록 Skill System과 연결했습니다.

---

## 설계 의도

3D 게임에서 월드 축을 기준으로 이동하면 카메라가 회전했을 때 입력 방향과 화면에서 보이는 방향이 어긋날 수 있습니다.

플레이어 입력을 카메라의 Forward / Right 방향으로 변환하여, 카메라가 어느 방향을 보고 있어도 화면 기준으로 일관된 이동이 가능하도록 구성했습니다.

또한 스킬 사용 시에는 일반 이동 회전과 별도로 대상 방향을 즉시 바라보게 하여 공격 애니메이션 방향과 Target 방향을 맞췄습니다.

---

## 주요 구현

### Camera-relative Movement

카메라의 Forward와 Right Vector에서 y축 성분을 제거한 뒤 이동 입력과 조합합니다.

```csharp
Vector3 cameraForward = cameraTransform.forward;
Vector3 cameraRight = cameraTransform.right;

cameraForward.y = 0f;
cameraRight.y = 0f;

cameraForward.Normalize();
cameraRight.Normalize();

moveVec = cameraForward * vAxis + cameraRight * hAxis;
moveVec = Vector3.ClampMagnitude(moveVec, 1f);
```

이를 통해 카메라의 상하 각도는 이동 방향에 영향을 주지 않고, 수평 방향만 반영됩니다.

### Rigidbody 기반 이동과 점프

- 입력 및 이동 방향 계산 — `Update`
- Rigidbody 위치 이동 및 점프 — `FixedUpdate`
- `Rigidbody.MovePosition` 기반 이동
- `ForceMode.Impulse` 기반 점프
- `isJump` 상태로 2단 점프 차단

### 이동 방향 회전

이동 입력이 있는 경우 `Quaternion.LookRotation`으로 목표 회전을 생성하고 `Quaternion.Slerp`로 부드럽게 회전합니다.

우클릭으로 카메라를 조작하는 동안에는 카메라 전방을 바라보도록 구성했습니다.

### Skill Target 방향 전환

스킬 실행에 성공하면 `PlayerSkillController`가 `FaceTargetInstant`를 호출합니다.

```csharp
public void FaceTargetInstant(Transform target)
{
    if (target == null)
    {
        FaceCameraDirection();
        return;
    }

    Vector3 directionToTarget =
        target.position - transform.position;

    directionToTarget.y = 0f;

    if (directionToTarget.sqrMagnitude <= 0.001f)
        return;

    transform.rotation =
        Quaternion.LookRotation(directionToTarget, Vector3.up);
}
```

Target이 없는 경우에는 카메라 전방을 기준으로 회전합니다.

### Follow Camera

우클릭 중 마우스 입력으로 yaw / pitch를 조정하고, pitch는 최소·최대 각도 안에서 Clamp합니다.

카메라는 플레이어보다 `heightOffset`만큼 위에 있는 지점을 Pivot으로 사용하며, 설정된 `distance`만큼 뒤에 위치합니다.

---

## 구조 다이어그램

```text
Input
├─ Horizontal / Vertical
├─ Run
├─ Jump
└─ Right Mouse Camera Rotation
        │
        ▼
PlayerMovement
├─ ReadInput()
├─ CalculateMoveDirection()
│    └─ Camera Forward / Right
├─ Move()
├─ Rotate()
├─ Jump()
└─ FaceTargetInstant()
        ▲
        │ Skill Execution Success
PlayerSkillController

FollowCamera
├─ RotateCamera()
│    ├─ yaw
│    ├─ pitch
│    └─ Clamp(minPitch, maxPitch)
└─ UpdateCameraPosition()
     └─ Pivot + Rotated Offset
```

---

## 검증 및 고려 사항

### Update와 FixedUpdate 역할 분리

입력은 `Update`에서 읽고, Rigidbody 이동과 Force 적용은 `FixedUpdate`에서 처리했습니다.

점프 입력이 한 Frame만 유지되는 문제를 방지하기 위해 `jumpRequested` 상태를 저장한 뒤 FixedUpdate에서 소비합니다.

### 카메라 상하 각도 제거

카메라의 Forward Vector를 그대로 사용하면 카메라가 아래를 볼 때 플레이어 이동 방향에도 y축 값이 포함될 수 있습니다.

Forward / Right의 y값을 0으로 만들고 정규화해 수평 이동만 계산했습니다.

### 공격 방향과 이동 회전 분리

일반 이동에서는 부드러운 보간 회전을 사용하지만, 스킬 사용 시에는 Target 방향을 즉시 바라보도록 분리했습니다.

이를 통해 공격 입력 직후 애니메이션이 다른 방향으로 재생되는 문제를 줄였습니다.

---

## 사용 기술

- `Rigidbody.MovePosition` — 물리 Update 주기에 맞춘 플레이어 이동
- `ForceMode.Impulse` — 점프 Force 적용
- `Quaternion.LookRotation` — 이동 및 Target 방향 회전
- `Quaternion.Slerp` — 이동 방향의 부드러운 회전
- Camera-relative Vector — 화면 기준 이동 방향 계산
- `Mathf.Clamp` — 카메라 Pitch 범위 제한
- Animator Parameter / Trigger — Walk, Run, Jump, Skill Animation 연동
