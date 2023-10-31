using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEffect : MonoBehaviour, IPoolable
{
    private Transform _followTarget;
    private Transform m_trans;

    private Vector3 _originScale; 

    public virtual void Awake()
    {
        m_trans = transform;
        _originScale = transform.localScale;
    }


    public virtual void OnCreate()
    {
        EffectManager.Instance.RegisterEffect(this);
    }

    public void SetFollowTarget(Transform followTarget)
    {
        _followTarget = followTarget;
    }

    public void MultipleScale(float scale)
    {
        transform.localScale = new Vector3(_originScale.x * scale, _originScale.y * scale, _originScale.z * scale);
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
        transform.localScale = _originScale;
        EffectManager.Instance.RemoveEffect(this);
    }

    public void PoolableSetActive(bool isactive = true)
    {

    }
}
