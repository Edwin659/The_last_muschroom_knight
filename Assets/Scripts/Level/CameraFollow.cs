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

        // Taille fixe de la caméra
        camHalfHeight = Camera.main.orthographicSize;
        camHalfWidth = camHalfHeight * (16f / 9f); // ratio forcé 16:9

        // Position cible centrée sur le joueur
        float targetX = player.position.x + offset.x;
        float targetY = player.position.y + offset.y + upwardLook;

        Vector3 desiredPosition = new Vector3(targetX, targetY, offset.z);

        // Limites calculées à partir des colliders
        float minX = leftBoundary.bounds.max.x + camHalfWidth;
        float maxX = rightBoundary.bounds.min.x - camHalfWidth;

        float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);

        Vector3 finalPosition = new Vector3(clampedX, targetY, desiredPosition.z);

        transform.position = Vector3.Lerp(transform.position, finalPosition, smoothSpeed * Time.deltaTime * 60f);
    }
}
