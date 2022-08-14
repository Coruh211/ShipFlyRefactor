using System.Linq;
using MoreLinq;
using UnityEngine;

public class BotsStateData : StateData
{
    public static readonly int run = Animator.StringToHash("Run");
    
    [HideInInspector] public Vector3 PositionToMove;
    [HideInInspector] public Transform TargetToMove;
}