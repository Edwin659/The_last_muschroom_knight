using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform player;      // Player
    public Vector3 offset;        // Camera
    public float smoothSpeed = 0.8f; // Smoothing speed

    void Start()
    {
        transform.position = player.position + offset;
    }
    // Verticals Limits
    public float verticalThreshold = 3f;
       void LateUpdate()
    {
        Vector3 desiredPosition = new Vector3(
        player.position.x + offset.x,
        player.position.y + offset.y,
        offset.z 
    );

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}

