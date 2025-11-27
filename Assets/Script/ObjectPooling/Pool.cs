using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour
{
    [System.Serializable]
    public class PoolItem
    {
        public string key;          
        public GameObject prefab;    
        public int size = 10;        
    }

    public List<PoolItem> poolItems = new List<PoolItem>();
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();

    void Awake()
    {
        foreach (var item in poolItems)
        {
            Queue<GameObject> newPool = new Queue<GameObject>();

            for (int i = 0; i < item.size; i++)
            {
                GameObject obj = Instantiate(item.prefab);
                obj.SetActive(false);
                newPool.Enqueue(obj);
            }

            pools[item.key] = newPool;
        }
    }

    public GameObject GetObject(string key)
    {
        if (!pools.ContainsKey(key))
        {
            Debug.LogWarning($"Pool with key '{key}' not found!");
            return null;
        }

        Queue<GameObject> selectedPool = pools[key];
        if (selectedPool.Count > 0)
        {
            GameObject obj = selectedPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        // Nếu hết, tạo thêm
        PoolItem item = poolItems.Find(p => p.key == key);
        if (item != null)
        {
            GameObject newObj = Instantiate(item.prefab);
            newObj.SetActive(true);
            return newObj;
        }

        return null;
    }

    public void ReturnObject(string key, GameObject obj)
    {
        obj.SetActive(false);
        if (!pools.ContainsKey(key))
        {
            pools[key] = new Queue<GameObject>();
        }
        pools[key].Enqueue(obj);
    }
}
