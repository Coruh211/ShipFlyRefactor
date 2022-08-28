using UnityEngine;

namespace StaticData
{
    [CreateAssetMenu(menuName = "Create ShipSO", fileName = "ShipSO")]
    public class ShipSO: ScriptableObject
    {
        public int Price;
        public int Lives;
        public GameObject ShipPrefab;
        public string Id;
    }
}