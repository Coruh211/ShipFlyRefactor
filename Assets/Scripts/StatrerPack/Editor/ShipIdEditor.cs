using System;
using System.Linq;
using StaticData;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StatrerPack.Editor
{
    [CustomEditor(typeof(ShipSO))]
    public class ShipIdEditor: UnityEditor.Editor
    {
        private void OnEnable()
        {
            ShipSO shipSO = (ShipSO)target;


            if (string.IsNullOrEmpty(shipSO.Id))
                GenerateID(shipSO);
            else
            {
                ShipSO[] ships = Resources.LoadAll<ShipSO>("Data/Ships");

                if (ships.Any(other => other != shipSO && other.Id == shipSO.Id))
                {
                    GenerateID(shipSO);
                }
            }
        }

        private void GenerateID(ShipSO task)
        {
            task.Id = $"{Guid.NewGuid()}";

            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(task);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
    }
}