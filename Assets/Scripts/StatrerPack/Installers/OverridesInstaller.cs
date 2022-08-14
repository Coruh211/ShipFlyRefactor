
using UnityEngine;

[CreateAssetMenu(fileName = "OverridesInstaller", menuName = "Installers/Overrides Installer")]
public class OverridesInstaller : ScriptableObject
{
    public bool Enable = true;
    public int Coins = -1;
    public int Improvement0Level = -1;
    public int Improvement1Level = -1;
    public string LanguageCode = "system";
}