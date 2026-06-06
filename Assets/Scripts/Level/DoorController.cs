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
    //public string winMenu;

    public GameObject overlayPanel;
    public TMP_Text messageText;

    // Variables for Level 1
    public int coinsRequired = 0;
    private int coinsCollected = 0;
    private string sceneToLoad;
    // Variables for Level 2
    private bool bossDead = false;

    public void AddCoin()
    {
        coinsCollected++;
    }

    public void BossDefeated()
    {
        bossDead = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        switch (currentLevel)
        {
            case LevelType.Tutorial:
                ShowOverlay("Tutoriel Fini, passage au level 1", nextLevelName1);
                break;

            case LevelType.Level1:
                if (CoinUIManager.instance.GetCoinCount() >= coinsRequired)
                    ShowOverlay("Level 1 Fini, passage au level 2", nextLevelName2);
                break;
        }

        //case LevelType.Level2:
        //  if (bossDead)
        //{
        //ShowOverlay("Boss vaincu, passage au prochain niveau",winMenu);
        //}
        //break;
        //}
    }
    private void ShowOverlay(string msg, string sceneName)
    {
        sceneToLoad = sceneName;
        overlayPanel.SetActive(true);
        messageText.text = msg;
    }
    public void LoadNextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
    
}
