using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LanguageManager : Singleton<LanguageManager> {
	
	public readonly List<Language> Languages = new List<Language>();
	public readonly ValueObservable<Language> CurrentLanguage = new ValueObservable<Language>(null);

	[SerializeField] private OverridesInstaller overridesInstaller;
	[SerializeField] private ArtInstaller artInstaller;

	protected override void AwakeSingletone()
	{
		TextAsset[] langAssets = Resources.LoadAll<TextAsset>("Lang");
		foreach (TextAsset asset in langAssets)
			RegisterLangFile(asset);

		if (overridesInstaller.LanguageCode.Equals("system"))
		{
			if (!SetLanguage(lang => lang.System == Application.systemLanguage))
				SetDefaultLanguage();
		}
		else if (!SetLanguage(lang => lang.Code.Equals(overridesInstaller.LanguageCode)))
			SetDefaultLanguage();
	}

	private void RegisterLangFile(TextAsset asset)
	{
		List<string> lines = Encoding.UTF8.GetString(asset.bytes).Split(new [] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries).ToList();
		lines.RemoveAll(x => x[0].Equals('#') || String.IsNullOrWhiteSpace(x));
			
		Dictionary<string, string> toLang = new Dictionary<string, string>();
		for (int i = 1; i < lines.Count; i++) {
			string[] splitted = lines[i].Split(new [] {'='}, StringSplitOptions.RemoveEmptyEntries);
			try {
				toLang.Add(splitted[0], splitted[1]);
			}
			catch (IndexOutOfRangeException e) {
				Debug.Log(e);
			}
		}

		string[] languageInfo = lines[0].Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
		Language existLanguage = Languages.FirstOrDefault(lang => lang.Name.Equals(languageInfo[0]));

		if (existLanguage == null)
			Languages.Add(new Language(
				languageInfo[0],
				languageInfo[1],
				(SystemLanguage) int.Parse(languageInfo[2]),
				artInstaller.LanguageFonts[int.Parse(languageInfo[3])],
				toLang
			));
		else
			existLanguage.Dictionary = existLanguage.Dictionary.Concat(toLang).ToDictionary(pair => pair.Key, pair => pair.Value);
	}
	
	public void SetDefaultLanguage() {
		SetLanguage(x => x.Code.Equals("en"));
	}
	
	public bool SetLanguage(Predicate<Language> predicate) {
		Language toCurrent = Languages.Find(predicate);
		
		if (toCurrent != null)
			SetLanguage(toCurrent);
			
		return toCurrent != null;
	}

	public void SetLanguage(Language language) {
		CurrentLanguage.Value = language;
	}
	
	public string GetValue(string key) {
		try {
			return CurrentLanguage.Value.Dictionary[key];
		}
		catch (KeyNotFoundException e) {
			return key;
		}
	}

	public class Language {
		
		public string Name;
		public string Code;
		public SystemLanguage System;
		public TMP_FontAsset Font;
		public Dictionary<string, string> Dictionary;
		
		public Language(string name, string code, SystemLanguage system, TMP_FontAsset font, Dictionary<string, string> dictionary) {
			Name = name;
			Code = code;
			System = system;
			Font = font;
			Dictionary = dictionary;
		}
	}
}
