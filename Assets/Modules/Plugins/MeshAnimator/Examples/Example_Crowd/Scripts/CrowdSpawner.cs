using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FSG.MeshAnimator
{
    public class CrowdSpawner : MonoBehaviour
    {
        private static List<CrowdSpawner> spawners = new List<CrowdSpawner>();

        public GameObject[] options;
        public string[] optionsDesc;
        public string[] meshAnimationNames;
        public string[] otherInfo = new string[0];
        public int sizeOfCrowd = 1000;
        public int selectedOption = 0;
        public int maxSize = 5000;
        public float radius = 100;
        public float slopeStart = 0;
        public float slopeAmount = 1;
        public Vector2 radiusScaler = Vector2.one;
        public Vector3 baseScale = Vector3.one;
        public bool randomizeTime = false;
        public bool showGUI = true;

        private string fps;
        private int previousFrame = 0;
        private int previousSelection = 0;
        private List<GameObject> spawnedObjects = new List<GameObject>();
        private int guiOffset = 0;

        void Start()
        {
            SpawnCrowd();
            InvokeRepeating("UpdateFPS", 0.0001f, 1f);
            guiOffset = spawners.Count;
            spawners.Add(this);
        }
        private void OnDestroy()
        {
            spawners.Remove(this);
        }
        void UpdateFPS()
        {
            fps = ((Time.frameCount - previousFrame) / 1f).ToString("00.00");
            previousFrame = Time.frameCount;
        }
        void SpawnCrowd()
        {
            int startIndex = 0;
            if (previousSelection == selectedOption)
            {
                startIndex = spawnedObjects.Count;
                int toRemove = spawnedObjects.Count - sizeOfCrowd;
                if (toRemove > 0)
                {
                    for (int i = 0; i < toRemove; i++)
                    {
                        if (spawnedObjects[i]) Destroy(spawnedObjects[i]);
                    }
                    spawnedObjects.RemoveRange(0, toRemove);
                }
            }
            else
            {
                foreach (var obj in spawnedObjects)
                    if (obj) Destroy(obj);
                spawnedObjects.Clear();
            }
            previousSelection = selectedOption;

            Vector3 center = transform.position;
            for (int i = startIndex; i < sizeOfCrowd; i++)
            {
                Vector3 rand = Random.onUnitSphere * radius;
                Vector3 position = center + new Vector3(rand.x * radiusScaler.x, 0, rand.z * radiusScaler.y);
                float disFromCenter = Vector3.Distance(center, position);
                if (disFromCenter < slopeStart)
                {
                    position.y = center.y;
                }
                else
                {
                    position.y = (disFromCenter - slopeStart) / (radius - slopeStart) * slopeAmount;
                }
                RaycastHit hit;
                if (Physics.Raycast(position + Vector3.up * 10, Vector3.down, out hit, 50, -1))
                {
                    position.y = hit.point.y;
                }
                var g = Instantiate(options[selectedOption], transform);
                g.transform.position = position;// + new Vector3(Random.value, 0, Random.value) * 2;
                g.transform.LookAt(new Vector3(0, position.y, 0), Vector3.up);
                g.transform.localScale = baseScale + Vector3.one * Random.value * 0.25f;
                if (g.GetComponent<Animator>())
                {
                    g.GetComponent<Animator>().speed = Random.Range(0.9f, 1.1f);
                    g.GetComponent<Animator>().SetInteger("Anim", Random.Range(0, 4));
                }
                else if (g.GetComponent<MeshAnimatorBase>())
                {
                    MeshAnimatorBase ma = g.GetComponent<MeshAnimatorBase>();
                    if (meshAnimationNames.Length > 0)
                    {
                        string name = meshAnimationNames[Random.Range(0, meshAnimationNames.Length)];
                        for (int a = 0; a < ma.animations.Length; a++)
                        {
                            if (ma.animations[a].AnimationName == name)
                            {
                                ma.defaultAnimation = ma.animations[a];
                                ma.Play(a);
                                break;
                            }
                        }
                        ma.speed = Random.Range(0.9f, 1.1f);
                    }
                    ma.SetTimeNormalized(Random.value, true);
                }
                spawnedObjects.Add(g);
            }
        }
        void OnGUI()
        {
            if (!showGUI)
                return;
            GUI.skin.label.richText = true;
            GUILayout.BeginArea(new Rect(Screen.width * 0.025f, Screen.height * 0.025f + (guiOffset * Mathf.Max(150f, Screen.height * 0.15f)) + (10 * guiOffset), Screen.width * 0.3f, Mathf.Max(150f, Screen.height * 0.15f)), GUI.skin.box);
            {
                GUI.color = Color.white;
                if (optionsDesc.Length > 1)
                {
                    GUI.color = selectedOption == 0 ? Color.green : Color.white;
                    for (int i = 0; i < optionsDesc.Length; i++)
                    {
                        GUI.color = selectedOption == i ? Color.green : Color.white;
                        if (GUILayout.Button(optionsDesc[i]))
                        {
                            previousSelection = selectedOption;
                            selectedOption = i;
                            SpawnCrowd();
                        }
                    }
                    GUI.color = Color.white;
                }
                else
                {
                    GUILayout.Label("<color=white><size=19><b>" + optionsDesc[0] + "</b></size></color>");
                }
                int size = sizeOfCrowd;
                GUILayout.Label("<color=white><size=19><b>Crowd Size: " + sizeOfCrowd + "</b></size></color>");
                sizeOfCrowd = (int)GUILayout.HorizontalSlider(sizeOfCrowd, 1, maxSize);
                if (size != sizeOfCrowd)
                {
                    CancelInvoke("SpawnCrowd");
                    Invoke("SpawnCrowd", 1);
                }
                else
                {
                    GUILayout.Label("<color=white><size=19><b>FPS: " + fps + "</b></size></color>");
                }
                for (int i = 0; i < otherInfo.Length; i++)
                {
                    GUILayout.Label("<color=white><size=19><b>" + otherInfo[i] + "</b></size></color>");
                }
            }
            GUILayout.EndArea();
        }
    }
}