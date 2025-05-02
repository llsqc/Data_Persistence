using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine;

public enum JsonType
{
    JsonUtlity,
    LitJson
}

public class JsonMgr
{
    private static JsonMgr _instance = new JsonMgr();
    public static JsonMgr Instance => _instance;

    private JsonMgr()
    {
    }

    public void SaveData(object data, string fileName, JsonType type = JsonType.LitJson)
    {
        var path = Application.persistentDataPath + "/" + fileName + ".json";
        var jsonStr = type switch
        {
            JsonType.JsonUtlity => JsonUtility.ToJson(data),
            JsonType.LitJson => JsonMapper.ToJson(data),
            _ => ""
        };

        File.WriteAllText(path, jsonStr);
    }

    public T LoadData<T>(string fileName, JsonType type = JsonType.LitJson) where T : new()
    {
        var path = Application.streamingAssetsPath + "/" + fileName + ".json";
        if (!File.Exists(path))
        {
            path = Application.persistentDataPath + "/" + fileName + ".json";
        }

        if (!File.Exists(path))
        {
            return new T();
        }

        var jsonStr = File.ReadAllText(path);

        var data = type switch
        {
            JsonType.JsonUtlity => JsonUtility.FromJson<T>(jsonStr),
            JsonType.LitJson => JsonMapper.ToObject<T>(jsonStr),
            _ => default(T)
        };

        return data;
    }
}