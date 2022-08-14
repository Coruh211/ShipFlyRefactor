using System;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomWalkingState : State<BotsStateData>
{
    private const float radius = 10f;

    public override void OnEnter()
    {
        data.Agent.destination = data.Main.transform.position + Random.insideUnitCircle.ToVector3Z() * radius;
        
        if (data.Animator)
            data.Animator.SetBool(BotsStateData.run, true);
    }

    public override void OnUpdate()
    {
        if (Vector3.Distance(data.Agent.destination, data.Main.transform.position) < 0.5f)
            ChangeState<SelectState>();
    }
}