using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;      // Player
    public Vector3 offset;        // Camera
    public float upwardLook = 0.5f; // Extra height above player
    public float smoothSpeed = 0.8f; // Smoothing speed

    public Transform leftBoundary;
    public Transform rightBoundary;

    void Awake()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    void Start()
    {
        if (player != null)
        {
            transform.position = player.position + offset + Vector3.up * upwardLook;
        }
    }
   
    // Verticals Limits
    void LateUpdate()
    {

        if (player == null) return;

        // Position cible centrée sur le joueur
        float targetX = player.position.x + offset.x;
        float targetY = player.position.y + offset.y + upwardLook;

        float camHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;

        // Clamp en tenant compte de la largeur de la caméra
        targetX = Mathf.Clamp(
            targetX,
            leftBoundary.position.x + camHalfWidth,
            rightBoundary.position.x - camHalfWidth
        );

        Vector3 desiredPosition = new Vector3(targetX, targetY, offset.z);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime * 60f);
    }
}

