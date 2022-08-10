using System;
using System.Linq;
using Ilumisoft.VisualStateMachine;
using UnityEngine;

[RequireComponent(typeof(StateData), typeof(StateMachine))]
public class State<T> : MonoBehaviour where T : StateData
{
    protected string stateName;
    protected T data;
    protected StateMachine stateMachine;

    private int skipFramesCount = 10;
    private int skipFramesTimer;
    private bool isEntered;
    private bool isChangedOnEnter;
    private string stateNameToChange;

    private void Awake()
    {
        stateName = GetType().Name;
        data = GetComponent<T>();
        stateMachine = GetComponent<StateMachine>();

        State state = stateMachine.Graph.GetState(stateName);
        if (state == null)
        {
            Debug.LogError("State " + stateName + " not found in state machine");
            return;
        }
        
        state.OnEnterState.AddListener(() =>
        {
            data.CurrentState = stateName;
            isEntered = false;
            isChangedOnEnter = false;
            OnEnter();
            skipFramesTimer = skipFramesCount;
        });
        
        state.OnUpdateState.AddListener(() =>
        {
            isEntered = true;
            
            if (isChangedOnEnter)
                ChangeState(stateNameToChange);
            else
            {
                OnUpdate();

                if (--skipFramesTimer <= 0)
                {
                    OnRareUpdate();
                    skipFramesTimer = skipFramesCount;
                }
            }
        });

        state.OnExitState.AddListener(() =>
        {
            data.PrevState = stateName;
            OnExit();
        });

        AwakeState();
    }
    
    protected virtual void AwakeState(){ }

    protected void SetRareUpdateSkipFrames(int skipFramesCount)
    {
        this.skipFramesCount = skipFramesCount;
    }

    protected bool IsCurrent()
    {
        return stateMachine.CurrentState.Equals(stateName);
    }

    protected void ChangeState<N>() where N : State<T>
    {
        string toStateName = typeof(N).Name;
        ChangeState(toStateName);
    }

    protected void ChangeState(string toStateName)
    {
        bool triggered = stateMachine.TryTriggerByState(toStateName);
        if (!triggered)
        {
            if (isEntered)
                Debug.LogError("Transition from " + stateName + " to " + toStateName + " not found");
            else
            {
                isChangedOnEnter = true;
                stateNameToChange = toStateName;
            }
        }
    }

    public bool IsFrom<N>()
    {
        return data.PrevState.Equals(typeof(N).Name);
    }
    
    public virtual void OnEnter()
    {

    }

    public virtual void OnUpdate()
    {

    }

    public virtual void OnRareUpdate()
    {
        
    }

    public virtual void OnExit()
    {

    }
}