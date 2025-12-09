using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraFollow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Camera Target")]
    [SerializeField] private Transform cameraTarget;
    [Header("Camera Distance")]
    [SerializeField] private Vector3 cameraDistance = new Vector3(0, 0, -20f);
    [Header("Camera Speed")]
    [SerializeField] private float cameraSpeed = 0.125f;

    private void LateUpdate()
    {
        if (cameraTarget == null)
        {
            Debug.LogError("Camera Target doesn't exist");
            return;
        }
        Vector3 targetPosition = cameraTarget.position + cameraDistance;
        Vector3 smoothedPosition=Vector3.Lerp(transform.position, targetPosition, cameraSpeed);
        transform.position = smoothedPosition;
    }
}
