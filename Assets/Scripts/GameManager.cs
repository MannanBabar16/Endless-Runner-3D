using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<string> segmentTags; // Pool tags like "Segment1", "Segment2"...
    public Transform player;

    [SerializeField] private Vector3 startSpawnPosition;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float spawnTrigger;
    [SerializeField] private float destroyDistance;

    private Vector3 previousPosition;
    private Queue<GameObject> activeSegments = new Queue<GameObject>();

    void Start()
    {
        GameObject first = ObjectPooler.Instance.SpawnFromPool(segmentTags[0], startSpawnPosition, Quaternion.identity);
        activeSegments.Enqueue(first);
        previousPosition = startSpawnPosition;
    }

    void Update()
    {
        if (player.position.z + spawnTrigger > previousPosition.z)
        {
            SpawnNextSegment();
        }

        CleanupOldSegments();
    }

    void SpawnNextSegment()
    {
        previousPosition += offset;

        int index = Random.Range(0, segmentTags.Count);
        string selectedTag = segmentTags[index];

        GameObject segment = ObjectPooler.Instance.SpawnFromPool(selectedTag, previousPosition, Quaternion.identity);
        activeSegments.Enqueue(segment);
    }

    void CleanupOldSegments()
    {
        while (activeSegments.Count > 0)
        {
            GameObject oldest = activeSegments.Peek();

            if (player.position.z - oldest.transform.position.z > destroyDistance)
            {
                oldest.SetActive(false); // Return to pool (not destroyed)
                activeSegments.Dequeue();
            }
            else
            {
                break;
            }
        }
    }
}
