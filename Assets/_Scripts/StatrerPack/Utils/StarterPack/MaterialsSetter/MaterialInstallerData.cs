
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MatEditor
{
    [CreateAssetMenu(fileName = "New mat data", menuName = "Installers/Material")]
    public class MaterialInstallerData : ScriptableObject, IEnumerable<MaterialInstallerData.PropertyField>
    {
        #region iterator
        IEnumerator<PropertyField> IEnumerable<PropertyField>.GetEnumerator()
        {
            foreach (var itm in allProps)
                yield return itm;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<PropertyField>).GetEnumerator();
        }

        #endregion
        [SerializeField] List<IndexName> listName = new List<IndexName>() { new IndexName(0, "Default") };
        [HideInInspector, SerializeField] List<PropertyField> allProps = new List<PropertyField>(); //avoid random npe
        public PropertyField Read(string lable)
        {
            foreach (var pr in allProps)
            {
                if (PropertyField.IsNullOrEmpty(pr)) continue;

                if (pr.Lable.Equals(lable))
                    return pr;
            }

            return null;
        }
        public PropertyField Read(int index)
        {
            try
            {
              
                var pr = allProps[index];
                if (PropertyField.IsNullOrEmpty(pr))
                    return null;
                else return pr;

            }
            catch(IndexOutOfRangeException)
            {
                return null;
            }
        }

        public int ReadPropertyIndex(in PropertyField field)
        {
            if (PropertyField.IsNullOrEmpty(field)) return -1;
            return allProps.IndexOf(field);
        }

        public bool RemoveProperty(int index)
        {
            try
            {
                if (!PropertyField.IsNullOrEmpty(allProps[index]))
                {
                    allProps[index] = null;
                    return true;
                }
                else return false;
            }
            catch(IndexOutOfRangeException)
            {
                return false;
            }
        }

        public int Write(in MaterialSetter setter, Material[] mats)
        {
            var outField = Read(setter.Lable);
            if (outField != null)
            {
                outField.WriteArray(mats, MaterialSelector.SelectedIndex);
                return allProps.IndexOf(outField);
            }

            outField = new PropertyField(setter.Lable, MaterialSelector.SelectedIndex, mats);
            //allProps.Add(outField);
            //return allProps.IndexOf(outField);

            return InsertNewPropField(outField);
        }

        private int InsertNewPropField(PropertyField field)
        {
            int selIndex = -1;
            for(int i =0;i<allProps.Count;i++)
            {
                if(PropertyField.IsNullOrEmpty(allProps[i]))
                {
                    selIndex = i;
                    break;
                }
            }

            if(selIndex>=0)
            {
                allProps[selIndex] = field;
                return selIndex;
            }
            else
            {
                allProps.Add(field);
                return allProps.IndexOf(field);
            }
        }

        //Индексы для визуального восприятия
        #region indexes
        public string ReadIndex(int index)
        {
            if (index < listName.Count && index >= 0)
            {
                var name = listName[index].name;
                if (string.IsNullOrWhiteSpace(name))
                    return "[null]";
                else return listName[index].name;
            }
            else return "Unknow pattern";
        }
        public int SupportPatternsCount => listName.Count;
        public bool SupportPatternIndex(int index)
        {
            return index < listName.Count && index >= 0;
        }
        public void WriteIndex(int index, string name)
        {
            var find = listName.Find((x) =>
            {
                return x.index == index;
            });

            if (find != null)
            {
                find.name = name;
            }
            else
            {
                listName.Add(new IndexName(index, name));

                CheckProps();
            }
        }
        public void WriteIndex(string name)
        {
            var find = listName.Find((x) =>
            {
                return x.name.Equals(name);
            });
            if (find != null) return;

            listName.Add(new IndexName(listName.Count, name));
            CheckProps();
        }

        private void CheckProps()
        {
            foreach (var prop in allProps)
            {
                if (PropertyField.IsNullOrEmpty(prop)) continue;

                if (prop.SupportSchemes < listName.Count)
                {
                    prop.UpdateWithMax(listName.Count);
                }
            }
        }

        public bool ClearIndex(int index)
        {
            var find = listName.Find((x) =>
            {
                return x.index == index;
            });

            if (find != null)
            {
                return listName.Remove(find);
            }
            else return false;
        }
        public IEnumerable<IndexName> EnumerateIndexes()
        {
            foreach (var ind in listName)
                yield return ind;
        }
        #endregion

        [System.Serializable]
        public class PropertyField : IEnumerable<MaterialInstallerData.PropertyField.MaterialsArray>
        {
            [SerializeField, Tooltip("Название объекта с типом компонента. Обычно не нужно его редактировать напрямую")] private string _objectFullName; //example myCustomRenderer:MeshRenderer
            public string Lable => _objectFullName;
            [SerializeField, Tooltip("Схемы материалов, которые будут применены к рендереру")] private MaterialsArray[] _materialsScheme;
            public int SupportSchemes => _materialsScheme.Length;
            public void AssignedAllMaterialsTo<T>(T tgComponent) where T : Renderer
            {
                try
                {
                    tgComponent.sharedMaterials = _materialsScheme[MaterialSelector.SelectedIndex]._assignedMaterials;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            public static bool IsNullOrEmpty(PropertyField field)
            {
                if (field is null) return true;
                return string.IsNullOrEmpty(field.Lable);
            }

            public MaterialsArray ReadArray(int customIndex)
            {
                if (customIndex >= _materialsScheme.Length)
                    UpdateWithMax(customIndex+1);

                return _materialsScheme[customIndex];
            }

            public void UpdateWithMax(int max)
            {
                if (max <= SupportSchemes) return;

                var tmp = _materialsScheme;
                _materialsScheme = new MaterialsArray[max];
                for (int i = 0; i < tmp.Length; i++)
                {
                    _materialsScheme[i] = tmp[i];
                }
                for (int i = tmp.Length; i < max; i++)
                {
                    _materialsScheme[i] = new MaterialsArray();
                }
            }
            public void WriteArray(Material[] array, int index)
            {
                if (index >= SupportSchemes)
                    UpdateWithMax(index + 1);


                var newAr = new Material[array.Length];
                _materialsScheme[index]._assignedMaterials = newAr;
                for (int i = 0; i < newAr.Length; i++)
                    newAr[i] = array[i];

            }
            public IEnumerator<MaterialsArray> GetEnumerator()
            {
                return ((IEnumerable<MaterialsArray>)_materialsScheme).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _materialsScheme.GetEnumerator();
            }

            [System.Serializable]
            public class MaterialsArray : IEnumerable<Material>
            {
                [InspectorName("Materials list"), Tooltip("Материалы, которые будут назначены определенному рендеру")]
                public Material[] _assignedMaterials = new Material[0];
                public int Size => _assignedMaterials.Length;
                public void Replace(Material from, Material to)
                {
                    for (int i = 0; i < _assignedMaterials.Length; i++)
                    {
                        if (_assignedMaterials[i] == from)
                        {
                            _assignedMaterials[i] = to;
                            return;
                        }
                    }
                }
                public void Replace(int index, Material to)
                {
                    _assignedMaterials[index] = to;
                }

                public void ChangeArraySize(int newSize)
                {
                    var newArray = new Material[newSize];
                    var curSize = newSize;
                    if (curSize > Size)
                        curSize = Size;
                    for (int i = 0; i < curSize; i++)
                    {
                        newArray[i] = _assignedMaterials[i];
                    }
                    _assignedMaterials = newArray;
                }

                public IEnumerator<Material> GetEnumerator()
                {
                    return ((IEnumerable<Material>)_assignedMaterials).GetEnumerator();
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return _assignedMaterials.GetEnumerator();
                }


            }

            public PropertyField(string lable, int initialIndex, Material[] loadMaterials)
            {
                this._objectFullName = lable;

                _materialsScheme = new MaterialsArray[initialIndex + 1];
                _materialsScheme[initialIndex] = new MaterialsArray() { _assignedMaterials = new Material[loadMaterials.Length] };
                var newAr = _materialsScheme[initialIndex]._assignedMaterials;

                for (int i = 0; i < newAr.Length; i++)
                    newAr[i] = loadMaterials[i];

                for (int i = 0; i < initialIndex; i++)
                    _materialsScheme[i] = new MaterialsArray();
            }
        }
        [System.Serializable]
        public class IndexName
        {
            public int index;
            public string name;

            public IndexName(int index, string name)
            {
                this.index = index;
                this.name = name;
            }
        }
    }
}