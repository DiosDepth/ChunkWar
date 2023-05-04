using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct StateChangeEvent<T> where T: struct, IComparable, IConvertible, IFormattable
{
    public GameObject Target;
    public StateMachine<T> TargetStateMachine;
    public T NewState;
    public T PreviousState;

    public StateChangeEvent(StateMachine<T> statemachine)
    {
        Target = statemachine.Target;
        TargetStateMachine = statemachine;
        NewState = statemachine.CurrentState;
        PreviousState = statemachine.PreviousState;
    }
}

public interface IStateMachine
{
    bool TriggerEvents { get; set; }
}


public class StateMachine<T>: IStateMachine where T : struct, IComparable, IConvertible, IFormattable
{
    public bool TriggerEvents { get; set; }

    public GameObject Target;

    public T CurrentState { get; protected set; }
    public T PreviousState { get; protected set; }

    public StateMachine(GameObject target, bool triggerEvents)
    {
        this.Target = target;
        this.TriggerEvents = triggerEvents;
    }

    public virtual void ChangeState(T newState)
    {
        if(newState.Equals(CurrentState))
        {
            return;
        }

        PreviousState = CurrentState;
        CurrentState = newState;

        if(TriggerEvents)
        {
            EventCenter.Instance.TriggerEvent(new StateChangeEvent<T>(this));
        }
    }

    public virtual void RestorePreviousState()
    {
        CurrentState = PreviousState;
        if(TriggerEvents)
        {
            EventCenter.Instance.TriggerEvent(new StateChangeEvent<T>(this));
        }
    }

}
