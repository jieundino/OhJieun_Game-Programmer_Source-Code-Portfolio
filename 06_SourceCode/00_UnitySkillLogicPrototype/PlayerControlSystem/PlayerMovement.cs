using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float runMultiplier = 1.5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 10f;
    [SerializeField] private bool isJump;

    private float hAxis;
    private float vAxis;

    private bool runKey;
    private bool jumpRequested;

    private Vector3 moveVec;

    private Rigidbody rigid;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ReadInput();
        CalculateMoveDirection();

        Rotate();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        Move();
        Jump();
    }

    private void ReadInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");

        runKey = Input.GetButton("Run");

        if(Input.GetButtonDown("Jump"))
            jumpRequested = true;
    }

    private void CalculateMoveDirection()
    {
        if (cameraTransform ==null)
        {
            moveVec = new Vector3(hAxis, 0f, vAxis).normalized;
            return;
        }

        // 카메라 전방 방향
        Vector3 cameraForward = cameraTransform.forward;

        // 카메라 오른쪽 방향
        Vector3 cameraRight = cameraTransform.right;

        // 상하 카메라 각도는 이동에 반영X
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        moveVec = cameraForward * vAxis + cameraRight * hAxis;

        moveVec = Vector3.ClampMagnitude(moveVec, 1f);
    }


    private void Move()
    {
        float currentSpeed = runKey ? speed * runMultiplier : speed;

        Vector3 movement = moveVec * currentSpeed * Time.fixedDeltaTime;

        rigid.MovePosition(rigid.position+movement);
    }

    private void Rotate()
    {
        // 우클릭 중에는 카메라 방향 유지
        if (Input.GetMouseButton(1))
        {
            FaceCameraDirection();
            return;
        }

        // 입력 없으면 회전하지 않음
        if (moveVec.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(moveVec, Vector3.up);

        // 현재 회전값에서 목표 회전값까지 보간
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }

    private void Jump()
    {
        if (!jumpRequested) return;

        jumpRequested = false;

        // 2단 점프 차단
        if (isJump) return;

        rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);

        anim.SetTrigger("doJump");
        isJump = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Floor")
            isJump = false;
    }

    private void UpdateAnimation()
    {
        bool isMoving = moveVec.sqrMagnitude > 0.001f;

        anim.SetBool("isWalk", isMoving && !runKey);
        anim.SetBool("isRun", isMoving && runKey);
        anim.SetBool("isJump", isJump);

    }

    // 공격 시, Enemy가 있는 방향으로 즉시 회전
    public void FaceTargetInstant(Transform target)
    {
        if(target ==null)
        {
            FaceCameraDirection();
            return;
        }

        Vector3 directionToTarget = target.position - transform.position;

        directionToTarget.y = 0f; // 수직 방향 무시

        if (directionToTarget.sqrMagnitude <= 0.001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
    }

    private void FaceCameraDirection()
    {
        if (cameraTransform == null)
            return;

        Vector3 cameraForward = cameraTransform.forward;

        // 카메라 상하 각도는 반영X
        cameraForward.y = 0f;

        if (cameraForward.sqrMagnitude <= 0.001f) return;

        transform.rotation = Quaternion.LookRotation(cameraForward, Vector3.up);
    }
}
