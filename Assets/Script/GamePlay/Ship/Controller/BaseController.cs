using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseController : MonoBehaviour, IPauseable
{
    public bool IsUpdate = false;


    public Rigidbody2D rb;
    public new Collider2D collider;

    protected Vector2 _lerpedInput;
    protected Vector2 _deltaMovement;
    protected float _deltaSpeed;
    protected float _deltaAcceleration;

    public virtual void Initialization()
    {
        GameManager.Instance.RegisterPauseable(this);
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
        if (m_dir.sqrMagnitude != 0)//���������벻Ϊ0 Ҳ�����в������������ʱ�� , �������˶�
        {
            _deltaAcceleration = Mathf.Lerp(_deltaAcceleration, 1, m_acceleration * Time.deltaTime);//������㵱ǰ֡�ļ��ٶȱ���, ����ÿ��ļ��ٶ���acceleration, ��ǰ�ļ��ٶȻ��0��ʼ��1 ��������������м���, ÿһ֡���ۻ����޽ӽ���1
            if (Mathf.Approximately(_deltaAcceleration, 1))
            {
                _deltaAcceleration = 1;
            }
            _lerpedInput = Vector2.ClampMagnitude(m_dir, _deltaAcceleration);
        }
        else //��inputΪ0��ʱ�򣬽��м����˶�
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
        _deltaSpeed = Mathf.Lerp(0, m_maxspeed, _deltaAcceleration) * m_speedmodify;//���ݵ�ǰ֡�ļ��ٱ��� ���㵱ǰ֡���ƶ��ٶ�
        _deltaMovement *= _deltaSpeed;

        //��������ƶ��ٶ�
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
    public virtual void PauseGame()
    {

    }
    public virtual void UnPauseGame()
    {
      
    }
}
