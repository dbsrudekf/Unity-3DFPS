using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMemoryPool : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private GameObject enemySpawnPointPrefab;
    [SerializeField]
    private GameObject[] enemyPrefab;
    [SerializeField]
    private float enemySpawnTime = 1;
    [SerializeField]
    private float enemySpawnLatency = 1;

    private MemoryPool spawnPointMemoryPool;
    private MemoryPool[] enemyMemoryPool;

    private int numberOfEnemiesSpawnedAtOne = 1;
    private Vector2Int mapSize = new Vector2Int(100, 100);

    private void Awake()
    {
        spawnPointMemoryPool = new MemoryPool(enemySpawnPointPrefab);

        enemyMemoryPool = new MemoryPool[enemyPrefab.Length];

        for(int i = 0; i < enemyPrefab.Length; i++)
        {
            enemyMemoryPool[i] = new MemoryPool(enemyPrefab[i]);
        }

        StartCoroutine("SpawnTile");
    }

    private IEnumerator SpawnTile()
    {
        int currentNumber = 0;
        int maximumNumber = 50;

        while(true)
        {
            for(int i = 0; i < numberOfEnemiesSpawnedAtOne; ++i)
            {
                GameObject item = spawnPointMemoryPool.ActivatePoolItem();

                item.transform.position = new Vector3(Random.Range(-mapSize.x * 0.49f, mapSize.x * 0.49f), 1, Random.Range(-mapSize.y * 0.49f, mapSize.y * 0.49f));

                StartCoroutine("SpawnEnemy", item);
            }

            currentNumber++;

            if(currentNumber >= maximumNumber)
            {
                currentNumber = 0;
                numberOfEnemiesSpawnedAtOne++;
            }

            yield return new WaitForSeconds(enemySpawnTime);
        }
    }

    private IEnumerator SpawnEnemy(GameObject point)
    {
        yield return new WaitForSeconds(enemySpawnLatency);

        int EnemyRandomSpawn = Random.Range(0, 2);

        GameObject item = enemyMemoryPool[EnemyRandomSpawn].ActivatePoolItem();
        item.transform.position = point.transform.position;

        if(EnemyRandomSpawn == 0)
        {
            item.GetComponent<EnemyFSM>().Setup(target, this);
        }
        else
        {
            item.GetComponent<BoomEnemyFSM>().Setup(target, this);
        }     
        spawnPointMemoryPool.DeactivatePoolItem(point);

    }

    public void DeactivateEnemy(GameObject enemy, int tempEnemy)
    {
        enemyMemoryPool[tempEnemy].DeactivatePoolItem(enemy);
    }
}
