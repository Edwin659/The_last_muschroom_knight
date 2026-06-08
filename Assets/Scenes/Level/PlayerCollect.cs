using UnityEngine;

public class PlayerCollect : MonoBehaviour
{
    public int acornCount = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Acorn"))
        {
            acornCount++;
            Destroy(other.gameObject);
            Debug.Log("Acorn: " + acornCount);
        }
    }
}