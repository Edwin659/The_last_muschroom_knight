using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    public float upwardLook = 0.5f;
    public float smoothSpeed = 0.8f;

    public BoxCollider2D leftBoundary;
    public BoxCollider2D rightBoundary;

    private float camHalfHeight;
    private float camHalfWidth;

    void LateUpdate()
    {
        if (player == null) return;

        // Camera
        camHalfHeight = UnityEngine.Camera.main.orthographicSize;
        camHalfWidth = camHalfHeight * (16f / 9f);

        // player in center
        float targetX = player.position.x + offset.x;
        float targetY = player.position.y + offset.y + upwardLook;

        Vector3 desiredPosition = new Vector3(targetX, targetY, offset.z);

        // limite
        float minX = leftBoundary.bounds.max.x + camHalfWidth;
        float maxX = rightBoundary.bounds.min.x - camHalfWidth;

        float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);

        Vector3 finalPosition = new Vector3(clampedX, targetY, desiredPosition.z);

        transform.position = Vector3.Lerp(transform.position, finalPosition, smoothSpeed * Time.deltaTime * 60f);
    }
}
