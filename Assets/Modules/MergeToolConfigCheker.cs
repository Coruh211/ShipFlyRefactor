#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MergeToolConfigCheker
{
	[Header("Раскройте свойства и используйте внизу метод \n \"WriteMergeToolConfig\"")]
	[SerializeField] private bool empty;
	private const string PATH_CONFIG = ".git/Config";
	private string[] _findWords = { "[mergetool", "\r\n[mergetool", "\n[mergetool" };

	private static string[] _writeConfig = {
		"\r\n[merge]",
		"\r\ntool = unityyamlmerge",
		"\r\n[mergetool \"unityyamlmerge\"]",
		"\r\ntrustExitCode = false",
		"\r\nkeepTemporaries = true",
		"\r\nkeepBackup = false",
		"\r\npath = 'C:\\\\Program Files\\\\Unity\\\\Hub\\\\Editor\\\\2020.3.5f1\\\\Editor\\\\Data\\\\Tools\\\\UnityYAMLMerge.exe'",
		"\r\ncmd = 'C:\\\\Program Files\\\\Unity\\\\Hub\\\\Editor\\\\2020.3.5f1\\\\Editor\\\\Data\\\\Tools\\\\UnityYAMLMerge.exe' merge -p $BASE $REMOTE $LOCAL $MERGED"
	};

	[InitializeOnLoadMethod]
	private static void OpenFile()
	{
		var linkClass = new MergeToolConfigCheker();

		var file = File.ReadAllText(PATH_CONFIG);
		if (!CheckConfigText(file, linkClass._findWords))
		{
			Debug.Log("Config записан");
			WriteConfig();
		}
		else
			Debug.Log("Config уже был записан ранее");
	}

	private static bool CheckConfigText(string file, string[] array)
	{
		var splitText = file.Split(new char[] { ' ', '.', ',', ':', '\t', '\r', '\n' }).ToList();

		foreach (var text in splitText)
		{
			foreach (var word in array)
				if (text == word)
					return true;
		}
		return false;

	}

	private static void WriteConfig()
	{
		FileStream stream = new FileStream(PATH_CONFIG, FileMode.Append);
		for (int i = 0; i < _writeConfig.Length; i++)
		{
			byte[] bytes = System.Text.Encoding.Default.GetBytes(_writeConfig[i]);
			stream.Write(bytes, 0, bytes.Length);
		}

		stream.Close();
	}
}
#endif