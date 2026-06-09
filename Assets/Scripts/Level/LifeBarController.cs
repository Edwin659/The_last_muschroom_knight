using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
            SceneManager.sceneLoaded += OnSceneLoaded;
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
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene chargée: " + scene.name);

        if (scene.name == "Level2objective"
            || scene.name == "Level1objective"
            || scene.name == "LevelChoice"
            || scene.name == "MainMenu"
            || scene.name == "GameComplete"
            || scene.name == "GameOverMenu")
        {
            Debug.Log("boom");
            Destroy(gameObject);
            instance = null;
        }
    }


    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
