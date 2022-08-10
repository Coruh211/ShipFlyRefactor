using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Preview = RuntimePreviewGenerator;
using System;
using Object = UnityEngine.Object;

namespace MatEditor.Editors
{
    public class MaterialDataEditor : EditorWindow
    {
        private static MaterialDataEditor _selectedWindow;
        public static MaterialDataEditor Window => _selectedWindow;
        [MenuItem("Editors/MatEditor")]
        public static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<MaterialDataEditor>("MatEditor v0.5");
            _selectedWindow = window;
            _selectedWindow.Init();
        }

        private void Init()
        {
            selectedIndex = MaterialSelector.SelectedIndex;


            boxStyle = new GUIStyle();
            boxStyle.padding.right = 10;

            selectHeadStyle = new GUIStyle();
            selectHeadStyle.normal.textColor = new Color32(128, 255, 128, 255);

            mainDb = MaterialSelector.Self.Data;
        }

        GUIStyle boxStyle;
        GUIStyle selectHeadStyle;

        MaterialInstallerData mainDb;
        int selectedIndex;

        Vector2 scrollArea = default;
        private bool createNewScheme = false;
        string newSchemeLable = string.Empty;

        private bool instanceDBIsDirty = true;
        private void OnGUI()
        {
            if(mainDb == null)
            {
                GUILayout.Label("Select database");
                mainDb = EditorGUILayout.ObjectField("Materials Database", mainDb, typeof(MaterialInstallerData), false) as MaterialInstallerData;
                return;
            }
            //header

            if (instanceDBIsDirty)
            {
                if (GUILayout.Button("Save"))
                {
                    EditorUtility.SetDirty(mainDb);
                    instanceDBIsDirty = false;
                }
            }
            else
            {
                EditorGUILayout.LabelField("Saved", selectHeadStyle);
            }

            if (!createNewScheme)
            {
                EditorGUILayout.LabelField($"Scheme: {selectedIndex}");

                var headerRect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));
                if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                {
                    if (selectedIndex != 0)
                    {
                        mainDb.ClearIndex(selectedIndex);
                        selectedIndex = 0;
                        instanceDBIsDirty = true;
                    }
                }

                foreach (var indx in mainDb.EnumerateIndexes())
                {
                    if (indx.index == selectedIndex)
                    {
                        if (GUILayout.Button(indx.name, selectHeadStyle, GUILayout.ExpandWidth(false)))
                        {
                            selectedIndex = indx.index;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(indx.name, GUILayout.ExpandWidth(false)))
                        {
                            selectedIndex = indx.index;
                        }
                    }
                }
                if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                {
                    createNewScheme = true;
                    return;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                newSchemeLable = EditorGUILayout.TextField(newSchemeLable);


                EditorGUILayout.BeginHorizontal();
                if (!string.IsNullOrWhiteSpace(newSchemeLable))
                {
                    if (GUILayout.Button("Submit"))
                    {
                        EditorGUILayout.EndHorizontal();
                        mainDb.WriteIndex(newSchemeLable);
                        instanceDBIsDirty = true;
                        createNewScheme = false;
                        return;
                    }
                }
                if(GUILayout.Button("Cancle"))
                {
                    EditorGUILayout.EndHorizontal();
                    createNewScheme = false;
                    return;
                }
                EditorGUILayout.EndHorizontal();
                return;
            }

            //main
            scrollArea = GUILayout.BeginScrollView(scrollArea, false, false, GUILayout.ExpandWidth(false));
            bool requestExit = false;
            foreach (var dt in mainDb)
            {
                if (MaterialInstallerData.PropertyField.IsNullOrEmpty(dt)) continue;

                EditorGUILayout.Space(10, false);
                RenderBlock(dt.Lable, dt.ReadArray(selectedIndex), mainDb.ReadPropertyIndex(dt));

                if (requestExit)
                    break;
            }

            GUILayout.EndScrollView();

