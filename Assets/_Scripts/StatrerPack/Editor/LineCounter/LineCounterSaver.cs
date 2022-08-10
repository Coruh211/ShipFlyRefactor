using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Xml;

public class LineCounterSaver
{
    public static void SaveData(ConfigData configData)
    {
        string path = Environment.CurrentDirectory + @"\Assets\_Scripts\Editor\LineCounter\CounterConfig.txt";

        string json = JsonUtility.ToJson(configData);
        File.WriteAllText(path, json);
    }

    public static bool LoadData(ref ConfigData configData)
    {
        string path = Environment.CurrentDirectory + @"\Assets\_Scripts\Editor\LineCounter\CounterConfig.txt";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            configData = JsonUtility.FromJson<ConfigData>(json);

            return true;
        }

        return false;
    }
}

[System.Serializable]
public struct ConfigData
{
    public int logErrorLineCount;
    public int exceptionLineCount;
    public bool isBlockPlayMode;
    public string[] exceptions;
}
