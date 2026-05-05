using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform player;      // Player
    public Vector3 offset;        // Camera
    public float upwardLook = 0.5f; // Extra height above player
    public float smoothSpeed = 0.8f; // Smoothing speed

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
        if (player == null)
        {
            Debug.LogWarning("Camera: Player reference is not set.");
            return;
        }
        transform.position = player.position + offset + Vector3.up * upwardLook;
    }
    // Verticals Limits
    public float verticalThreshold = 3f;
       void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        Vector3 desiredPosition = new Vector3(
        player.position.x + offset.x,
        player.position.y + offset.y + upwardLook,
        transform.position.z 
    );

        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime * 60f
        );
        transform.position = smoothedPosition;
    }
}

