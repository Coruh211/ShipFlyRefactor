using System;
using TMPro;
using UnityEngine;

public class NickSetter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nickText;

    private string nick;
    private string flag;
    
    public string Nick
    {
        get => nick;
        set
        {
            nick = value;
            nickText.text = flag + nick;
        }
    }

    public string Flag
    {
        get => flag;
        set
        {
            flag = value;
            nickText.text = flag + nick;
        }
    }

    private void Start()
    {
        flag = NickManager.Instance.GetRandomFlag();
        Nick = NickManager.Instance.PullRandomNick();
    }
}