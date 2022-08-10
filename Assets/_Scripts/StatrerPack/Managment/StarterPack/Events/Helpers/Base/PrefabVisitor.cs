using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class PrefabVisitor
{
    protected static List<Type> types = new List<Type>();
    protected AttributeSearchHelper<IInputHandler, InputCallbackAttribute> attributeSearchHelper = new AttributeSearchHelper<IInputHandler, InputCallbackAttribute>();

    protected List<Type> SearchForTypesOfInteface<T>()
    {
        var type = typeof(T);
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p)).ToList();
        
    }

    protected static List<string> FindAllPrefabs<T>(string[] folders, bool findDependency = true)
    {
        List<string> result = new List<string>();
        string[] guids = AssetDatabase.FindAssets("", folders);

        string path;
        for (int i = 0; i < guids.Length; i++)
        {
            path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (findDependency && path.EndsWith(".prefab"))
            {
                SearchForDependenciesInTypes<T>(result, path);
            }
        }
        return result;
    }

    protected static void SearchForDependenciesInTypes<T>(List<string> result, string path)
    {
        string[] dependencies = AssetDatabase.GetDependencies(path, false);
        for (int j = 0; j < dependencies.Length; j++)
        {
            string scriptName = GetScriptName(dependencies[j]);
            if (scriptName.EndsWith(".cs"))
            {
                Type type = typeof(T).Assembly.GetType(scriptName.Remove(scriptName.Length - 3));
                if (types.Contains(type))
                {
                    result.Add(path);
                }
            }
        }
    }

    public static string GetScriptName(string dependency)
    {
        string[] pathSplit = dependency.Split('/');
        string scriptName = pathSplit[pathSplit.Length - 1];
        return scriptName;
    }

    public List<HandlerType> SearchForHandlers<AttributeType, HandlerType>(GameObject root) where AttributeType : Attribute
    {
        return new List<HandlerType>(root.GetComponentsInChildren<HandlerType>());
    }
}
#endif