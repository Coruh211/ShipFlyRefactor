using UnityEngine;

public class MoveToPositionState : State<BotsStateData>
{
    public override void OnEnter()
    {
        data.Agent.destination = data.PositionToMove;
        
        if (data.Animator)
            data.Animator.SetBool(BotsStateData.run, true);
    }
    
    public override void OnUpdate()
    {
        if (Vector3.Distance(data.Agent.destination, data.Main.transform.position) < 0.1f)
            ChangeState<SelectState>();
    }
}