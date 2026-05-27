using UnityEngine;
using UnityEngine.UI;

public class LifeBarController : MonoBehaviour
{
    public static LifeBarController instance;
    public int currentLives = 3;
    public Image[] hearts;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = (i < currentLives);
        }
    }

}
