using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public  class SteeringBehavior
{
    
    //[SerializeField] protected List<Transform> envAgents;
    [SerializeField] public Transform target ;
    [SerializeField] public float weight = 1f;


    public virtual float GetWeight() { return weight; }


    public virtual void SetTarget(Transform m_target)
    {
        target = m_target;
    }
}
