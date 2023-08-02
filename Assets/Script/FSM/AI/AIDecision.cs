using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIDecision : MonoBehaviour
{
    public bool isDebug = false;
    
    public string Label;
    public abstract bool Decide();

    public bool DecisionInProgress { get; set; }
    protected AIController _controller;



    public virtual void SetController(AIController controller)
    {
        _controller = controller;
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {

    }

    public virtual void Initialization()
    {

    }

    public virtual void OnEnterDecision()
    {
        DecisionInProgress = true;
    }

    public virtual void OnExitDecision()
    {
        DecisionInProgress = false;
    }
}
