using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public static class Extensions {
    public static Vector2 ToVector2(this Vector3 vec, bool zToY = true) {
        return new Vector2(vec.x, zToY ? vec.z : vec.y);
    }
    
    public static Vector3 ToVector3(this Vector2 vec, float z = 0) {
        return new Vector3(vec.x, vec.y, z);
    }
    
    public static Vector3 ToVector3Z(this Vector2 vec, float y = 0) {
        return new Vector3(vec.x, y, vec.y);
    }

    public static Vector3 SwapXZ(this Vector3 vec)
    {
        return new Vector3(vec.z, vec.y, vec.x);
    }
    
    public static Vector3 SwapXY(this Vector3 vec)
    {
        return new Vector3(vec.y, vec.x, vec.z);
    }
    
    public static Vector3 SwapYZ(this Vector3 vec)
    {
        return new Vector3(vec.x, vec.z, vec.y);
    }

    public static Vector3 ChangeX(this Vector3 vec, float x = 0) 
    {
        return new Vector3(x, vec.y, vec.z);
    }
    
    public static Vector3 ChangeY(this Vector3 vec, float y = 0) 
    {
        return new Vector3(vec.x, y, vec.z);
    }
	
	public static Vector3 ChangeZ(this Vector3 vec, float z = 0) 
    {
        return new Vector3(vec.x, vec.y, z);
    }

    public static int RoundSinged(this float number) {
        return (int)(number > 0 ? Mathf.Floor(number) : Mathf.Ceil(number));
    }
    
    public static Color ToColor(this int HexVal) {
        byte A = (byte)((HexVal >> 24) & 0xFF);
        byte R = (byte)((HexVal >> 16) & 0xFF);
        byte G = (byte)((HexVal >> 8) & 0xFF);
        byte B = (byte)((HexVal) & 0xFF);
        return new Color(R, G, B, A);
    }
    
    public static int ToHex(this Color color) {
        int hex = 0;
        hex += (int)(color.b * 255);
        hex += (int)(color.g * 255) << 8;
        hex += (int)(color.r * 255) << 16;
        hex += (int)(color.a * 255) << 24;
        return hex;
    }
    
    public static U Get<T, U>(this Dictionary<T, U> dict, T key) where U : class {
        U val;
        dict.TryGetValue(key, out val);
        return val;
    }

    public static (int, T) GetRandom<T>(this IList<T> collection) {
        int count = collection.Count;
        int index = Random.Range(0, count);
        return (index, collection[index]);
    }

    public static T GetRandomValue<T>(this IList<T> collection) {
        int count = collection.Count;
        return collection[Random.Range(0, count)];
    }
    
    public static int GetRandomIndex<T>(this IList<T> collection, params int[] exclusive) {
        int count = collection.Count;
        
        if (exclusive.Length == 0)
            return Random.Range(0, count);
        
        int index;
        
        do
        {
            index = Random.Range(0, count);
        } while (exclusive.Contains(index));

        return index;
    }
    
    public static (int, T)[] GetRandomCollection<T>(this IList<T> collection, int count) {
        List<int> randomizeFrom = Enumerable.Range(0, collection.Count).ToList();
        (int, T)[] result = new (int, T)[count];

        if (count > collection.Count)
            count = collection.Count;
        
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, randomizeFrom.Count);
            result[i].Item1 = randomizeFrom[index];
            result[i].Item2 = collection[result[i].Item1];
            randomizeFrom.RemoveAt(index);
        }

        return result;
    }
    
    public static void SubscribeAll<T>(this List<ValueObservable<T>> list, Action action) {
        foreach (ValueObservable<T> t in list)
            t.Subscribe(action);
    }

    public static void WhenAll(this List<ValueObservable<bool>> list, Action action) {
        int count = list.Count(e => e.Value);
        list.SubscribeAll(() => {
            count++;
            if (count == list.Count)
                action.Invoke();
        });
    }

    public static void WhenEqual(this ValueObservable<int> observable, int value, Action action) {
        observable.Subscribe(() => {
            if (observable.Value == value)
                action.Invoke();
        });
    }
    
    public static Transform RecursiveFindChild(this Transform parent, string childName)
    {
        Transform child = null;
        for (int i = 0; i < parent.childCount; i++)
        {
            child = parent.GetChild(i);
            if (child.name == childName)
                break;

            child = RecursiveFindChild(child, childName);
            if (child != null)
                break;
        }

        return child;
    }
    
    public static void RecursiveIterateChildes(this Transform parent, Action<Transform> iterator)
    {
        foreach (Transform child in parent)
        {
            iterator.Invoke(child);
            RecursiveIterateChildes(child, iterator);
        }
    }
    
    public static void IterateChildes(this Transform parent, Action<Transform, int> iterator)
    {
        for (int i = 0; i < parent.childCount; i++)
            iterator.Invoke(parent.GetChild(i), i);
    }
    
    public static List<Transform> GetChildes(this Transform parent)
    {
        List<Transform> result = new List<Transform>();
        foreach (Transform transform in parent)
            result.Add(transform);

        return result;
    }

    public static string Localize(this string original)
    {
        if (!original.Contains('['))
            return LanguageManager.Instance.GetValue(original);

        StringBuilder builder = new StringBuilder(original);
        int openIndex = -1;
        for (int i = 0; i < builder.Length; i++)
        {
            if (openIndex == -1 && builder[i] == '[')
            {
                openIndex = i;
                continue;
            }
            
            if (openIndex != -1 && builder[i] == ']')
            {
                string key = original.Substring(openIndex, i - openIndex + 1);
                builder.Remove(openIndex, key.Length);
                builder.Insert(openIndex, LanguageManager.Instance.GetValue(key.Substring(1, key.Length - 2)));
                openIndex = -1;
            }
        }

        return builder.ToString();
    }

    public static void SetLocalizedText(this TextMeshProUGUI tmp, string text)
    {
        tmp.text = text.Localize();

        TMP_FontAsset font;
        if (tmp.font != (font = LanguageManager.Instance.CurrentLanguage.Value.Font))
            tmp.font = font;
    }

    public static TimeSpan sec(this int value)
    {
        return TimeSpan.FromSeconds(value);
    }
    
    public static TimeSpan sec(this float value)
    {
        return TimeSpan.FromSeconds(value);
    }
    
    public static TimeSpan sec(this double value)
    {
        return TimeSpan.FromSeconds(value);
    }
    
    public static string NameOfCallingClass()
    {
        string fullName;
        Type declaringType;
        int skipFrames = 2;
        
        do
        {
            MethodBase method = new StackFrame(skipFrames, false).GetMethod();
            declaringType = method.DeclaringType;
            if (declaringType == null)
                return method.Name;

            skipFrames++;
            fullName = declaringType.FullName;
        }
        while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

        return fullName;
    }
}
