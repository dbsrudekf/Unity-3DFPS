using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CasingMemoryPool : MonoBehaviour
{
    [SerializeField]
    private GameObject casingPrefab;
    private MemoryPool memoryPool;
    private MemoryPool SecmemoryPool;
    public void Awake()
    {
        //Debug.Log("casing");
        //Debug.Log(memoryPool);
        memoryPool = new MemoryPool(casingPrefab);
        //Debug.Log("after" + " " + memoryPool);
        //BossMemory();
    }

    public void BossCasingMemory()
    {
        //Debug.Log("bossmemory");
        //Debug.Log(SecmemoryPool);
        SecmemoryPool = new MemoryPool(casingPrefab);
        //Debug.Log("after" + " " + SecmemoryPool);
        //Debug.Log(SecmemoryPool.poolItemList);

    }


    public void SpawnCasing(Vector3 position, Vector3 direction)
    {
        GameObject item = memoryPool.ActivatePoolItem();
        item.transform.position = position;
        item.transform.rotation = Random.rotation;
        item.GetComponent<Casing>().Setup(memoryPool, direction);
    }
}
