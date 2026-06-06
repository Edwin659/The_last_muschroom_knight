using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinUIManager : MonoBehaviour
{
    public static CoinUIManager instance;

    public Image coinIcon;
    public TMP_Text coinText;
    private int coinCount = 0;

    void Awake()
    {
        instance = this;
        UpdateUI();
    }

    public void AddCoin()
    {
        coinCount++;
        UpdateUI();
    }

    public int GetCoinCount()
    {
        return coinCount;
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = coinCount.ToString();

        if (coinIcon != null)
            coinIcon.enabled = true;
    }
}
