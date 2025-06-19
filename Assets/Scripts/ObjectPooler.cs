using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
   public static ObjectPooler Instance;
   
       [System.Serializable]
       public class Pool
       {
           public string tag;
           public GameObject prefab;
           public int size;
       }
   
       public List<Pool> pools;
       private Dictionary<string, Queue<GameObject>> poolDictionary;
   
       void Awake()
       {
           Instance = this;
       }
   
       void Start()
       {
           poolDictionary = new Dictionary<string, Queue<GameObject>>();
   
           foreach (var pool in pools)
           {
               Queue<GameObject> objectPool = new Queue<GameObject>();
   
               for (int i = 0; i < pool.size; i++)
               {
                   GameObject obj = Instantiate(pool.prefab);
                   obj.SetActive(false);
                   objectPool.Enqueue(obj);
               }
   
               poolDictionary[pool.tag] = objectPool;
           }
       }
   
       public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
       {
           if (!poolDictionary.ContainsKey(tag))
           {
               return null;
           }
   
           GameObject obj = poolDictionary[tag].Dequeue();
   
           obj.SetActive(true);
           obj.transform.position = position;
           obj.transform.rotation = rotation;
   
           poolDictionary[tag].Enqueue(obj); 
   
           return obj;
       }
}
