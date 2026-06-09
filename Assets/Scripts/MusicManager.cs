using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    public AudioSource audioSource;

    public AudioClip menuMusic;
    public AudioClip tutoMusic;
    public AudioClip level1Music;
    public AudioClip level2Music;
    public AudioClip victoryMusic;
    public AudioClip defeatMusic;

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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene chargée: " + scene.name);

        if (scene.name == "MainMenu" || scene.name == "LevelChoice")
        {
            SwitchMusic(menuMusic);
        }
        else if (scene.name == "Tutorial")
        {
            SwitchMusic(tutoMusic);
        }
        else if (scene.name == "level1")
        {
            SwitchMusic(level1Music);
        }
        else if (scene.name == "level2")
        {
            SwitchMusic(level2Music);
        }
        else if (scene.name == "GameComplete")
        {
            SwitchMusic(victoryMusic);
        }
        else if (scene.name == "GameOverMenu")
        {
            SwitchMusic(defeatMusic);
        }
    }

    void SwitchMusic(AudioClip newClip)
    {
        if (newClip == null) return;
        if (audioSource.clip == newClip) return;
        audioSource.clip = newClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
