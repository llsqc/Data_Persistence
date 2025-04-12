using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class XMLDataMgr
{
    private static XMLDataMgr _instance = new XMLDataMgr();

    public static XMLDataMgr Instance => _instance;

    private XMLDataMgr()
    {
    }

    public void SaveData(object data, string fileName)
    {
        string path = $"{Application.persistentDataPath}/{fileName}.xml";

        using (StreamWriter writer = new StreamWriter(path))
        {
            XmlSerializer serializer = new XmlSerializer(data.GetType());
            serializer.Serialize(writer, data);
        }
    }

    public object LoadData(Type type, string fileName)
    {
        string path = $"{Application.persistentDataPath}/{fileName}.xml";

        if (!File.Exists(path)) path = $"{Application.streamingAssetsPath}/{fileName}.xml";
        if (!File.Exists(path)) return Activator.CreateInstance(type);
        
        using (StreamReader reader = new StreamReader(path))
        {
            XmlSerializer serializer = new XmlSerializer(type);
            return serializer.Deserialize(reader);
        }
    }
}