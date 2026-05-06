using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    public PlayerHealth playerHealth;

    public void DieEnd()
    {
        if (playerHealth != null)
        {
            playerHealth.DieEnd();
        }
    }
}
