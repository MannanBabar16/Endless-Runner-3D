using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class UiManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI scoreText;

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

    // --- PAUSE MENU ADDITIONS ---
    [Header("Pause Menu UI")]
    public GameObject pauseButton;
    public GameObject pausePanel;
    public GameObject resumeCountdownPanel;
    public TextMeshProUGUI countdownText;

    private bool isPaused = false;

    // ----------------------------

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

        magnetSliderPanel.SetActive(false);
        invisibilitySliderPanel.SetActive(false);

        gameManager.onScoreUpdated.AddListener(UpdateScoreUI);
        UpdateScoreUI(gameManager.gameScore);

        // Initialize pause menu
        if (pausePanel != null) pausePanel.SetActive(false);
        if (resumeCountdownPanel != null) resumeCountdownPanel.SetActive(false);
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

    void UpdateScoreUI(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
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

    // --------- PAUSE MENU METHODS ---------

    public void OnPausePressed()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(false);
    }

    public void OnResumePressed()
    {
        pausePanel.SetActive(false);
        StartCoroutine(ResumeCountdown());
    }

    IEnumerator ResumeCountdown()
    {
        resumeCountdownPanel.SetActive(true);
        if (pauseButton != null) pauseButton.SetActive(false);

        int countdown = 3;
        while (countdown > 0)
        {
            if (countdownText != null)
                countdownText.text = $"Resuming in {countdown}...";
            yield return new WaitForSecondsRealtime(1f);
            countdown--;
        }

        resumeCountdownPanel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OnQuitPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    // -------------------------------------
}
