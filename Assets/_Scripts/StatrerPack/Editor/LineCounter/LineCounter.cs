#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;

public class LineCounter
{
    private static int logErrorLineCount = 100;
    private static int exceptionLineCount = 200;

    private static bool isBlockPlayMode = true;

    private static string[] exceptions = new string[0];

    [InitializeOnLoadMethod]
    private static void StartEditor()
    {
        ApplyChanges();
        CountLines();
    }

    [InitializeOnEnterPlayMode]
    private static void StartEditorOnPlayMod()
    {
        if (!EditorSettings.enterPlayModeOptionsEnabled)
            return;

        StartEditor();
    }

    public static ConfigData GetData()
    {
        return new ConfigData
        {
            logErrorLineCount = logErrorLineCount,
            exceptionLineCount = exceptionLineCount,
            isBlockPlayMode = isBlockPlayMode,
            exceptions = exceptions.ToArray(),
        };
    }

    public static void ApplyChanges()
    {
        ConfigData configData = new ConfigData();
        if (LineCounterSaver.LoadData(ref configData))
        {
            logErrorLineCount = configData.logErrorLineCount;
            exceptionLineCount = configData.exceptionLineCount;
            isBlockPlayMode = configData.isBlockPlayMode;
            exceptions = configData.exceptions;
        }
    }

    private static void CountLines()
    {
        DirectoryInfo dirInfo = new DirectoryInfo(Environment.CurrentDirectory + @"\Assets\_Scripts");
        int exceptionLines = 0;

        IEnumerable files = dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories)
            .Where(w => w.Extension == ".cs" && !w.FullName.Contains("StarterPack") && !w.FullName.Contains("TTM"));

        string message = "";
        string exceptionMessage = "";

        foreach (FileInfo fileInfo in files)
        {
            bool isException = false;
            for (int i = 0; i < exceptions.Length; i++)
            {
                if (fileInfo.Name.Contains(exceptions[i]))
                {
                    isException = true;
                    break;
                }
            }

            if (isException)
                continue;

            int lineCount = File.ReadAllLines(fileInfo.FullName).Count();

            if (lineCount >= exceptionLineCount)
            {
                exceptionLines++;
                exceptionMessage += fileInfo.Name + " этот скрипт содержит следующее количество строчек - <color=red>" + lineCount + "</color>. Видимо надо рефакторить :D \n";
                continue;
            }

            if (lineCount >= logErrorLineCount)
            {
                message += fileInfo.Name + " этот скрипт содержит следующее количество строчек - <color=yellow>" + lineCount + "</color> . Пожалуйста, воздержитесь от его расширения \n";
                continue;
            }
        }

        if (message.Length > 0)
            Debug.Log(message);

        if (exceptionLines > 0)
        {
            if(isBlockPlayMode)
                UnityEditor.EditorApplication.isPlaying = false;

            Debug.LogException(new LineCountException(exceptionMessage));
        }
    }
}
#endif