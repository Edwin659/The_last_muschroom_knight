using UnityEngine;

public class AnimationEventProxy : MonoBehaviour
{
    public void DealDamage()
    {
        PlayerDamage playerDamage = GetComponentInParent<PlayerDamage>();
        if (playerDamage != null)
        {
            playerDamage.DealDamage();
        }
    }
}
