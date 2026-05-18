using UnityEngine;

public class AnimationEventProxy : MonoBehaviour
{
    public void DealDamage()
    {
        // Cherche le script PlayerDamage sur le parent
        PlayerDamage playerDamage = GetComponentInParent<PlayerDamage>();
        if (playerDamage != null)
        {
            playerDamage.DealDamage();
        }
    }
}
