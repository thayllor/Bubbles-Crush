using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveData
{
    public bool[] isActive;
    public int[] highScores;
    public int[] stars;
}
public class GameData : MonoBehaviour
{
    public static GameData gameData;
    public bool alreadyRunning;
    public SaveData saveData;
    // Start is called before the first frame update
    void Awake()
    {
        alreadyRunning = false;
        if (gameData == null)
        {
            DontDestroyOnLoad(this.gameObject);
            gameData = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        Load();
    }

    private void Start()
    {
        alreadyRunning = !alreadyRunning;

    }

    public void Save()
    {
        //cria o formatador que transforma no arquivo binario
        BinaryFormatter formatter = new BinaryFormatter();
        //abre/cria o arquivo que ficara salvo os dados
        if (File.Exists(Application.persistentDataPath + "/player.dat"))
        {
            FileStream file = File.Open(Application.persistentDataPath + "/player.dat", FileMode.Open);
            //pega os dados que existem
            SaveData data = new SaveData();
            data = saveData;
            //atribui os dados ao arquivo
            formatter.Serialize(file, data);
            //fecha o arquivo
            file.Close();
        }
        else
        {
            FileStream file = File.Open(Application.persistentDataPath + "/player.dat", FileMode.Create);
            //pega os dados que existem
            SaveData data = new SaveData();
            data = saveData;
            //atribui os dados ao arquivo
            formatter.Serialize(file, data);
            //fecha o arquivo
            file.Close();
        }
        
        Debug.Log("Saved");
    }
    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/player.dat"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/player.dat", FileMode.Open);
            saveData = formatter.Deserialize(file) as SaveData;
            file.Close();
            Debug.Log("Loaded");
        }
        else
        {
            saveData = new SaveData();
            saveData.isActive = new bool[100];
            saveData.stars = new int[100];
            saveData.highScores = new int[100];
            saveData.isActive[0] = true;
        }
    }

    private void OnDestroy()
    {
        Save();
    }
}
