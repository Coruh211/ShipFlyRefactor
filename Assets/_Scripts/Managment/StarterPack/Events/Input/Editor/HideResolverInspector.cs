using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Input
{
    [CustomEditor(typeof(InputAttributesResolver))]
    public class HideResolverInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            (target as InputAttributesResolver).hideFlags = HideFlags.HideInInspector;
        }
    }

}