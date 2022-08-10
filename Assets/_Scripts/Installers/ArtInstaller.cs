using System;
using System.Collections.Generic;
using Imphenzia;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "ArtInstaller", menuName = "Installers/Art Installer")]
public class ArtInstaller : ScriptableObject
{
    public Gradient GradientSky;
    public List<TMP_FontAsset> LanguageFonts = new List<TMP_FontAsset>();
    public List<Sprite> PositiveEmoji;
    public List<Sprite> NegativeEmoji;

    public void OnValidate()
    {
        try
        {
            Camera.main.GetComponent<GradientSkyCamera>().gradient = GradientSky;
        }
        catch (NullReferenceException e){ }
    }
}