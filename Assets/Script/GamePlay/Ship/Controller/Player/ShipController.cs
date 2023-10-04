using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

public class ShipController : BaseController, IBoid
{

    public PlayerShip targetShip;
    public ShipUnitManager shipUnitManager;


    // Start is called before the first frame update
    public Vector3 MovementInput { get { return _movementInput; } }
    protected Vector3 _movementInput;
    protected Vector2 _lastmovementInput;
    
    [ShowInInspector]
    private float maxSpeed = 10;
    [ShowInInspector]
    private float acceleration = 10;
    [ShowInInspector]
    private float maxRotateSpeed = 15;
    private float boidRadius = 20f;

     [ShowInInspector]
    private float rotationAcceleration = 0.25f;
    private float rotationSpeedDamping = 0.25f;

    /// <summary>
    /// 舰船等级速度修正
    /// </summary>
    private float shipClass_SpeedRatio = 1f;
    /// <summary>
    /// 是否正在移动
    /// </summary>
    private bool _isShipMoving = false;

    public Vector3 WorldPointInput { get { return CameraManager.Instance.mainCamera.ScreenToWorldPoint(new Vector3(_pointInput.x, _pointInput.y, 0)); } }
    public  Vector3 WorldDirection { get { return (WorldPointInput - transform.position).normalized; } }
    public Vector2 PointInput { get { return _pointInput; } }
    protected Vector2 _pointInput;

    protected Vector3 velocity;
    protected Vector3 lastpos;

    protected ShipControlConfig _controlConfig;
    private LevelManager _levelMgr;

    protected override void Start()
    {
        //InputDispatcher.Instance.Action_GamePlay_Attack += HandleAttackInput;
        base.Start();
    }
    public override void Initialization()
    {
        base.Initialization();
        _levelMgr = LevelManager.Instance;
   
        if (!_levelMgr.IsBattleLevel()) { return; }

        targetShip = GetComponent<PlayerShip>();
        InitShipControlData();
        BindPropertyChangeAction();
        InitUnitManagerData();
        InputDispatcher.Instance.Action_GamePlay_Move += HandleMovementInput;
        InputDispatcher.Instance.Action_GamePlay_Point += HandlePointInput;
        InputDispatcher.Instance.Action_GamePlay_Attack += HandleAttackInput;

    }

    public virtual void InitUnitManagerData()
    {
        if(shipUnitManager == null)
        {
            shipUnitManager = new ShipUnitManager();
            shipUnitManager.Initialization(targetShip);
        }
     
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        base.Update();
        if (!_levelMgr.IsBattleLevel()) { return; }
        if (!IsUpdate) { return; }
        HandleRotation();

        // if weapon mode is Autonomy than using HandleShipAutonomyMainWeapon to process weapon 
        //other wise using Ship Controller Update to process weapon (cus ship controller need to listen player input in every frame)

        targetShip.mainWeapon.ProcessWeapon();
        shipUnitManager?.Update();

    }

