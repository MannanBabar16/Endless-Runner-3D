using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Segment Management")]
    public List<string> segmentTags;
    public Transform player;
    public Vector3 startSpawnPosition;
    public Vector3 offset;
    public float spawnTrigger;
    public float destroyDistance;

    [Header("Coin & Obstacle Settings")]
    public string coinTag = "Coin";
    public string obstacleTag = "Obstacle";
    public int coinsPerChunk = 5;
    public int maxObstaclesPerSegment = 3;
    public float obstacleY = 0.5f; // Adjust based on obstacle prefab pivot
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

    public float invisibilityChance = 0.5f; // 50%
    
    public float magnetChance = 0.5f; // 50%

    public float powerupY = 1f;
    private List<GameObject> activePowerups = new List<GameObject>();


    void Start()
    {
        SpawnFirstSegment();

    }

    void Update()
    {
        if (gameOver) return;

        if (player.position.z + spawnTrigger > previousPosition.z)
        {
            SpawnNextSegment();
        }

        CleanupObjects();
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
            Vector3 pos = new Vector3(x, coinY, startZ + i * 2f); // even spacing
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

                // Ensure enough space from previous
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

            if (attempts == attemptsMax)
                continue;

            // 🔀 Randomize obstacle type
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
        // Segments
        while (activeSegments.Count > 0 && player.position.z - activeSegments.Peek().transform.position.z > destroyDistance)
        {
            GameObject seg = activeSegments.Dequeue();
            seg.SetActive(false);
        }

        // Coins
        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            if (player.position.z - activeCoins[i].transform.position.z > destroyDistance)
            {
                activeCoins[i].SetActive(false);
                activeCoins.RemoveAt(i);
            }
        }

        // Obstacles
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (player.position.z - activeObstacles[i].transform.position.z > destroyDistance)
            {
                activeObstacles[i].SetActive(false);
                activeObstacles.RemoveAt(i);
            }
        }
        
        //Power Ups
        for (int i = activePowerups.Count - 1; i >= 0; i--)
        {
            if (player.position.z - activePowerups[i].transform.position.z > destroyDistance)
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
        player.GetComponent<PlayerController>().enabled = false;
    }

    public void AddCoin()
    { 
        // This can be expanded to update UI, currency, etc.
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
