using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class LineCounterWindow : EditorWindow
{
    private Vector2 scrollPos;

    private static int logErrorLineCount = 150;
    private static int exceptionLineCount = 200;
    private static int exceptionsSize = 0;

    private static bool isBlockPlayMode = true;

    private static List<string> exceptions = new List<string>(5);

    [MenuItem("Window/Line Counter Settings")]
    public static void ShowWindow()
    {
        ConfigData configData = LineCounter.GetData();

        logErrorLineCount = configData.logErrorLineCount;
        exceptionLineCount = configData.exceptionLineCount;
        isBlockPlayMode = configData.isBlockPlayMode;
        exceptions = configData.exceptions.ToList();
        exceptionsSize = exceptions.Count;

        GetWindow<LineCounterWindow>("Line Counter Settings ");
    }

    private void OnGUI()
    {
        logErrorLineCount = EditorGUILayout.IntField("Line count for error", logErrorLineCount);
        exceptionLineCount = EditorGUILayout.IntField("Line count for exception", exceptionLineCount);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Only for producer", MessageType.Warning, true);

        isBlockPlayMode = EditorGUILayout.Toggle("Block playmode", isBlockPlayMode);

        EditorGUILayout.Space();
        GUILayout.Label("Exceptions: ");

        exceptionsSize = EditorGUILayout.IntField("Exceptions Count", exceptionsSize);
        exceptionsSize = exceptionsSize < 0 ? 0 : exceptionsSize;

        if (exceptions != null && exceptionsSize != exceptions.Count)
        {
            if (exceptionsSize > exceptions.Count)
            {
                for (int i = exceptions.Count; i <= exceptionsSize; i++)
                {
                    exceptions.Add("");
                }
            }
            else
            {
                for (int i = exceptions.Count - 1; i >= exceptionsSize; i--)
                {
                    exceptions.RemoveAt(i);
                }
            }
        }

        if(exceptionsSize * 20 > 100)
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(100));

        for (int i = 0; i < exceptions.Count; i++)
        {
            string name = exceptions[i];
            exceptions[i] = EditorGUILayout.TextField(name);
        }

        if (exceptionsSize * 20 > 100)
            EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        if (GUILayout.Button("Apply Changes"))
        {
            ApplyChanges();
        }
    }

    private void ApplyChanges()
    {
        ConfigData configData = new ConfigData
        {
            logErrorLineCount = logErrorLineCount,
            exceptionLineCount = exceptionLineCount,
            isBlockPlayMode = isBlockPlayMode,
            exceptions = exceptions.ToArray(),
        };
       
        LineCounterSaver.SaveData(configData);
        LineCounter.ApplyChanges();
    }

}
