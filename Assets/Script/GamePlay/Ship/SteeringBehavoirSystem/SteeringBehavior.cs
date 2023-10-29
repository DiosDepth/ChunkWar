using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public  class SteeringBehavior
{
    
    //[SerializeField] protected List<Transform> envAgents;
    [SerializeField] public Transform target ;
    [SerializeField] protected float weight = 1f;

    public virtual void SetWeight(float m_weight) { weight = m_weight; }
    public virtual float GetWeight() { return weight; }


    public virtual void SetTarget(Transform m_target)
    {
        target = m_target;
    }
}
