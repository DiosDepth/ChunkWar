using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BaseController : MonoBehaviour, IPauseable, IBoid
{
    public bool IsUpdate = false;


    public Rigidbody2D rb;
    public new Collider2D collider;
    public float boidRadius = 1f;

    protected Vector2 _lerpedInput;
    protected Vector2 _deltaMovement;
    protected float _deltaSpeed;
    protected float _deltaAcceleration;

    protected Vector3 velocity;
    public Vector3 lastpos;

    public virtual void Initialization()
    {
        GameManager.Instance.RegisterPauseable(this);
    }

    void Awake()
    {
        if (rb == null)
            rb = transform.SafeGetComponent<Rigidbody2D>();

        if (collider == null)
            collider = transform.SafeGetComponent<Collider2D>();
    }

    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    protected virtual void FixedUpdate()
    {

    }

    protected virtual void OnDestroy()
    {
        GameManager.Instance.UnRegisterPauseable(this);
    }

    public virtual void SetControllerUpdate(bool isupdate)
    {
        IsUpdate = isupdate;
    }

    public virtual Vector2 CalculateDeltaMovement(Vector2 m_dir, float m_acceleration, float m_maxspeed, float m_speedmodify = 1)
    {
        if (m_dir.sqrMagnitude != 0)//当操作输入不为0 也就是有操作输入进来的时候 , 做加速运动
        {
            _deltaAcceleration = Mathf.Lerp(_deltaAcceleration, 1, m_acceleration * Time.deltaTime);//这里计算当前帧的加速度比例, 比如每秒的加速度是acceleration, 当前的加速度会从0开始到1 按照这个比例进行计算, 每一帧积累会无限接近于1
            if (Mathf.Approximately(_deltaAcceleration, 1))
            {
                _deltaAcceleration = 1;
            }
            _lerpedInput = Vector2.ClampMagnitude(m_dir, _deltaAcceleration);
        }
        else //当input为0的时候，进行减速运动
        {
            _deltaAcceleration = Mathf.Lerp(_deltaAcceleration, 0, Time.deltaTime);

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
                _lerpedInput = Vector2.Lerp(_lerpedInput, _lerpedInput * _deltaAcceleration, Time.deltaTime);
            }
        }



        _deltaMovement = _lerpedInput;
        _deltaSpeed = Mathf.Lerp(0, m_maxspeed, _deltaAcceleration) * m_speedmodify;//根据当前帧的加速比例 计算当前帧的移动速度
        _deltaMovement *= _deltaSpeed;

        //限制最大移动速度
        if (_deltaMovement.magnitude > m_maxspeed)
        {
            _deltaMovement = Vector3.ClampMagnitude(_deltaMovement, m_maxspeed);
        }

        return _deltaMovement;

    }

    public virtual void GameOver()
    {
        SetControllerUpdate(false);

    }


    public virtual Vector3 GetPosition()
    {
        return transform.position;
    }

    public virtual Vector3 GetVelocity()
    {
        return (transform.position - lastpos) / Time.fixedDeltaTime;
    }

    public virtual float GetRadius()
    {
        return boidRadius;
    }


    public virtual float GetRotationZ()
    {
        return math.degrees(math.atan2(transform.up.y, transform.up.x)) - 90;
        //return transform.rotation.eulerAngles.z;
    }

    public virtual void SetVelocity(Vector3 m_vect)
    {
        velocity = m_vect;

    }
    public virtual void UpdateBoid()
    {
        //velocity = (transform.position - lastpos) / Time.fixedDeltaTime;
        lastpos = transform.position;
    }
    public virtual void PauseGame()
    {

    }
    public virtual void UnPauseGame()
    {
      
    }
}
