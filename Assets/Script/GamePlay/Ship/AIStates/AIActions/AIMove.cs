using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMove : AIAction
{
    public float maxSpeed = 7;
    public float acceleration = 15;
    public float maxRotateSpeed = 15;

    private Vector3 _moveDirection;
    private Vector3 _randomdir;
    private Vector3 _destination;
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

    public override void FixedUpdateAction()
    {
       
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

        
        if(RogueManager.Instance.currentShip.transform.position.SqrDistanceXY(this.transform.position) <= Mathf.Pow(30f, 2))
        {
            _randomdir = MathExtensionTools.GetRandomDirection((transform.position - RogueManager.Instance.currentShip.transform.position).normalized, 45);
            _destination = _randomdir * (20f + UnityEngine.Random.Range(-1, 1)) + RogueManager.Instance.currentShip.transform.position;

            _moveDirection = (_destination - transform.position).normalized;
        }
        else
        {
            _moveDirection = (RogueManager.Instance.currentShip.transform.position - this.transform.position).normalized;
        }
    }

    public void Move()
    {
        Vector2 _deltaMovement = _controller.CalculateDeltaMovement(_moveDirection,acceleration,maxSpeed);
        var newMovement = _controller.rb.position + _deltaMovement * Time.fixedDeltaTime;
        _controller.rb.MovePosition(newMovement);
    }

    public void Rotate()
    {
        _controller.transform.rotation = MathExtensionTools.CalculateRotation(_controller.transform.up, _moveDirection,maxRotateSpeed);
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

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, _destination);
        Gizmos.DrawWireSphere(_destination, 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, _moveDirection * 5f + transform.position);
    }
}
