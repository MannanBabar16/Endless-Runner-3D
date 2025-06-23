using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI coinText;

    [Header("Power-Up UI")]
    public Slider magnetSlider;
    public Slider invisibilitySlider;

    public GameObject magnetSliderPanel;
    public GameObject invisibilitySliderPanel;

    private float magnetTimer;
    private float invisibilityTimer;

    private bool isMagnetActive = false;
    private bool isInvisibilityActive = false;

    [Header("Dependencies")]
    public GameManager gameManager;
    
    public TextMeshProUGUI scoreText;


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

        gameManager.onCoinCollected.AddListener(UpdateUI);
        UpdateUI(gameManager.coinCount);

        // Ensure power-up panels are hidden initially
        magnetSliderPanel.SetActive(false);
        invisibilitySliderPanel.SetActive(false);
        
        gameManager.onScoreUpdated.AddListener(UpdateScoreUI);
        UpdateScoreUI(gameManager.gameScore);

    }

    void Update()
    {
        if (isMagnetActive)
        {
            magnetTimer -= Time.deltaTime;
            magnetSlider.value = magnetTimer;

            if (magnetTimer <= 0f)
            {
                isMagnetActive = false;
                magnetSliderPanel.SetActive(false);
            }
        }

        if (isInvisibilityActive)
        {
            invisibilityTimer -= Time.deltaTime;
            invisibilitySlider.value = invisibilityTimer;

            if (invisibilityTimer <= 0f)
            {
                isInvisibilityActive = false;
                invisibilitySliderPanel.SetActive(false);
            }
        }
    }

    void UpdateUI(int count)
    {
        coinText.text = $"Coins: {count}";
    }

    public void ActivateMagnet(float duration)
    {
        magnetTimer = duration;
        isMagnetActive = true;

        magnetSliderPanel.SetActive(true);
        magnetSlider.maxValue = duration;
        magnetSlider.value = duration;
    }

    public void ActivateInvisibility(float duration)
    {
        invisibilityTimer = duration;
        isInvisibilityActive = true;

        invisibilitySliderPanel.SetActive(true);
        invisibilitySlider.maxValue = duration;
        invisibilitySlider.value = duration;
    }
    
    void UpdateScoreUI(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

}
