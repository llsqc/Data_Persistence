using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public class BinaryDataMgr
{
    public static string DATA_BINARY_PATH = Application.streamingAssetsPath + "/Binary/";

    private Dictionary<string, object> tableDic = new Dictionary<string, object>();

    private static string SAVE_PATH = Application.persistentDataPath + "/BinaryData/";

    private static BinaryDataMgr _instance = new BinaryDataMgr();
    public static BinaryDataMgr Instance => _instance;

    private BinaryDataMgr()
    {
    }

    public void InitData()
    {
        LoadTable<PlayerInfoContainer, PlayerInfo>();
    }

    public void LoadTable<TContainer, TData>()
    {
        using (FileStream fs = File.Open(DATA_BINARY_PATH + typeof(TData).Name + ".bytes", FileMode.Open,
                   FileAccess.Read))
        {
            byte[] bytes = new byte[fs.Length];
            int read = fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            int index = 0;

            int count = BitConverter.ToInt32(bytes, index);
            index += 4;

            int keyNameLength = BitConverter.ToInt32(bytes, index);
            index += 4;
            string keyName = Encoding.UTF8.GetString(bytes, index, keyNameLength);
            index += keyNameLength;

            Type containerType = typeof(TContainer);
            object containerObject = Activator.CreateInstance(containerType);

            Type classType = typeof(TData);
            FieldInfo[] infos = classType.GetFields();

            for (int i = 0; i < count; i++)
            {
                object instance = Activator.CreateInstance(classType);
                foreach (FieldInfo info in infos)
                {
                    if (info.FieldType == typeof(int))
                    {
                        info.SetValue(instance, BitConverter.ToInt32(bytes, index));
                        index += 4;
                    }

                    else if (info.FieldType == typeof(float))
                    {
                        info.SetValue(instance, BitConverter.ToSingle(bytes, index));
                        index += 4;
                    }

                    else if (info.FieldType == typeof(bool))
                    {
                        info.SetValue(instance, BitConverter.ToBoolean(bytes, index));
                        index += 1;
                    }

                    else if (info.FieldType == typeof(string))
                    {
                        int length = BitConverter.ToInt32(bytes, index);
                        index += 4;
                        info.SetValue(instance, Encoding.UTF8.GetString(bytes, index, length));
                        index += length;
                    }
                }

                object dic = containerType.GetField("dataDic").GetValue(containerObject);
                MethodInfo methodInfo = dic.GetType().GetMethod("Add");
                object keyValue = classType.GetField(keyName).GetValue(instance);
                methodInfo?.Invoke(dic, new object[] { keyValue, instance });
            }

            tableDic.Add(typeof(TContainer).Name, containerObject);
            fs.Close();
        }
    }

    public T GetTable<T>() where T : class
    {
        string tableName = typeof(T).Name;
        if (tableDic.TryGetValue(tableName, out var value))
            return value as T;
        return null;
    }

    public void Save(object obj, string fileName)
    {
        using (FileStream fs = new FileStream(SAVE_PATH + fileName + ".bytes", FileMode.OpenOrCreate, FileAccess.Write))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, obj);
            fs.Close();
        }
    }

    public T Load<T>(string fileName) where T : class
    {
        if (!File.Exists(SAVE_PATH + fileName + ".bytes"))
            return null;

        T data;
        using (FileStream fs = new FileStream(SAVE_PATH + fileName + ".bytes", FileMode.Open, FileAccess.Read))
        {
            BinaryFormatter bf = new BinaryFormatter();
            data = bf.Deserialize(fs) as T;
            fs.Close();
        }

        return data;
    }
}