using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class EmojiSetter : MonoBehaviour
{
    [SerializeField] private bool isPositive;

    private void Start()
    {
        Image image = GetComponent<Image>();
        image.sprite = isPositive
            ? GameManager.Instance.artInstaller.PositiveEmoji.GetRandomValue()
            : GameManager.Instance.artInstaller.NegativeEmoji.GetRandomValue();
    }
}