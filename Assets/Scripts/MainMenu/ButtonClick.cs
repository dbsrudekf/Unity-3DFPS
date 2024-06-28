using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonClick : MonoBehaviour
{
    private const string defaultSaveFile = "save";


    public void OnClickStart()
    {
        SceneManager.LoadScene("Stage");
        GetComponent<SavingSystem>().Delete(defaultSaveFile);

    }

    public void OnClickLoad()
    {
        SceneManager.LoadScene("Stage");
        GetComponent<SavingSystem>().Load(defaultSaveFile);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}