    protected override void FixedUpdate()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!_levelMgr.IsBattleLevel()) { return; }
        if (!IsUpdate) { return; }
        base.FixedUpdate();
        HandleMovement();

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnBindPropertyChangeAction();
        InputDispatcher.Instance.Action_GamePlay_Move -= HandleMovementInput;
        InputDispatcher.Instance.Action_GamePlay_Point -= HandlePointInput;
        InputDispatcher.Instance.Action_GamePlay_Attack -= HandleAttackInput;
    }
    public void HandleMovementInput(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!IsUpdate) { return; }
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
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!IsUpdate) { return; }
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
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!IsUpdate) { return; }
        if (targetShip.conditionState.CurrentState == ShipConditionState.Freeze || 
            targetShip.conditionState.CurrentState == ShipConditionState.Death)
        {
            return;
        }
        Debug.Log("HandleAttackInput : " + context.phase);
        targetShip.mainWeapon.HandleShipMainWeapon(context);
    }

    public virtual void HandleMainWeaponRotaion()
    {
        if(targetShip.mainWeapon.rotationRoot == null)
        {
            return;
        }
        targetShip.mainWeapon.rotationRoot.rotation = MathExtensionTools.CalculateRotation(targetShip.mainWeapon.transform.up, WorldDirection, targetShip.mainWeapon.roatateSpeed);

    }

   

    public virtual void HandleMovement()
    {
        if (!IsUpdate) { return; }
        if (targetShip.conditionState.CurrentState == ShipConditionState.Immovable ||
            targetShip.conditionState.CurrentState == ShipConditionState.Freeze ||
            targetShip.conditionState.CurrentState == ShipConditionState.Death)
        {
            return;
        }
        
        _deltaMovement = CalculateDeltaMovement(MovementInput,acceleration, maxSpeed);

        if (Mathf.Approximately( _deltaMovement.sqrMagnitude , 0))
        {
            targetShip.movementState.ChangeState(ShipMovementState.Idle);
            _deltaMovement = Vector2.zero;
            if (_isShipMoving)
            {
                ShipStateEvent.Trigger(targetShip, null, ShipMovementState.Idle, ShipConditionState.Normal, true, true);
                _isShipMoving = false;
            }
        }
        else
        {
            targetShip.movementState.ChangeState(ShipMovementState.Move);
            if (!_isShipMoving)
            {
                ShipStateEvent.Trigger(targetShip, null, ShipMovementState.Move, ShipConditionState.Normal, true, true);
                _isShipMoving = true;
            }
        }


        var newMovement =rb.position + _deltaMovement * Time.fixedDeltaTime;
        rb.MovePosition(newMovement);
        
    }

    public virtual void HandleRotation()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!IsUpdate) { return; }
        //if (MovementInput.sqrMagnitude == 0)
        //{
        //    _crossZ = 0;
        //}
        //else
        //{
        //    _crossZ = Vector3.Cross(transform.up, _lastmovementInput).z;
        //}

        //if (Mathf.Approximately(_crossZ, 0))
        //{
        //    _refrotationspeed -= rotationSpeedDamping * Time.deltaTime;
        //}
        //else
        //{
        //    _refrotationspeed += rotationAcceleration * Time.deltaTime;
        //}

        //_refrotationspeed =  Mathf.Clamp(_refrotationspeed, 0, maxRotateSpeed);

        //if(!Mathf.Approximately(_refrotationspeed, 0))
        //{
        //    transform.rotation = MathExtensionTools.CalculateRotation(transform.up, _lastmovementInput, _refrotationspeed * Time.deltaTime);
        //}
        transform.rotation = MathExtensionTools.CalculateRotation(transform.up, WorldDirection, maxRotateSpeed * Time.deltaTime);
    }


    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Vector3 GetVelocity()
    {
        return (transform.position - lastpos) / Time.fixedDeltaTime;
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

    public override void GameOver()
    {
        base.GameOver();
        targetShip.GameOver();
        shipUnitManager.GameOver();
    }

    #region Private

    private void InitShipControlData()
    {
        _controlConfig = DataManager.Instance.gameMiscCfg.ShipControlCfg;
        if(targetShip != null)
        {
            rotationAcceleration = _controlConfig.RotationAcceleration;
            var classCfg = DataManager.Instance.gameMiscCfg.GetShipClassConfig(targetShip.playerShipCfg.ShipClass);
            if(classCfg != null)
            {
                shipClass_SpeedRatio = classCfg.BaseSpeedRatio;
            }
            CalculateShipMoveSpeed();
        }
    }

    private void BindPropertyChangeAction()
    {
        RogueManager.Instance.MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.ShipSpeed, CalculateShipMoveSpeed);
    }

    private void UnBindPropertyChangeAction()
    {
        RogueManager.Instance.MainPropertyData.UnBindPropertyChangeAction(PropertyModifyKey.ShipSpeed, CalculateShipMoveSpeed);
    }

    private void CalculateShipMoveSpeed()
    {
        var speedModify = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.ShipSpeed);
        speedModify = Mathf.Clamp(speedModify / 100f + 1, 0, GameGlobalConfig.ShipSpeedModify_Protected_MaxSpeed);
        maxSpeed = (speedModify) * _controlConfig.MaxSpeed_Base * shipClass_SpeedRatio;
        maxRotateSpeed = (speedModify) * _controlConfig.MaxRotateSpeed_Base * shipClass_SpeedRatio;
        acceleration = (speedModify) * _controlConfig.Acceleration_Base * shipClass_SpeedRatio;
    }

    #endregion
}

