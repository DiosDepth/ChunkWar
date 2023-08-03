using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipController : BaseController
{

    public PlayerShip controlledTarget;


    // Start is called before the first frame update



    public  Vector3 WorldPointInput { get { return CameraManager.Instance.mainCamera.ScreenToWorldPoint(new Vector3(_pointInput.x, _pointInput.y, 0)); } }
    public  Vector3 WorldDirection { get { return (WorldPointInput - transform.position).normalized; } }
    public Vector2 PointInput { get { return _pointInput; } }
    private Vector2 _pointInput;







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
        HandleMovement();
        HandleRotation();
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

        Debug.Log("HandleAttackInput : " + context.phase);
        controlledTarget.mainWeapon.HandleWeapon(context);
    }

    public virtual void HandleMovement()
    {
        _deltaMovement = CalculateDeltaMovement(MovementInput);

        var newMovement =rb.position + _deltaMovement * Time.fixedDeltaTime;
        rb.MovePosition(newMovement);
    }

    public virtual void HandleRotation()
    {
        Debug.DrawLine(transform.position, WorldDirection * 100f, Color.red);
        Debug.DrawLine(transform.position, transform.up * 100f, Color.green);

        transform.rotation = CalculateRotation(WorldDirection);
    }

 
}
    
