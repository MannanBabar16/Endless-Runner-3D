using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Cinemachine;


public class GameManager : MonoBehaviour
{
    [Header("Segment Management")]
    public List<string> segmentTags;
    public Vector3 startSpawnPosition;
    public Vector3 offset;
    public float spawnTrigger;
    public float destroyDistance;

    [Header("Coin & Obstacle Settings")]
    public string coinTag = "Coin";
    public string obstacleTag = "Obstacle";
    public int coinsPerChunk = 5;
    public int maxObstaclesPerSegment = 3;
    public float obstacleY = 0.5f;
    public float coinY = 1f;

    public float[] laneX = new float[] { -3f, 0f, 3f };

    public int coinCount = 0;
    public UnityEvent<int> onCoinCollected = new UnityEvent<int>();

    [Header("UI")]
    public GameObject gameOverPanel;

    private Vector3 previousPosition;
    private Queue<GameObject> activeSegments = new Queue<GameObject>();
    private List<GameObject> activeCoins = new List<GameObject>();
    private List<GameObject> activeObstacles = new List<GameObject>();

    private bool gameOver = false;

    [Header("Power-Up Settings")]
    public string invisibilityTag = "Invisibility";
    public string magnetTag = "Magnet";
    public float powerupSpawnChance = 0.2f;
    public float invisibilityChance = 0.5f;
    public float magnetChance = 0.5f;
    public float powerupY = 1f;
    private List<GameObject> activePowerups = new List<GameObject>();

    public int gameScore = 0;
    public UnityEvent<int> onScoreUpdated = new UnityEvent<int>();
    public float scoreRate = 100f;

    [Header("Player Prefabs")]
    public GameObject[] playerPrefabs;

    [Header("Spawn Settings")]
    public Vector3 spawnPoint;

    public UiManager uiManager;

    private GameObject currentPlayer;
    private Transform playerTransform;

    void Start()
    {
        SpawnFirstSegment();
        SpawnSelectedPlayer();
    }

    void Update()
    {
        if (gameOver || playerTransform == null) return;

        if (playerTransform.position.z + spawnTrigger > previousPosition.z)
        {
            SpawnNextSegment();
        }

        CleanupObjects();

        gameScore += Mathf.FloorToInt(scoreRate * Time.deltaTime);
        onScoreUpdated.Invoke(gameScore);
    }

    void SpawnSelectedPlayer()
    {
        int selectedIndex = SaveData.GetSelectedPlayer();
        currentPlayer = Instantiate(playerPrefabs[selectedIndex], spawnPoint, Quaternion.identity);
        playerTransform = currentPlayer.transform;

        PlayerController controller = currentPlayer.GetComponent<PlayerController>();
        if (controller != null && uiManager != null)
        {
            controller.uiManager = uiManager;
        }
        
        CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            vcam.Follow = currentPlayer.transform;
            vcam.LookAt = currentPlayer.transform;
        }
    }

    void SpawnFirstSegment()
    {
        GameObject first = ObjectPooler.Instance.SpawnFromPool(segmentTags[0], startSpawnPosition, Quaternion.identity);
        activeSegments.Enqueue(first);
        previousPosition = startSpawnPosition;
    }

    void SpawnNextSegment()
    {
        previousPosition += offset;
        string tag = segmentTags[Random.Range(0, segmentTags.Count)];
        GameObject segment = ObjectPooler.Instance.SpawnFromPool(tag, previousPosition, Quaternion.identity);
        activeSegments.Enqueue(segment);

        SpawnCoins(previousPosition);
        SpawnObstacles(previousPosition);
        SpawnPowerUp(previousPosition);
    }

    void SpawnCoins(Vector3 segmentPos)
    {
        float x = laneX[Random.Range(0, laneX.Length)];
        float startZ = segmentPos.z + 2f;

        for (int i = 0; i < coinsPerChunk; i++)
        {
            Vector3 pos = new Vector3(x, coinY, startZ + i * 2f);
            GameObject coin = ObjectPooler.Instance.SpawnFromPool(coinTag, pos, Quaternion.identity);
            activeCoins.Add(coin);
        }
    }

    void SpawnObstacles(Vector3 segmentPos)
    {
        int obstacleCount = Random.Range(1, maxObstaclesPerSegment + 1);
        List<Vector3> spawnedPositions = new List<Vector3>();
        float minDistanceBetweenObstacles = 20f;
        int attemptsMax = 20;

        for (int i = 0; i < obstacleCount; i++)
        {
            int attempts = 0;
            Vector3 candidatePos;

            do
            {
                float x = laneX[Random.Range(0, laneX.Length)];
                float z = segmentPos.z + Random.Range(2f, offset.z - 2f);
                candidatePos = new Vector3(x, obstacleY, z);
                attempts++;

                bool tooClose = false;
                foreach (var pos in spawnedPositions)
                {
                    if (Vector3.Distance(new Vector3(candidatePos.x, 0, candidatePos.z), new Vector3(pos.x, 0, pos.z)) < minDistanceBetweenObstacles)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose) break;

            } while (attempts < attemptsMax);

            if (attempts == attemptsMax) continue;

            string selectedTag = Random.value < 0.5f ? "JumpObstacle" : "SlideObstacle";
            GameObject obstacle = ObjectPooler.Instance.SpawnFromPool(selectedTag, candidatePos, Quaternion.identity);
            activeObstacles.Add(obstacle);
            spawnedPositions.Add(candidatePos);
        }
    }

    void SpawnPowerUp(Vector3 segmentPos)
    {
        if (Random.value > powerupSpawnChance) return;

        float x = laneX[Random.Range(0, laneX.Length)];
        float z = segmentPos.z + Random.Range(4f, offset.z - 4f);
        Vector3 pos = new Vector3(x, powerupY, z);

        string tag = Random.value < invisibilityChance ? invisibilityTag : magnetTag;
        GameObject powerup = ObjectPooler.Instance.SpawnFromPool(tag, pos, Quaternion.identity);
        activePowerups.Add(powerup);
    }

    void CleanupObjects()
    {
        float playerZ = playerTransform.position.z;

        while (activeSegments.Count > 0 && playerZ - activeSegments.Peek().transform.position.z > destroyDistance)
        {
            GameObject seg = activeSegments.Dequeue();
            seg.SetActive(false);
        }

        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            if (playerZ - activeCoins[i].transform.position.z > destroyDistance)
            {
                activeCoins[i].SetActive(false);
                activeCoins.RemoveAt(i);
            }
        }

        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (playerZ - activeObstacles[i].transform.position.z > destroyDistance)
            {
                activeObstacles[i].SetActive(false);
                activeObstacles.RemoveAt(i);
            }
        }

        for (int i = activePowerups.Count - 1; i >= 0; i--)
        {
            if (playerZ - activePowerups[i].transform.position.z > destroyDistance)
            {
                activePowerups[i].SetActive(false);
                activePowerups.RemoveAt(i);
            }
        }
    }

    public void GameOver()
    {
        gameOver = true;
        gameOverPanel.SetActive(true);

        if (currentPlayer != null)
        {
            var controller = currentPlayer.GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = false;
        }

        SaveData.AddCoins(coinCount);
        SaveData.SetHighScore(gameScore);
    }

    public void AddCoin()
    {
        Debug.Log("Coin collected!");
    }

    public void CollectCoin()
    {
        coinCount++;
        onCoinCollected.Invoke(coinCount);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
