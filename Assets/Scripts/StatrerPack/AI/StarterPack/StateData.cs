using System.Linq;
using MoreLinq;
using UnityEngine;
using UnityEngine.AI;

public class StateData : MonoBehaviour
{
    public string CurrentState;
    public string PrevState;
    public GameObject Main;
    public NavMeshAgent Agent;
    public Animator Animator;
}