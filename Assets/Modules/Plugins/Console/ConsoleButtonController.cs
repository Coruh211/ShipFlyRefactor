using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleButtonController : MonoBehaviour
{
    [SerializeField] private Console _console;

	private void Start()
	{
		if (GameManager.Instance.dataInstaller.DebugMode)
		{
			_console.gameObject.SetActive(true);
			_console.enabled = true;
		}
	}
}
