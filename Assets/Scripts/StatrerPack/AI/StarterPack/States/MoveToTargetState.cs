using UnityEngine;

public class MoveToTargetState : State<BotsStateData>
{
    public override void OnEnter()
    {
        if (data.Animator)
            data.Animator.SetBool(BotsStateData.run, true);
    }

    public override void OnUpdate()
    {
        data.Agent.destination = data.TargetToMove.position;
        if (Vector3.Distance(data.Agent.destination, data.Main.transform.position) < 0.1f)
            ChangeState<SelectState>();
    }
}