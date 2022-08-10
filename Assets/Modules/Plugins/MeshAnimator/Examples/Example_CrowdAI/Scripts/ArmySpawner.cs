using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmySpawner : MonoBehaviour
{
    [System.Serializable]
    public struct Unit
    {
        public GameObject prefab;
        public int count;
        public float delay;
        public bool loop;
    }

    private static Dictionary<int, Stack<GameObject>> _pool = new Dictionary<int, Stack<GameObject>>();

    public Vector2 spawnSize;
    public Vector3 sizeMult = Vector3.one;
    public Unit[] units;

    private IEnumerator Start()
    {
        var trans = transform;
        var center = trans.position;
        for (int i = 0; i < units.Length; i++)
        {
            for (int j = 0; j < units[i].count; j++)
            {
                Vector3 pos = new Vector3(center.x + spawnSize.x * (Random.value - 0.5f), center.y, center.z + spawnSize.y * (Random.value - 0.5f));
                var spawned = LoadFromPool(units[i]);
                spawned.transform.SetParent(transform);
                spawned.transform.position = pos;
                spawned.transform.localScale += sizeMult * 0.5f * Random.value;
                spawned.SetActive(true);
                if (units[i].delay > 0)
                    yield return new WaitForSeconds(Random.Range(0, units[i].delay));
            }
            if (units[i].loop)
                i--;
            yield return null;
        }
    }

    private GameObject LoadFromPool(Unit unit)
    {
        int key = unit.prefab.GetInstanceID();
        if (_pool.ContainsKey(key) && _pool[key].Count > 0)
        {
            return _pool[key].Pop();
        }
        return Instantiate(unit.prefab);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnSize.x, 0, spawnSize.y));
    }
}
