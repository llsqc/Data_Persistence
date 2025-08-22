using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Excel;
using UnityEditor;
using UnityEngine;

public class ExcelTool
{
    public static string EXCEL_PATH = Application.dataPath + "/ArtRes/Excel/";
    public static string DATA_CLASS_PATH = Application.dataPath + "/Scripts/ExcelData/DataClass/";
    public static string DATA_CONTAINER_PATH = Application.dataPath + "/Scripts/ExcelData/Container/";
    public static string DATA_BINARY_PATH = Application.streamingAssetsPath + "/Binary/";
    public static int BEGIN_INDEX = 4;

    [MenuItem("GameTool/GenerateExcel")]
    private static void GenerateExcelInfo()
    {
        DirectoryInfo dir = Directory.CreateDirectory(EXCEL_PATH);
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            if (file.Extension != ".xlsx" && file.Extension != ".xls")
                continue;

            DataTableCollection dataTableCollection;
            using (FileStream fs = file.Open(FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(fs);
                dataTableCollection = reader.AsDataSet().Tables;
                fs.Close();
            }

            foreach (DataTable table in dataTableCollection)
            {
                Debug.Log(table.TableName);
                GenerateExcelDataClass(table);
                GenerateExcelContainer(table);
                GenerateExcelBinary(table);
            }
        }
    }

    private static void GenerateExcelDataClass(DataTable table)
    {
        DataRow rowName = GetVariableNameRow(table);
        DataRow rowType = GetVariableTypeRow(table);

        if (!Directory.Exists(DATA_CLASS_PATH))
            Directory.CreateDirectory(DATA_CLASS_PATH);

        string str = $"public class {table.TableName}\n{{\n";

        for (int i = 0; i < table.Columns.Count; i++)
        {
            str += $"    public {rowType[i]} {rowName[i]};\n";
        }

        str += "}";
        File.WriteAllText(DATA_CLASS_PATH + table.TableName + ".cs", str);
        AssetDatabase.Refresh();
    }

    private static void GenerateExcelContainer(DataTable table)
    {
        int keyIndex = GetKeyIndex(table);
        DataRow rowType = GetVariableTypeRow(table);

        if (!Directory.Exists(DATA_CONTAINER_PATH))
            Directory.CreateDirectory(DATA_CONTAINER_PATH);
        string str = "using System.Collections.Generic;\n";
        str += $"public class {table.TableName}Container\n{{\n";
        str +=
            $"    public Dictionary<{rowType[keyIndex]}, {table.TableName}> dataDic = new Dictionary<{rowType[keyIndex]}, {table.TableName}>();\n";
        str += "}";
        File.WriteAllText(DATA_CONTAINER_PATH + table.TableName + "Container.cs", str);
        AssetDatabase.Refresh();
    }

    private static void GenerateExcelBinary(DataTable table)
    {
        if (!Directory.Exists(DATA_BINARY_PATH))
            Directory.CreateDirectory(DATA_BINARY_PATH);

        using (FileStream fs = new FileStream(DATA_BINARY_PATH + table.TableName + ".bytes", FileMode.OpenOrCreate,
                   FileAccess.Write))
        {
            fs.Write(BitConverter.GetBytes(table.Rows.Count - BEGIN_INDEX), 0, 4);
            string keyName = GetVariableNameRow(table)[GetKeyIndex(table)].ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(keyName);
            fs.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
            fs.Write(bytes, 0, bytes.Length);

            DataRow row;
            DataRow rowType = GetVariableTypeRow(table);
            for (int i = BEGIN_INDEX; i < table.Rows.Count; i++)
            {
                row = table.Rows[i];
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    switch (rowType[j].ToString())
                    {
                        case "int":
                            fs.Write(BitConverter.GetBytes(int.Parse(row[j].ToString())), 0, 4);
                            break;
                        case "float":
                            fs.Write(BitConverter.GetBytes(float.Parse(row[j].ToString())), 0, 4);
                            break;
                        case "bool":
                            fs.Write(BitConverter.GetBytes(bool.Parse(row[j].ToString())), 0, 1);
                            break;
                        case "string":
                            bytes = Encoding.UTF8.GetBytes(row[j].ToString());
                            fs.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
                            fs.Write(bytes, 0, bytes.Length);
                            break;
                    }
                }
            }

            fs.Close();
        }

        AssetDatabase.Refresh();
    }

    private static DataRow GetVariableNameRow(DataTable table)
    {
        return table.Rows[0];
    }

    private static DataRow GetVariableTypeRow(DataTable table)
    {
        return table.Rows[1];
    }

    private static int GetKeyIndex(DataTable table)
    {
        DataRow row = table.Rows[2];
        for (int i = 0; i < row.ItemArray.Length; i++)
        {
            if (row[i].ToString() == "key")
                return i;
        }

        return -1;
    }
}