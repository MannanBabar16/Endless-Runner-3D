using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI coinText;

    [Header("Dependencies")]
    public GameManager gameManager;

    void Start()
    {
        if (gameManager == null)
        {
            Debug.LogError("UiManager: GameManager reference is missing!");
            return;
        }

        if (coinText == null)
        {
            Debug.LogError("UiManager: Coin Text reference is missing!");
            return;
        }

        // Subscribe to the coin collected event
        gameManager.onCoinCollected.AddListener(UpdateUI);

        // Initialize the UI with the starting coin count
        UpdateUI(gameManager.coinCount);
    }

    void UpdateUI(int count)
    {
        coinText.text = $"Coins: {count}";
    }
}
