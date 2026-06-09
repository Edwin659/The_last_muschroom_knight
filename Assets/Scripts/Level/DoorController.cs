using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DoorController : MonoBehaviour
{
    public enum LevelType { Tutorial, Level1, Level2 }
    public LevelType currentLevel;

    public string nextLevelName1;
    public string nextLevelName2;
    public string winMenu;

    public GameObject overlayPanel;
    public TMP_Text messageText;

    // Variables for Level 1
    public int coinsRequired = 0;
    public string sceneToLoad;

    // Variables for Level 2
    public bool bossDead = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        switch (currentLevel)
        {
            case LevelType.Tutorial:
                ShowOverlay("Tutorial Done, move to level 1", nextLevelName1);
                break;

            case LevelType.Level1:
                if (CoinUIManager.instance.GetCoinCount() >= coinsRequired)
                    ShowOverlay("Level 1 Finished, move to \nlevel 2", nextLevelName2);
                break;

        case LevelType.Level2:
            if (bossDead)
            {
                ShowOverlay("Boss defeated",winMenu);
            }
        break;
        }
    }
    private void ShowOverlay(string msg, string sceneName)
    {
        sceneToLoad = sceneName;
        overlayPanel.SetActive(true);
        messageText.text = msg;
    }
    public void SetBossDead()
    {
        bossDead = true;
    }

    public void LoadNextScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("SceneToLoad is empty !");
            return;
        }

        Debug.Log("scene to load: " + sceneToLoad);

        if (Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("the scene " + sceneToLoad + " is not in built !");
        }
    }


}
