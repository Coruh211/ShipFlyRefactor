using UnityEngine;

public class IdleState : State<BotsStateData>
{
    private void Start()
    {
        EventManager.OnStartGame.Subscribe(() =>
        {
            if (IsCurrent())
                ChangeState<SelectState>();
        });
    }

    public override void OnEnter()
    {
        data.Agent.destination = data.Main.transform.position;
        
        if (data.Animator)
            data.Animator.SetBool(BotsStateData.run, false);
    }
}