using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public Istate CurrentState { get; private set; }
    public Istate BaseState { get; private set; }
    public Istate LastState { get; private set; }

    public StateMachine(Istate defaultState)
    {
        BaseState = defaultState;
        CurrentState = defaultState;
        LastState = CurrentState;
    }

    public void SetState(Istate state)
    {
        if (CurrentState == state) return;

        LastState = CurrentState;

        CurrentState.OperateExit();
        CurrentState = state;
        CurrentState.OperateEnter();
    }

    public void SetBaseState() => CurrentState = LastState;
    
    public void SetLastState() => CurrentState = LastState;

    public void DoOperateUpdate() => CurrentState.OperateUpdate();

    public void DoOperateFixedUpdate() => CurrentState.OperateFixedUpdate();
}
