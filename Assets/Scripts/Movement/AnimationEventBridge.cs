using UnityEngine;

public class AnimationEventBridge : MonoBehaviour
{
    public PlayerMovement playerMovement; // drag the Player here in Inspector

    public void EndAttack()
    {
        playerMovement.EndAttack();
    }
}
