using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace MatEditor
{
    public enum InitializeMode
    {
        Awake,
        Start,
        Script,
    }

    [RequireComponent(typeof(Renderer))]
    public class MaterialSetter : MonoBehaviour
    {
        [SerializeField, Tooltip("DO NOT CHANGE IT MANNUALY")] private int index = -1;
        [SerializeField, Tooltip("Когда произойдет инициализация")] private InitializeMode _InitializeMode;
        [SerializeField, HideInInspector] Renderer[] allRenderers;
        [SerializeField, HideInInspector] private string lable;
        public string Lable => lable;
        /// <summary>
        /// Загружает материалы на рендерер
        /// </summary>
        public void ApplyAllMaterials()
        {
            if (index < 0)
            {
                Debug.LogError("Invalid material index. Use ToHash previosly for registration item", this);
                return;
            }

            var mat = MaterialSelector.Self.Read(index);
            if(mat is null)
            {
                Debug.LogError($"Unknow index [{index}]. Maybe you delete this item in MatEditor? Please, reuse ToCash or reassign valid index", this);
                return;
            }
            foreach (var render in allRenderers)
            {
                mat.AssignedAllMaterialsTo(render);
            }
        }

        /// <summary>
        /// Используется эдитором
        /// </summary>
        public void ToHash() //Used by Editor
        {
            allRenderers = GetComponents<Renderer>();
            lable = $"{gameObject.name}::{allRenderers[0].GetType().Name}";
            Transform parent = transform.parent;
            while (parent != null)
            {
                lable = $"{parent.name}/{lable}";
                parent = parent.parent;
            }
            MaterialSelector.AssignNewHash(this, out index, allRenderers[0].sharedMaterials);

            Debug.Log($"New Setter was inited with parametrs: {lable}\nIndex: {index}", this);
        }

        private void Reset()
        {
            if (index is -1)
            {
                ToHash();
            }
        }
        private void Awake()
        {
            if (_InitializeMode == InitializeMode.Awake)
                ApplyAllMaterials();
        }

        private void Start()
        {
            if (_InitializeMode == InitializeMode.Start)
                ApplyAllMaterials();
        }
    }
}
