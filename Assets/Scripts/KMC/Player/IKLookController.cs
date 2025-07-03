using UnityEngine;

public class IKLookController : MonoBehaviour
{
    public Transform headTransform;         // 머리 본
    public Transform cameraTransform;       // 카메라
    public float lookSpeed = 5f;
    public float maxRotationAngle = 80f;    // 최대 회전 각도 제한

    void LateUpdate()
    {
        if (headTransform == null || cameraTransform == null) return;

        Vector3 lookTarget = cameraTransform.position + cameraTransform.forward * 10f;
        Vector3 direction = lookTarget - headTransform.position;

        // 방향 확인
        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // 회전 각도 제한 (optional)
        float angle = Quaternion.Angle(headTransform.rotation, targetRotation);
        if (angle < maxRotationAngle)
        {
            headTransform.rotation = Quaternion.Slerp(headTransform.rotation, targetRotation, Time.deltaTime * lookSpeed);
        }
    }
}