using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveData
{
    private const string CoinsKey = "TotalCoins";
    private const string HighScoreKey = "HighScore";
    private const string PlayerKeyPrefix = "PlayerUnlocked_";
    private const string SelectedPlayerKey = "SelectedPlayer";

    // --- Coins ---
    public static int GetTotalCoins() => PlayerPrefs.GetInt(CoinsKey, 0);
    public static void AddCoins(int amount) => PlayerPrefs.SetInt(CoinsKey, GetTotalCoins() + amount);
    public static bool HasEnoughCoins(int cost) => GetTotalCoins() >= cost;
    public static void SpendCoins(int amount) => PlayerPrefs.SetInt(CoinsKey, GetTotalCoins() - amount);

    // --- High Score ---
    public static int GetHighScore() => PlayerPrefs.GetInt(HighScoreKey, 0);
    public static void SetHighScore(int newScore)
    {
        if (newScore > GetHighScore())
            PlayerPrefs.SetInt(HighScoreKey, newScore);
    }

    // --- Player Unlocking (int index) ---
    public static void UnlockPlayer(int index) => PlayerPrefs.SetInt(PlayerKeyPrefix + index, 1);
    public static bool IsPlayerUnlocked(int index) => PlayerPrefs.GetInt(PlayerKeyPrefix + index, index == 0 ? 1 : 0) == 1;

    // --- Player Unlocking (string ID) ---
    public static void UnlockPlayer(string playerId) => PlayerPrefs.SetInt(PlayerKeyPrefix + playerId, 1);
    public static bool IsPlayerUnlocked(string playerId) => PlayerPrefs.GetInt(PlayerKeyPrefix + playerId, playerId == "Default" ? 1 : 0) == 1;

    // --- Player Selection ---
    public static void SetSelectedPlayer(int index) => PlayerPrefs.SetInt(SelectedPlayerKey, index);
    public static int GetSelectedPlayer() => PlayerPrefs.GetInt(SelectedPlayerKey, 0);

    // --- Player Selection (string ID) ---
    public static void SetSelectedPlayer(string playerId) => PlayerPrefs.SetString(SelectedPlayerKey, playerId);
    public static string GetSelectedPlayerId() => PlayerPrefs.GetString(SelectedPlayerKey, "Default");

    // --- Save manually if needed ---
    public static void Save() => PlayerPrefs.Save();
}


