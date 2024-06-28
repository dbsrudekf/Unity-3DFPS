using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CutSceneCamera : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera virtualCam;
    private Transform target;
    [SerializeField]
    private GameObject virtualSecCam;
    [SerializeField]
    private GameObject colliderObj;
    [SerializeField]
    private float orbitSpeed;
    Vector3 offset;

    [SerializeField]
    private TimelineAsset ta;
    private PlayableDirector pd;

    [SerializeField]
    private Camera MainCamera;

    private bool bIsCol = false;

    [SerializeField]
    private GameObject canvasUI;

    private void Awake()
    {
        pd = GetComponent<PlayableDirector>();
        
        target = virtualCam.LookAt.transform;
        offset = transform.position - target.position;

        canvasUI.SetActive(false);

        StartCoroutine("orbitCollider");
    }

    private void Update()
    {
        if(!bIsCol)
        {
            transform.position = target.position + offset;
            transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime);
            offset = transform.position - target.position;
        }            
    }

    private IEnumerator orbitCollider()
    {
        yield return new WaitForSeconds(0.1f);

        colliderObj.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "orbitCollider")
        {
            //pd.Play(ta);
            bIsCol = true;
            //virtualSecCam.gameObject.SetActive(true);
            other.gameObject.SetActive(false);
            canvasUI.SetActive(true);
            gameObject.SetActive(false);
        }

        
    }
}
