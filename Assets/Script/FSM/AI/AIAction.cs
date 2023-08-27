using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIAction : MonoBehaviour
{
    public string Label;

    public bool isDebug = false;
    public bool ActionInProgress { get; set; }


    protected AIController _controller;

   
    // Start is called before the first frame update

    public abstract void UpdateAction();
    public abstract void FixedUpdateAction();
    protected virtual void Start()
    {
        //_brain = this.gameObject.GetComponent<TDAIBrain>();
        //Initialization();
    }

    public virtual void SetController(AIController controller)
    {
        _controller = controller;
    }

    public virtual void Initialization()
    {

    }

    public virtual void OnEnterAction()
    {
        ActionInProgress = true;
    }

    public virtual void OnExitAction()
    {
        ActionInProgress = false;
    }
}
