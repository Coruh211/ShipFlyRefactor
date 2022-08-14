using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MatEditor
{
    public class MaterialSelector : MonoBehaviour
    {
        private static MaterialSelector _self;
        public static MaterialSelector Self
        {
            get
            {
                if (_self == null)
                    _self = GameObject.FindObjectOfType<MaterialSelector>();
                return _self;
            }
        }

        private void Awake()
        {
            if (_self == null)
            {
                _self = this;
            }
            else if (_self != this)
                Destroy(this);
        }

        /// <summary>
        /// Вызов крайне не рекомендуется в рантайме. Используется редактором
        /// </summary>
        public void Rehash()
        {

            if (allSetters != null)
            {
                List<MaterialSetter> list = new List<MaterialSetter>(allSetters.Count);
                foreach (var setter in allSetters)
                {
                    if (setter != null)
                    {
                        setter.ToHash();
                        list.Add(setter);
                    }
                }

                allSetters.Clear(); //dispose
                allSetters = list;


            }
        }

        /// <summary>
        /// Вызов не рекомендуется в рантайме. Для смены материалов используйте <see cref="ChangeGlobalIndex(int, bool)"/>
        /// </summary>
        public void LoadMaterialsPattern()
        {
            if (allSetters != null)
            {
                if (selectedIndex >= data.SupportPatternsCount)
                {
                    Debug.LogError("Selected index has unregistred pattern. Use the MatEditor to register new pattern");
                    return;
                }

                foreach (var setter in allSetters)
                {
                    setter?.ApplyAllMaterials();
                }
            }
            else
            {
                Debug.Log("Empty register. Add MaterialSetters in Scene");
            }
        }

        [SerializeField, Tooltip("Нужно для покраски художникам")] int selectedIndex = 0;
        public static int SelectedIndex => Self.selectedIndex;
        public static string SelectedPattern => Self.internal_SelectedPattern();
        private string internal_SelectedPattern()
        {
            return data?.ReadIndex(selectedIndex) ?? string.Empty;
        }

        [SerializeField] new bool enabled = true;
        public static bool Enabled => _self.enabled;
        [SerializeField] MaterialInstallerData data;
        public MaterialInstallerData Data => data;

        [SerializeField] private List<MaterialSetter> allSetters;
        public MaterialInstallerData.PropertyField Read(int index)
        {
            return data.Read(index);
        }

        /// <summary>
        /// Ивент, который вызывается после смены индекса через <see cref="ChangeGlobalIndex(int,bool)"/>
        /// </summary>
        public event System.Action<int> OnSelectedIndexChangeHandler;
        public static bool SupportPatternIndex(int index) => Self.data.SupportPatternIndex(index);
        /// <summary>
        /// Меняет выбранный паттерн и обновляет материалы
        /// </summary>
        /// <param name="newValue">Новый индекс. Индекс означает номер цветовой схемы</param>
        /// <param name="asyncChange">Менять материалы асинхронно?</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public static void ChangeGlobalIndex(int newValue, bool asyncChange = false)
        {
            if (!Self.data.SupportPatternIndex(newValue))
                throw new System.ArgumentOutOfRangeException("Unsupported pattern index");

            Self.selectedIndex = newValue;

            if (!asyncChange)
            {
                foreach (var st in _self.allSetters)
                {
                    if(st!=null)
                    st.ApplyAllMaterials();
                }
            }
            else
            {
                _self.internal_StartChangeMaterialsAsync();
            }

            _self.OnSelectedIndexChangeHandler?.Invoke(newValue);
        }

        Coroutine asyncChangeCoroutine = null;
        private void internal_StartChangeMaterialsAsync()
        {
            if (asyncChangeCoroutine != null)
                StopCoroutine(asyncChangeCoroutine);
            asyncChangeCoroutine = StartCoroutine(AsyncChange());
            IEnumerator AsyncChange()
            {
                foreach (var st in _self.allSetters)
                {
                    if(st!=null)
                    st.ApplyAllMaterials();
                    yield return null;
                }
            }
        }

        public static void AssignNewHash(in MaterialSetter setter, out int index, in Material[] mat)
        {
            Self.internal_AssignNewHash(setter, out index, mat);
        }

        private void internal_AssignNewHash(in MaterialSetter setter, out int index, in Material[] mat)
        {
            if (!allSetters.Contains(setter))
                allSetters.Add(setter);

            index = data.Write(setter, mat);

#if UNITY_EDITOR
            SaveData();
#endif


        }


#if UNITY_EDITOR
        private void SaveData()
        {
            EditorUtility.SetDirty(data);
        }
#endif
    }
}
