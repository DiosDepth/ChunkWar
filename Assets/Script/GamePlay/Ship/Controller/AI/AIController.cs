using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class AIController : BaseController
{

    public bool isComplexAI = false;
    public AIShip controlledTarget;

    [ListDrawerSettings(DraggableItems = true,Expanded =true)]
    public List<AIState> States;


    public AIState currentState;

    public AIState previousState;
   

    protected AIDecision[] _decisions;

    public Dictionary<string, List<GameObject>> hearingDic = new Dictionary<string, List<GameObject>>();
    public Dictionary<string, List<GameObject>> seeingDic = new Dictionary<string, List<GameObject>>();

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
        currentState.OnUpdate();
    }

    public virtual void TransitionToState(string newstatename)
    {
        if (newstatename != currentState.StateName)
        {
            currentState.OnExit();
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


    public virtual List<GameObject> GetHearingTargetList(string layername)
    {
        if (hearingDic.ContainsKey(layername))
        {
            return hearingDic[layername];
        }
        return null;
    }

    public virtual List<GameObject> GetSeeingTargetList(string layername)
    {
        if (seeingDic.ContainsKey(layername))
        {
            return seeingDic[layername];
        }
        return null;
    }

    public virtual GameObject GetHearingPlayerTarget(string layername = "Player")
    {
        if (hearingDic.ContainsKey("Player"))
        {
            return hearingDic["Player"][0];
        }
        return null;
    }

    public virtual GameObject GetSeeingPlayerTarget(string layername = "Player")
    {
        if (seeingDic.ContainsKey("Player"))
        {
            return hearingDic["Player"][0];
        }
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
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }




}
