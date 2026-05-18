using UnityEngine;

public class BunnyCollider : MonoBehaviour
{
    public BoxCollider2D[] idleColliders;
    public BoxCollider2D[] runColliders;

    void EnableColliders(BoxCollider2D[] colliders, bool active)
    {
        foreach (var col in colliders)
            col.enabled = active;
    }

    public void OnIdle()
    {
        EnableColliders(idleColliders, true);
        EnableColliders(runColliders, false);
    }

    public void OnRun()
    {
        EnableColliders(idleColliders, false);
        EnableColliders(runColliders, true);
    }

    public void OnDead()
    {
        EnableColliders(idleColliders, false);
        EnableColliders(runColliders, false);
    }
    public void OnHurt()
    {
        EnableColliders(idleColliders, true);
        EnableColliders(runColliders, false);
    }

}
