using System.Collections.Generic;
using UnityEngine;

public class AI_Manager : MonoBehaviour
{
    private static AI_Manager _instance;
    private static List<AI_Unit> _units = new List<AI_Unit>(50000);

    public static void Add(AI_Unit unit)
    {
        if (_instance == null)
            _instance = new GameObject("AI_Manager").AddComponent<AI_Manager>();
        _units.Add(unit);
    }
    public static void Remove(AI_Unit unit)
    {
        _units.Remove(unit);
    }
    private void Update()
    {
        float t = Time.time;
        float d = Time.deltaTime;
        for (int i = 0; i < _units.Count; i++)
        {
            _units[i].Tick(t, d);
        }
    }
}
