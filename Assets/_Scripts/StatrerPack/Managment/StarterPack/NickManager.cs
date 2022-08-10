using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class NickManager : Singleton<NickManager>
{
    [SerializeField] private DataInstaller dataInstaller;
    
    private List<string> freeNicks;

    protected override void AwakeSingletone()
    {
        freeNicks = dataInstaller.Nicknames.ToList();
    }

    public string PullRandomNick()
    {
        (int index, string nick) pair = freeNicks.GetRandom();
        freeNicks.RemoveAt(pair.index);
        return pair.nick;
    }

    public string GetRandomFlag()
    {
        return $"<sprite={Random.Range(0, 20)}>";
    }
}