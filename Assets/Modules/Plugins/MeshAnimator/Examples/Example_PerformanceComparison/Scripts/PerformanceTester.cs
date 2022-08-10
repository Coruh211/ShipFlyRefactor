using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FSG.MeshAnimator
{
	public class PerformanceTester : MonoBehaviour
	{
		public GameObject[] animObjects;
		public string[] options;
		public float cameraSpeed = 20;
		public Vector3 spawnOffset = new Vector3(-10, 0, 5);

		private int[] spawnedMeshes = new int[3];
		private List<GameObject> meshes = new List<GameObject>();
		private string fps;
		private int previousFrame = 0;
		private Vector3 offset = new Vector3(-10, 0, 0);

		void Start()
		{
			InvokeRepeating("UpdateFPS", 0.0001f, 1f);
		}
		void UpdateFPS()
		{
			fps = ((Time.frameCount - previousFrame) / 1f).ToString("00.00");
			previousFrame = Time.frameCount;
		}
		void Update()
		{
            var transform = Camera.main.transform;
			if (Input.GetKey(KeyCode.W))
				transform.position += Vector3.forward * Time.deltaTime * cameraSpeed;
			if (Input.GetKey(KeyCode.A))
				transform.position -= Vector3.right * Time.deltaTime * cameraSpeed;
			if (Input.GetKey(KeyCode.S))
				transform.position -= Vector3.forward * Time.deltaTime * cameraSpeed;
			if (Input.GetKey(KeyCode.D))
				transform.position += Vector3.right * Time.deltaTime * cameraSpeed;
		}
		void OnGUI()
		{
			GUI.skin.label.richText = true;
			GUILayout.BeginArea(new Rect(Screen.height * 0.1f, Screen.width * 0.1f, Screen.width * 0.3f, Screen.height));
			{
				GUI.color = Color.white;
				GUILayout.Label("<size=20><b>FPS: " + fps + "</b></size>");
				GUILayout.Label("WASD to move the camera");
				for (int i = 0; i < options.Length; i++)
				{
					if (GUILayout.RepeatButton(options[i] + " Spawned: " + spawnedMeshes[i], GUILayout.Height(Screen.height * 0.05f)))
					{
						meshes.Add((GameObject)GameObject.Instantiate(animObjects[i], offset, Quaternion.Euler(0, 180, 0)));
                        spawnedMeshes[i] += 2;
						offset.x += -spawnOffset.x / 10f;
						if (meshes.Count % 20 == 0)
						{
							offset.x = spawnOffset.x;
							offset.z += spawnOffset.z;
						}
					}
				}
				if (GUILayout.Button("Clear", GUILayout.Height(Screen.height * 0.05f)))
				{
					foreach (var m in meshes)
						GameObject.Destroy(m);
					meshes.Clear();
					spawnedMeshes = new int[3];
					offset = new Vector3(-10, 0, 0);
                }
            }
			GUILayout.EndArea();
		}
	}
}