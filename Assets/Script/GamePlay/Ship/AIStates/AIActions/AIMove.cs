using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMove : AIAction
{

    private Vector3 _moveDirection;
  



    public override void Initialization()
    {
        base.Initialization();

 
    }


    public override void OnEnterAction()
    {
        base.OnEnterAction();
    }

    public override void UpdateAction()
    {

            CalculateMoveDirection();
            Rotate();
            Move();


    }

    public override void OnExitAction()
    {
        base.OnExitAction();
        //_brain.seeingDic.Clear();
    }

    protected virtual void CalculateMoveDirection()
    {
        if(RogueManager.Instance.currentShip == null)
        {
            _moveDirection = Vector3.zero;
            return;
        }
        _moveDirection = (RogueManager.Instance.currentShip.transform.position - transform.position).normalized;
    }

    public void Move()
    {
        Vector2 _deltaMovement = _controller.CalculateDeltaMovement(_moveDirection);
        var newMovement = _controller.rb.position + _deltaMovement * Time.fixedDeltaTime;
        _controller.rb.MovePosition(newMovement);
    }

    public void Rotate()
    {
        _controller.transform.rotation = MathExtensionTools.CalculateRotation(_controller.transform.up, _moveDirection, _controller.rotateSpeed);
    }


    // Start is called before the first frame update
    protected override void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (!isDebug)
        {
            return;
        }
    }
}
