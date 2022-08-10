#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;

public class EditorConfigInit
{

    [InitializeOnLoadMethod]
    private static void InitEditorConfig()
    {
        DirectoryInfo dirInfo = new DirectoryInfo(Environment.CurrentDirectory);

        List<FileInfo> editorConfigFiles = dirInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly)
            .Where(s => s.Extension == ".editorconfig").ToList();

        if (editorConfigFiles.Count > 0)
        {
            return;
        }

        IEnumerable editorConfigText = dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories)
            .Where(s => s.Name == "EditorConfig.txt");
        
        foreach (FileInfo file in editorConfigText)
        {
            string[] allLines = File.ReadAllLines(file.FullName);
            StreamWriter streamWriter = File.CreateText(Environment.CurrentDirectory + "/.editorconfig");
            for (int i = 0; i < allLines.Length; i++)
            {
                streamWriter.WriteLine(allLines[i]);
            }

            return;
        }
    }

}
#endif