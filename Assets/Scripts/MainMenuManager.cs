using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
   public TextMeshProUGUI highScoreText;
   public TextMeshProUGUI totalCoinsText;
   public GameObject StorePanel;
   
       void Start()
       {
           UpdateStats();
       }
   
       void UpdateStats()
       {
           highScoreText.text = "" + SaveData.GetHighScore();
           totalCoinsText.text = "" + SaveData.GetTotalCoins();
       }
   
       public void OnPlayPressed()
       {
           SceneManager.LoadScene("Gameplay");
       }
   
       public void OnQuitPressed()
       {
           Application.Quit();
       }
   
       public void OnOpenStore()
       {
           // Activate store UI panel
           Debug.Log("Store opened");
           StorePanel.SetActive(true);
       }
       
       public void OnCloseStore()
       {
      
           StorePanel.SetActive(false);
       }
}
