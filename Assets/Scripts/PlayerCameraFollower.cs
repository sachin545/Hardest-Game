using UnityEngine;

public class PlayerCameraFollower : MonoBehaviour
{
    [Header("Camera Settings")]
    public float smoothTime = 0.2f;
    public Vector2 offset;

    private Camera mainCamera;
    private Vector3 velocity = Vector3.zero;

    void Awake()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found. Make sure the camera is tagged as MainCamera.");
        }
    }

    void LateUpdate()
    {
        if (mainCamera == null)
            return;

        Vector3 targetPosition = new Vector3(
            transform.position.x + offset.x,
            transform.position.y + offset.y,
            mainCamera.transform.position.z
        );

        mainCamera.transform.position = Vector3.SmoothDamp(
            mainCamera.transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}
