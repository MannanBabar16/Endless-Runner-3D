using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    [System.Serializable]
    public class Character
    {
        public int index;
        public int cost;
        public GameObject root;
        public GameObject modelPreview;
        public Button unlockButton;
        public Button selectButton;
        public TextMeshProUGUI statusText;
    }

    [Header("UI")]
    public RectTransform carouselRoot;
    public Character[] characters;

    public Button leftArrow;
    public Button rightArrow;
    public TextMeshProUGUI totalCoinsText;
    public TextMeshProUGUI warningText; // 🆕 Add this in Inspector
    public GameObject warningPanel; // 🆕 Add this in Inspector

    public float panelSpacing = 900f;
    private int currentIndex = 0;
    private float warningDuration = 2f;
    private Coroutine warningRoutine;

    void Start()
    {
        if (carouselRoot == null)
        {
            Debug.LogError("CarouselRoot is not assigned.");
            return;
        }

        leftArrow.onClick.AddListener(() => Scroll(-1));
        rightArrow.onClick.AddListener(() => Scroll(1));

        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].index = i;
        }

        if (warningPanel != null)
            warningPanel.SetActive(false);  // ✅ Disable the whole panel at start

        UpdateCarousel();
        CenterOnCurrent();
    }


    void Scroll(int direction)
    {
        currentIndex += direction;

        if (currentIndex < 0)
            currentIndex = characters.Length - 1;
        else if (currentIndex >= characters.Length)
            currentIndex = 0;

        CenterOnCurrent();
        UpdateCarousel();
    }

    void CenterOnCurrent()
    {
        Vector2 targetPos = new Vector2(-currentIndex * panelSpacing, carouselRoot.anchoredPosition.y);
        carouselRoot.anchoredPosition = targetPos;
    }

    void UpdateCarousel()
    {
        totalCoinsText.text = "Coins: " + SaveData.GetTotalCoins();

        for (int i = 0; i < characters.Length; i++)
        {
            var character = characters[i];
            bool isCurrent = (i == currentIndex);
            bool unlocked = SaveData.IsPlayerUnlocked(character.index);
            bool selected = SaveData.GetSelectedPlayer() == character.index;

            character.root.SetActive(true);
            character.modelPreview.SetActive(isCurrent);

            character.unlockButton.gameObject.SetActive(isCurrent && !unlocked);
            character.selectButton.gameObject.SetActive(isCurrent && unlocked && !selected);

            if (unlocked)
                character.statusText.text = selected ? "Selected" : "Unlocked";
            else
                character.statusText.text = $" {character.cost} Coins to Unlock";
        }
    }

    public void UnlockCurrent()
    {
        var character = characters[currentIndex];

        // 🛡️ Safety Check: Prevent spending on already unlocked
        if (SaveData.IsPlayerUnlocked(character.index))
        {
            UpdateCarousel();
            return;
        }

        if (!SaveData.HasEnoughCoins(character.cost))
        {
            ShowWarning("Not enough coins!");
            return;
        }

        SaveData.SpendCoins(character.cost);
        SaveData.UnlockPlayer(character.index);
        SaveData.SetSelectedPlayer(character.index);

        UpdateCarousel();
    }


    public void SelectCurrent()
    {
        var character = characters[currentIndex];
        SaveData.SetSelectedPlayer(character.index);
        UpdateCarousel();
    }

    void ShowWarning(string message)
    {
        if (warningText == null || warningPanel == null)
        {
            Debug.LogWarning("WarningText or WarningPanel not assigned.");
            return;
        }

        if (warningRoutine != null)
            StopCoroutine(warningRoutine);

        warningRoutine = StartCoroutine(ShowWarningRoutine(message));
    }


    IEnumerator ShowWarningRoutine(string message)
    {
        warningText.text = message;
        warningPanel.gameObject.SetActive(true);

        yield return new WaitForSeconds(warningDuration);

        warningPanel.gameObject.SetActive(false);
    }
}
