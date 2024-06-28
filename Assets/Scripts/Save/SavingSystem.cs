using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavingSystem : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    public void LoadLastScene(string saveFile)
    {
        Dictionary<string, object> state = LoadFile(saveFile);
        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        if (state.ContainsKey("lastSceneBuildIndex"))
        {
            buildIndex = (int)state["lastSceneBuildIndex"];
        }
        SceneManager.LoadScene(buildIndex);
        
        //RestoreState(state);
        Time.timeScale = 1;
    }

    public void Save(string saveFile)
    {
        Dictionary<string, object> state = LoadFile(saveFile);
        CaptureState(state);
        SaveFile(saveFile, state);
    }

    public void Load(string saveFile)
    {
        RestoreState(LoadFile(saveFile));
    }

    public void Delete(string saveFile)
    {
        File.Delete(GetPathFromSaveFile(saveFile));
    }

    private Dictionary<string, object> LoadFile(string saveFile)
    {
        string path = GetPathFromSaveFile(saveFile);
        if (!File.Exists(path)) //����� ���� ���� Ȯ��
        {
            return new Dictionary<string, object>();
        }
        using (FileStream stream = File.Open(path, FileMode.Open))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (Dictionary<string, object>)formatter.Deserialize(stream); //���̳ʸ� ������ ������ȭ
        }
    }

    private void SaveFile(string saveFile, object state)
    {
        string path = GetPathFromSaveFile(saveFile);
        print("Saving to " + path);
        using (FileStream stream = File.Open(path, FileMode.Create)) //���Ͽ��� ���ٸ� ����
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, state); //state�� Object�� ���̳ʸ����Ϸ� ����ȭ
        }
    }

    private void CaptureState(Dictionary<string, object> state)
    {
        foreach (SaveableEntity saveable in FindObjectsOfType<SaveableEntity>()) //SaveableEntity�� ���� �ִ� ��� ������Ʈ ��ȸ
        {
            state[saveable.GetUniqueIdentifier()] = saveable.CaptureState(); //��� ���尡���� ������Ʈ�� ���� ���� state�� ����Ű������ ����
        }

        state["lastSceneBuildIndex"] = SceneManager.GetActiveScene().buildIndex;
    }

    private void RestoreState(Dictionary<string, object> state)
    {
        foreach (SaveableEntity saveable in FindObjectsOfType<SaveableEntity>())
        {
            string id = saveable.GetUniqueIdentifier();
            if (state.ContainsKey(id))
            {
                saveable.RestoreState(state[id]);
            }
        }
    }

    private string GetPathFromSaveFile(string saveFile)
    {
        return Path.Combine(Application.persistentDataPath, saveFile + ".sav");
    }
}