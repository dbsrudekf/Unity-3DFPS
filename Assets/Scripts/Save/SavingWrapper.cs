using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavingWrapper : MonoBehaviour
{
    const string defaultSaveFile = "save";
    [SerializeField] float fadeInTime = 0.2f;

    private void Awake()
    {
        StartCoroutine(LoadLastScene());
    }

    private IEnumerator LoadLastScene()
    {
        //yield return GetComponent<SavingSystem>().LoadLastScene(defaultSaveFile);
        //Fader fader = FindObjectOfType<Fader>();
        //fader.FadeOutImemediate(); //카메라 조정 위한 페이드
        //yield return fader.FadeIn(fadeInTime);
        yield return null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Load();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            Delete();
        }
    }

    public void Load()
    {
        GetComponent<SavingSystem>().Load(defaultSaveFile);
    }

    public void Save()
    {
        GetComponent<SavingSystem>().Save(defaultSaveFile);
    }

    public void Delete()
    {
        GetComponent<SavingSystem>().Delete(defaultSaveFile);
    }
}
