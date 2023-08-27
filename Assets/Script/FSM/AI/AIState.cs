using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;



[System.Serializable]
public class AIActionsList : List<AIAction>
{
}

[System.Serializable]
public class AITransitionsList : List<AITransition>
{
}


[System.Serializable]
public class AIState : State
{
    [SerializeField]
    public string StateName;
   
    [ListDrawerSettings(DraggableItems = true, Expanded = true)]
    // [Reorderable(null, "Action", null)]
    public List<AIAction> Actions;

    [ListDrawerSettings(DraggableItems = true, Expanded = true)]
    public List<AIAction> FixedActions;
    [ListDrawerSettings(DraggableItems = true, Expanded = true)]
    // [Reorderable(null, "Transitions", null)]
    public List<AITransition> Transitions;

    protected AIController _controller;


    public virtual void SetController(AIController controller)
    {
        _controller = controller;
        foreach (AIAction action in Actions)
        {
            if (action != null)
            {
                action.SetController(_controller);
            }

        }
        foreach (AITransition transition in Transitions)
        {
            if (transition != null)
            {
                if (transition.Decision != null)
                {
                    transition.Decision.SetController(_controller);
                }

            }

        }
    }

    public override void OnEnter()
    {
        foreach (AIAction action in Actions)
        {
            action.OnEnterAction();
        }

        foreach (AITransition transition in Transitions)
        {
            if (transition.Decision != null)
            {
                transition.Decision.OnEnterDecision();
            }
        }
    }

    public override void OnUpdate()
    {
        UpdateActions();
        UpdateTrasitions();
    }

    public override void OnFixedUpdate()
    {
        FixedUpdateActions();
        //UpdateTrasitions();
    }

    public override void OnExit()
    {
        foreach (AIAction action in Actions)
        {
            if(action != null)
            {
                action.OnExitAction();
            }

        }

        foreach (AITransition transition in Transitions)
        {
            if (transition.Decision != null)
            {
                transition.Decision.OnExitDecision();
            }
        }
    }

    public virtual void UpdateActions()
    {
        if (Actions.Count == 0) { return; }
        for (int i = 0; i < Actions.Count; i++)
        {
            if (Actions[i] != null)
            {
                Actions[i].UpdateAction();
            }
            else
            {
                Debug.LogError("An action in " + _controller.gameObject.name + " is null");
            }
        }
    }

    public virtual void FixedUpdateActions()
    {
        if (FixedActions.Count == 0) { return; }
        for (int i = 0; i < FixedActions.Count; i++)
        {
            if (FixedActions[i] != null)
            {
                FixedActions[i].FixedUpdateAction();
            }
            else
            {
                Debug.LogError("An fixed action in " + _controller.gameObject.name + " is null");
            }
        }
    }

    public virtual void UpdateTrasitions()
    {
        if (Transitions.Count == 0) { return; }
        for (int i = 0; i < Transitions.Count; i++)
        {
            if (Transitions[i].Decision != null)
            {
                if (Transitions[i].Decision.Decide())
                {
                    if (Transitions[i].TrueState != "")
                    {
                        _controller.TransitionToState(Transitions[i].TrueState);
                    }
                }
                else
                {
                    if (Transitions[i].FalseState != "")
                    {
                        _controller.TransitionToState(Transitions[i].FalseState);
                    }
                }
            }
        }
    }
}
