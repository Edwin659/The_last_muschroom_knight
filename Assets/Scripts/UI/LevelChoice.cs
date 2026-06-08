using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChoiceMenu : MonoBehaviour
{
    public void LoadLevel1()
    {
        SceneManager.LoadScene("Level1");
    }

    public void LoadLevel2()
    {
        SceneManager.LoadScene("Level2");
    }

    public void LoadTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
