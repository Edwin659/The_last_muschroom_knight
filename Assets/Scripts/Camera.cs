using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform player;      // Player
    public Vector3 offset;        // Camera
    public float smoothSpeed = 0.125f; // Smoothing speed

    // Verticals Limits
    public float verticalThreshold = 3f;

    void LateUpdate()
    {
        //Desired pos
        float targetX = player.position.x + offset.x;
        float targetY = transform.position.y;

        // Limits
        if (player.position.y > transform.position.y + verticalThreshold)
        {
            targetY = player.position.y - verticalThreshold;
        }
        else if (player.position.y < transform.position.y - verticalThreshold)
        {
            targetY = player.position.y + verticalThreshold;
        }

        Vector3 desiredPosition = new Vector3(targetX, targetY, transform.position.z);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}

