using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Target settings")]
    public Transform target;

    [Header("Camera Offset")]
    [SerializeField] private float distance = 10f;
    [SerializeField] private float heightOffset = 1.5f;

    [Header("Camera Rotation")]
    public float sensitivity = 100f; // 마우스 감도 조절 변수

    // 좌우 회전 = Y축 회전
    private float yaw = 0f;
    // 상하 회전 = X축 회전
    private float pitch = 20f;

    [SerializeField] private float minPitch = -10f; // 최소 상하 회전 각도
    [SerializeField] private float maxPitch = 60f;  // 최대 상하 회전 각도

    private void Awake()
    {
        if(target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
                target = player.transform;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(target == null)
        {
            return;
        }

        RotateCamera();
        UpdateCameraPosition();
    }

    private void RotateCamera()
    {
        // 우클릭 중에만 카메라 회전
        if(!Input.GetMouseButton(1))
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void UpdateCameraPosition()
    {
        // 플레이어보다 heightOffset 만큼 위에 있는 지점을
        // 카메라 회전 중심으로 사용
        Vector3 pivotPosition = target.position + Vector3.up * heightOffset;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 cameraOffset = rotation * new Vector3(0f, 0f, -distance);

        transform.position = pivotPosition + cameraOffset;

        transform.rotation = rotation;
    }

}
