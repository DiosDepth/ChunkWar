using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipController : MonoBehaviour
{

    public Ship ship;
    public Rigidbody2D rb;
    public new CompositeCollider2D collider;

    // Start is called before the first frame update

    public Vector3 MovementInput { get { return _movementInput; } }
    private Vector3 _movementInput;

    public  Vector3 WorldPointInput { get { return CameraManager.Instance.mainCamera.ScreenToWorldPoint(new Vector3(_pointInput.x, _pointInput.y, 0)); } }
    public Vector2 PointInput { get { return _pointInput; } }
    private Vector2 _pointInput;


    public float maxSpeed = 10;
    public float acceleration = 10;
    public float thresholdMoveSpeed = 0.01f;
    public float chargeAttackMoveSpeedModifer = 0.3f;

    private Vector2 _lerpedInput;
    private Vector2 _deltaMovement;
    private float _deltaSpeed;
    private float _deltaAcceleration;



    void Start()
    {

        if(LevelManager.Instance.currentLevel.levelName != "BattleLevel_001") { return; }
        ship = GetComponent<Ship>();


        InputDispatcher.Instance.Action_GamePlay_Move += HandleMovementInput;
        InputDispatcher.Instance.Action_GamePlay_Point += HandlePointInput;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        InputDispatcher.Instance.Action_GamePlay_Move -= HandleMovementInput;
        InputDispatcher.Instance.Action_GamePlay_Point -= HandlePointInput;
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



    public virtual Vector2 CalculateDeltaMovement(Vector2 m_dir, float m_speedmodify = 1)
    {
        if (m_dir.sqrMagnitude != 0)//���������벻Ϊ0 Ҳ�����в������������ʱ�� , �������˶�
        {
            _deltaAcceleration = Mathf.Lerp(_deltaAcceleration, 1, acceleration * Time.deltaTime);//������㵱ǰ֡�ļ��ٶȱ���, ����ÿ��ļ��ٶ���acceleration, ��ǰ�ļ��ٶȻ��0��ʼ��1 ��������������м���, ÿһ֡���ۻ����޽ӽ���1
            if (Mathf.Approximately(_deltaAcceleration, 1))
            {
                _deltaAcceleration = 1;
            }
            _lerpedInput = Vector2.ClampMagnitude(m_dir, _deltaAcceleration);
        }
        else //��inputΪ0��ʱ�򣬽��м����˶�
        {
            _deltaAcceleration = Mathf.Lerp(_deltaAcceleration, 0, acceleration * Time.deltaTime);

            if (Mathf.Approximately(_deltaAcceleration, 0))
            {
                _deltaAcceleration = 0;
            }
            if (_deltaAcceleration == 0)
            {
                _lerpedInput = Vector3.zero;
            }
            else
            {
                _lerpedInput = Vector2.Lerp(_lerpedInput, _lerpedInput * _deltaAcceleration, acceleration * Time.deltaTime);
            }
        }



        _deltaMovement = _lerpedInput;
        _deltaSpeed = Mathf.Lerp(0, maxSpeed, _deltaAcceleration) * m_speedmodify;//���ݵ�ǰ֡�ļ��ٱ��� ���㵱ǰ֡���ƶ��ٶ�
        _deltaMovement *= _deltaSpeed;

        //��������ƶ��ٶ�
        if (_deltaMovement.magnitude > maxSpeed)
        {
            _deltaMovement = Vector3.ClampMagnitude(_deltaMovement, maxSpeed);
        }

        return _deltaMovement;

    }
}
    
