using System;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "DataInstaller", menuName = "Installers/Data Installer")]
public class DataInstaller : ScriptableObject
{
    public ScreenOrientation ScreenOrientation = ScreenOrientation.Portrait;
    public bool DebugMode;
    public bool EnableAds;

    public string[] Nicknames = 
    {
        "KutilinSA", "Veta", "MatriX", "R1nGoo", "Corwin", "Minebot", "Kenzi", "CyrilZ",
        "Tema", "Rider_Blade", "gexly", "Temo4kin", "adguard", "Yanic", "Gidel",
        "Inahidi", "Ssosa", "Gomer", "Edan", "Ulonasta", "Jeffer", "Rbyaakov",
        "WhshNik", "Ulua", "Dysius", "Skiff79", "Rionair", "Xandread", "Ssergus",
        "Onaneva", "Najaxo", "Leynara", "Minivel", "Inetan", "Perenec", "Quinti",
        "Patiera", "Everice", "Ober", "Xonah", "Ipporan", "Nitan", "Joker000", "Jesi",
        "OXILORE", "Stic", "Janasna", "Elia", "Noritt", "Llame", "Karikor", "Caesaray",
        "Isiss", "Hoan", "Belemu", "Vendarr", "Jesusa", "Xanth", "Jensouth", "Saee",
        "Winta", "Cherriel", "Garf", "Branzaaa", "Cytheodo", "Bster", "Axim",
        "Uilleri", "Gianal", "Naricka", "Rahma", "Hate", "Cony", "Cort", "Zloann",
        "Heala", "Gangie", "Katiser", "Gaza", "Keili", "Onal", "Yvon", "Felin",
        "Ueloisel", "Drickoaa", "Ssann", "Kortoniq", "Beckenn", "Wardonor", "Quel",
        "Zahina", "Dora", "Briel", "Qwar", "adguardPaladin", "DJ Daks NN", "shelpi",
        "Chris Blade", "simonov-89", "Antonio_Bianco", "NIKALATA", "kizune",
        "so#unpredictable", "mamalama", "Arti", "Amrun", "iwannabesonic"
    };

    [Header("Control Settings")]
    public bool ShowJoystick;
    public ControlType ControlType;
}

public enum ControlType
{
    None,
    DynamickJoystick,
    StaticJoystick,
    FloatingJoystick,
}
