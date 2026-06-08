using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    public LifeBarController lifeBarController;

    public void RestartGame()
    {
        // Reload Last Scene
        string lastScene = PlayerPrefs.GetString("LastScene", "MenuScene");
        SceneManager.LoadScene(lastScene);
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