            void RenderBlock(string objectLable, MaterialInstallerData.PropertyField.MaterialsArray customMats, int cashIndex)
            {
                var splited = objectLable.Split('/');

                string shortWrite = null;
                if (splited.Length > 4)
                    shortWrite = $"{splited[0]}/ ... [{splited.Length - 2}] ... /{splited[splited.Length - 1]} [{cashIndex}]";
                else
                    shortWrite = objectLable + $" [{cashIndex}]";

                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

                EditorGUILayout.LabelField(shortWrite);

                if (GUILayout.Button("Delete"))
                {
                    if(mainDb.RemoveProperty(cashIndex))
                    {
                        Debug.Log($"Property {shortWrite} was deleted");
                        requestExit = true;
                        instanceDBIsDirty = true;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

                //GUILayout.BeginArea(GUILayoutUtility.GetRect(256 * (customMats.Size + 1), 64));

                int index = 0;
                foreach (var mat in customMats)
                {
                    bool throwError = false;
                    bool throwWarning = false;
                    if (mat != null)
                    {
                        if (mat.name.Contains("(Instance)"))
                        {
                            throwError = true;
                            //Debug.LogError($"Material {mat.name} is marked as instance scene material. Change it material on another material from Assets folder");
                        }
                    }
                    else
                    {
                        throwWarning = true;
                    }

                    var rect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
                    //var rect = GUILayoutUtility.GetRect(128, 128 + 20);
                    //Debug.Log(rect);
                    //GUILayout.BeginArea(rect);

                    EditorGUI.DrawRect(rect, new Color32(32, 32, 32, 255));

                    rect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                    EditorGUI.DrawRect(rect, new Color32(96, 96, 96, 255));

                    var changedMat = EditorGUI.ObjectField(GUILayoutUtility.GetRect(128, 20), mat, typeof(Material), false) as Material;
                    //var changedMat = EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false)) as Material;
                    EditorGUILayout.LabelField($"Material {++index}", GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();
                    if (throwError)
                        EditorGUILayout.HelpBox($"Material {mat.name} is marked as instance scene material. Change it material on another material from Assets folder", MessageType.Error, true);
                    if (throwWarning)
                        EditorGUILayout.HelpBox("Material is not assigned", MessageType.Warning, true);

                    if (changedMat!=mat)
                    {
                        customMats.Replace(mat, changedMat);
                        instanceDBIsDirty = true;
                    }
                    RenderMat(changedMat);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(10, false);
                }

                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("Add"))
                {
                    customMats.ChangeArraySize(customMats.Size + 1);
                    instanceDBIsDirty = true;
                }
                else if (GUILayout.Button("Remove"))
                {
                    customMats.ChangeArraySize(customMats.Size - 1);
                    instanceDBIsDirty = true;
                }


                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                void RenderMat(Material mat)
                {
                    //if (mat == null) return;
                    //objEditor.target = mat;
                    //objEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(100,100), style);
                    var texture = ReadTexture(mat);
                    EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(128, 128), texture , null, ScaleMode.ScaleToFit, 1);
                }
            }
        }

        private Dictionary<Material, TextureData> textureCache = new Dictionary<Material, TextureData>();
        private Texture2D nullTexture;

        private Texture2D ReadTexture(Material mat)
        {
            if (mat == null)
            {
                if(nullTexture == null)
                    nullTexture = Preview.GenerateMaterialPreview(mat, PrimitiveType.Sphere, 256, 256);

                return nullTexture;
            }

            if(textureCache.TryGetValue(mat, out var texData))
            {
                return texData.PassTexture(mat);
            }
            else
            {
                texData = new TextureData(mat);
                textureCache.Add(mat, texData);
                return texData.Texture;
            }
        }

        private sealed class TextureData
        {
            private int crc;
            public int CRC => crc;
            private Texture2D texture;
            public Texture2D Texture => texture;

            public Texture2D PassTexture(Material incomeMat)
            {
                var newCrc = incomeMat.ComputeCRC();

                REPEAT:
                if(crc != newCrc)
                {
                    if (texture != null)
                        DestroyImmediate(texture);

                    texture = Generate(incomeMat);
                    crc = newCrc;
                }

                if(texture == null)
                {
                    crc = 0;
                    goto REPEAT;
                }

                return texture;
            }

            public TextureData(Material mat)
            {
                if (mat == null) throw new ArgumentNullException("Materaial can't be null");

                crc = mat.ComputeCRC();
                texture = Generate(mat);
            }

            private Texture2D Generate(Material mat) => Preview.GenerateMaterialPreview(mat, PrimitiveType.Sphere, 256, 256);
        }
    }
}
