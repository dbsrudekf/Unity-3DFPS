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
        if (!File.Exists(path)) //저장된 파일 존재 확인
        {
            return new Dictionary<string, object>();
        }
        using (FileStream stream = File.Open(path, FileMode.Open))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (Dictionary<string, object>)formatter.Deserialize(stream); //바이너리 파일을 역직렬화
        }
    }

    private void SaveFile(string saveFile, object state)
    {
        string path = GetPathFromSaveFile(saveFile);
        print("Saving to " + path);
        using (FileStream stream = File.Open(path, FileMode.Create)) //파일열기 없다면 생성
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, state); //state의 Object를 바이너리파일로 직렬화
        }
    }

    private void CaptureState(Dictionary<string, object> state)
    {
        foreach (SaveableEntity saveable in FindObjectsOfType<SaveableEntity>()) //SaveableEntity를 갖고 있는 모든 오브젝트 순회
        {
            state[saveable.GetUniqueIdentifier()] = saveable.CaptureState(); //모든 저장가능한 오브젝트의 상태 저장 state의 고유키값으로 구분
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
