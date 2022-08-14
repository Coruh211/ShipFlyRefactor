using UnityEngine;

public class SelectState : State<BotsStateData>
{
    public override void OnEnter()
    {
        float random = Random.value;
        if (random > 0f)
            ChangeState<RandomWalkingState>();
    }
}