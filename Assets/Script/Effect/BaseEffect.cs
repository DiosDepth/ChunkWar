using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEffect : MonoBehaviour, IPoolable
{
    private Transform _followTarget;
    private Transform m_trans;

    public virtual void Awake()
    {
        m_trans = transform;
    }


    public virtual void OnCreate()
    {
        EffectManager.Instance.RegisterEffect(this);
    }

    public void SetFollowTarget(Transform followTarget)
    {
        _followTarget = followTarget;
    }

    public void OnUpdate()
    {
        if (_followTarget == null)
            return;

        m_trans.position = _followTarget.position;
        m_trans.rotation = _followTarget.rotation;
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(transform.name, gameObject);
    }

    public virtual void PoolableReset()
    {
        _followTarget = null;
        EffectManager.Instance.RemoveEffect(this);
    }

    public void PoolableSetActive(bool isactive = true)
    {

    }
}
