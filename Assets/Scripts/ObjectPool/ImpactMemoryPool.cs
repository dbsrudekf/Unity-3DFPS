using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ImpactType { Normal = 0, Obstacle, Enemy, InterationObject, }
public class ImpactMemoryPool : MonoBehaviour
{
    [SerializeField]
    private GameObject[] impactPrefab;
    private MemoryPool[] memoryPool;
    private MemoryPool[] SecmemoryPool;
    private void Awake()
    {
        memoryPool = new MemoryPool[impactPrefab.Length];
        for (int i = 0; i < impactPrefab.Length; ++i)
        {
            memoryPool[i] = new MemoryPool(impactPrefab[i]);
        }
    }


    public void SpawnImpact(RaycastHit hit)
    {
        if(hit.transform.CompareTag("ImpactNormal"))
        {
            OnSpawnImpact(ImpactType.Normal, hit.point, Quaternion.LookRotation(hit.normal));
        }
        else if(hit.transform.CompareTag("ImpactObstacle"))
        {
            OnSpawnImpact(ImpactType.Obstacle, hit.point, Quaternion.LookRotation(hit.normal));
        }
        else if(hit.transform.CompareTag("ImpactEnemy"))
        {
            OnSpawnImpact(ImpactType.Enemy, hit.point, Quaternion.LookRotation(hit.normal));
        }
        else if(hit.transform.CompareTag("InteractionObject"))
        {
            Color color = hit.transform.GetComponentInChildren<MeshRenderer>().material.color;
            OnSpawnImpact(ImpactType.InterationObject, hit.point, Quaternion.LookRotation(hit.normal), color);
        }
    }

    public void SpawnImpact(Collider other, Transform knifeTransform)
    {
        if(other.CompareTag("ImpactNormal"))
        {
            OnSpawnImpact(ImpactType.Normal, knifeTransform.position, Quaternion.Inverse(knifeTransform.rotation));
        }
        else if(other.CompareTag("ImpactObstacle"))
        {
            OnSpawnImpact(ImpactType.Obstacle, knifeTransform.position, Quaternion.Inverse(knifeTransform.rotation));
        }
        else if (other.CompareTag("ImpactEnemy"))
        {
            OnSpawnImpact(ImpactType.Enemy, knifeTransform.position, Quaternion.Inverse(knifeTransform.rotation));
        }
        else if (other.CompareTag("InteractionObject"))
        {
            Color color = other.transform.GetComponentInChildren<MeshRenderer>().material.color;
            OnSpawnImpact(ImpactType.InterationObject, knifeTransform.position, Quaternion.Inverse(knifeTransform.rotation), color);
        }
    }

    public void OnSpawnImpact(ImpactType type, Vector3 position, Quaternion rotation, Color color = new Color())
    {
        GameObject item = memoryPool[(int)type].ActivatePoolItem();
        item.transform.position = position;
        item.transform.rotation = rotation;
        item.GetComponent<Impact>().Setup(memoryPool[(int)type]);

        if(type == ImpactType.InterationObject)
        {
            ParticleSystem.MainModule main = item.GetComponent<ParticleSystem>().main;
            main.startColor = color;
        }
    }
}
