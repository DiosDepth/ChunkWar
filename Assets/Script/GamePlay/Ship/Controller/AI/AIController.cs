using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class AIController : BaseController
{


    public AIShip controlledTarget;
    public float updateFrequence = 0.05f;
    [ListDrawerSettings(DraggableItems = true,Expanded =true)]
    public List<AIState> States;


    public AIState currentState;

    public AIState previousState;
   

    protected AIDecision[] _decisions;


    protected float _nextupdatetime;
    

    public override void Initialization()
    {
        base.Initialization();
        controlledTarget = GetComponent<AIShip>();
  

        InitializeStates();
        InitializeDecisions();
        if (States.Count > 0)
        {
            TransitionToState(States[0].StateName);
        }
        IsUpdate = true;
    
    }

    protected virtual void InitializeStates()
    {
        foreach (AIState state in States)
        {
            state.SetController(this);
            foreach (AIAction action in state.Actions)
            {
                if (action != null)
                {
                    action.Initialization();
                }
            }
            foreach (AITransition transition in state.Transitions)
            {
                if (transition != null)
                {
                    transition.Initialization();
                }

            }
        }
    }
    protected virtual void InitializeDecisions()
    {
        _decisions = this.gameObject.GetComponents<AIDecision>();
        foreach (AIDecision decision in _decisions)
        {
            decision.Initialization();
        }
    }

    public virtual void UpdateState()
    {
        if (!IsUpdate || currentState == null)
        {
            return;
        }
        if(Time.time - _nextupdatetime >= updateFrequence)
        {
            currentState.OnUpdate();
            _nextupdatetime = Time.time;
        }
        
    }

    public virtual void TransitionToState(string newstatename)
    {
        if (newstatename != currentState.StateName)
        {
            currentState?.OnExit();
            OnExitState();
            previousState = currentState;
            currentState = FindState(newstatename);
            if (currentState != null)
            {
                currentState.OnEnter();
            }
        }
    }

    protected virtual void OnExitState()
    {

    }

    protected virtual AIState FindState(string statename)
    {
        foreach (AIState state in States)
        {
            if (state.StateName == statename)
            {
                return state;
            }
        }
        Debug.LogError("You can't find a state " + statename + " in " + this.gameObject.name + "'s AIControoler, but there is no state of this name exists");
        return null;
    }

    






    // Start is called before the first frame update
    protected override  void Start()
    {
        base.Start();
    }

    // Update is called once per frame
   protected override void Update()
    {
        base.Update();
        UpdateState();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }




}
