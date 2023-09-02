using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipController : BaseController, IBoid
{

    public PlayerShip controlledTarget;


    // Start is called before the first frame update
    public Vector3 MovementInput { get { return _movementInput; } }
    protected Vector3 _movementInput;
    protected Vector2 _lastmovementInput;
    

    public float maxSpeed = 10;
    public float acceleration = 10;
    public float maxRotateSpeed = 15;
    public float boidRadius = 10f;




    public Vector3 WorldPointInput { get { return CameraManager.Instance.mainCamera.ScreenToWorldPoint(new Vector3(_pointInput.x, _pointInput.y, 0)); } }
    public  Vector3 WorldDirection { get { return (WorldPointInput - transform.position).normalized; } }
    public Vector2 PointInput { get { return _pointInput; } }
    protected Vector2 _pointInput;

    public float rotationAcceleration = 0.25f;
    public float rotationSpeedDamping = 0.25f;




    public float _refrotationspeed;
    public float _crossZ;

    protected Vector3 velocity;
    protected Vector3 lastpos;
    protected override void Start()
    {
        //InputDispatcher.Instance.Action_GamePlay_Attack += HandleAttackInput;
        base.Start();

    }
    public override void Initialization()
    {
        base.Initialization();

        if (LevelManager.Instance.currentLevel.levelName != "BattleLevel_001") { return; }

        controlledTarget = GetComponent<PlayerShip>();
        InputDispatcher.Instance.Action_GamePlay_Move += HandleMovementInput;
        InputDispatcher.Instance.Action_GamePlay_Point += HandlePointInput;
        InputDispatcher.Instance.Action_GamePlay_Attack += HandleAttackInput;

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (LevelManager.Instance.currentLevel.levelName != "BattleLevel_001") { return; }
        if (!IsUpdate) { return; }
        HandleRotation();
        HandleMainWeaponRotaion();
        //HandleOtherWeaponRotation();
    }

    protected override void FixedUpdate()
    {

        if (LevelManager.Instance.currentLevel.levelName != "BattleLevel_001") { return; }
        if (!IsUpdate) { return; }
        base.FixedUpdate();
        HandleMovement();
        UpdateIBoid();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        InputDispatcher.Instance.Action_GamePlay_Move -= HandleMovementInput;
        InputDispatcher.Instance.Action_GamePlay_Point -= HandlePointInput;
        InputDispatcher.Instance.Action_GamePlay_Attack -= HandleAttackInput;
    }
    public void HandleMovementInput(InputAction.CallbackContext context)
    {
        if (LevelManager.Instance.currentLevel.levelName != "BattleLevel_001") { return; }
        switch (context.phase)
        {
            case InputActionPhase.Disabled:
                break;
            case InputActionPhase.Waiting:
                break;
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Performed:
                _movementInput.x = context.ReadValue<Vector2>().x;
                _movementInput.y = context.ReadValue<Vector2>().y;
                _lastmovementInput = _movementInput;
                
                break;
            case InputActionPhase.Canceled:
                _movementInput.x = context.ReadValue<Vector2>().x;
                _movementInput.y = context.ReadValue<Vector2>().y;
                break;
        }
        
       // Debug.Log("HandleMovement : " + context.phase.ToString() + " Value = " + context.ReadValue<Vector2>());
    }
    public void HandlePointInput(InputAction.CallbackContext context)
    {
        if (LevelManager.Instance.currentLevel.levelName != "BattleLevel_001") { return; }
        switch (context.phase)
        {
            case InputActionPhase.Disabled:
                break;
            case InputActionPhase.Waiting:
                break;
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Performed:
                _pointInput = context.ReadValue<Vector2>();
                    
                break;
            case InputActionPhase.Canceled:
                break;
        }
        //Debug.Log("HandleRushDown : " + context.phase.ToString() + " Value = " + context.ReadValueAsButton());
    }


    public void HandleAttackInput(InputAction.CallbackContext context)
    {
        if(controlledTarget.conditionState.CurrentState == ShipConditionState.Freeze || 
            controlledTarget.conditionState.CurrentState == ShipConditionState.Death)
        {
            return;
        }
        Debug.Log("HandleAttackInput : " + context.phase);
        controlledTarget.mainWeapon.HandleWeapon(context);
    }

    public virtual void HandleMainWeaponRotaion()
    {
        if(controlledTarget.mainWeapon.rotationRoot == null)
        {
            return;
        }
        controlledTarget.mainWeapon.rotationRoot.rotation = MathExtensionTools.CalculateRotation(controlledTarget.mainWeapon.transform.up, WorldDirection, controlledTarget.mainWeapon.roatateSpeed);

    }

   

    public virtual void HandleMovement()
    {
        if(controlledTarget.conditionState.CurrentState == ShipConditionState.Immovable ||
            controlledTarget.conditionState.CurrentState == ShipConditionState.Freeze ||
            controlledTarget.conditionState.CurrentState == ShipConditionState.Death)
        {
            return;
        }
        
        _deltaMovement = CalculateDeltaMovement(MovementInput,acceleration, maxSpeed);

        if (Mathf.Approximately( _deltaMovement.sqrMagnitude , 0))
        {
            controlledTarget.movementState.ChangeState(ShipMovementState.Idle);
            _deltaMovement = Vector2.zero;
        }
        else
        {
            controlledTarget.movementState.ChangeState(ShipMovementState.Move);
            
        }


        var newMovement =rb.position + _deltaMovement * Time.fixedDeltaTime;
        rb.MovePosition(newMovement);
        
    }

    public virtual void HandleRotation()
    {

        if (MovementInput.sqrMagnitude == 0)
        {
            _crossZ = 0;
        }
        else
        {
            _crossZ = Vector3.Cross(transform.up, _lastmovementInput).z;
        }

        //Debug.DrawLine(transform.position, WorldDirection * 100f, Color.red);
        //Debug.DrawLine(transform.position, transform.up * 100f, Color.green);
        //calculate deacceleration ref rotation speed;
        //if(Mathf.Approximately(_crossZ,0))
        //{
        //    if(_refrotationspeed >0)
        //    {
        //        _refrotationspeed -= rotationSpeedDamping * Time.deltaTime;
        //    }
        //    if(_refrotationspeed < 0)
        //    {
        //        _refrotationspeed += rotationSpeedDamping * Time.deltaTime;
        //    }
        //    if(Mathf.Approximately(_refrotationspeed, 0))
        //    {
        //        _refrotationspeed = 0;
        //    }
        //}
        ////turn right
        //if(_crossZ <0)
        //{
        //    _refrotationspeed += -1 * rotationAcceleration * Time.deltaTime;
        //    if(_refrotationspeed <= (-1 * maxRotateSpeed))
        //    {
        //        _refrotationspeed = (-1 * maxRotateSpeed);
        //    }
        //}
        ////turn left
        //if (_crossZ >0)
        //{
        //    _refrotationspeed +=  rotationAcceleration * Time.deltaTime;
        //    if (_refrotationspeed >=  maxRotateSpeed)
        //    {
        //        _refrotationspeed = maxRotateSpeed;
        //    }
        //}
        if (Mathf.Approximately(_crossZ, 0))
        {
            _refrotationspeed -= rotationSpeedDamping * Time.deltaTime;
        }
        else
        {
            _refrotationspeed += rotationAcceleration * Time.deltaTime;
        }

        _refrotationspeed =  Mathf.Clamp(_refrotationspeed, 0, maxRotateSpeed);

        if(!Mathf.Approximately(_refrotationspeed, 0))
        {
            transform.rotation = MathExtensionTools.CalculateRotation(transform.up, _lastmovementInput, _refrotationspeed * Time.deltaTime);
        }

            //transform.Rotate(new Vector3(0, 0, ));
          
    }


    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    public float GetRadius()
    {
        return boidRadius;
    }

    public float GetRotationZ()
    {
        return transform.rotation.eulerAngles.z;
    }
    public void SetVelocity(Vector3 m_vect)
    {
        velocity = m_vect;

    }
    public void UpdateIBoid()
    {
        velocity = (transform.position - lastpos) / Time.deltaTime;
        lastpos = transform.position;
    }


}
    
