using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SteeringData
{
    public Vector3 linear;
    public float angular;
    public SteeringData()
    {
        linear = Vector3.zero;
        angular = 0f;
    }
}

[System.Serializable]
public class KinmaticData
{
    public Vector2 velocity;
    public Vector2 lastPosition;
    public float movementDrag;
    public float avoidenRadius;

}
public  class SteeringBehavior : MonoBehaviour
{
    
    //[SerializeField] protected List<Transform> envAgents;
    [SerializeField] protected Transform target ;
    [SerializeField] protected float weight = 1f;
    public virtual SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    {
        return null;
    }

    public virtual float GetWeight() { return weight; }

    public virtual void SetEnvAgents(List<Transform> m_agents)
    {
        //envAgents = m_agents;
        //for (int i = 0; i < m_agents.Count; i++)
        //{
        //    if (m_agents[i].gameObject != gameObject)
        //    {
        //        envAgents.Add(m_agents[i]);
        //    }
        //}
    }

    public virtual void SetTarget(Transform m_target)
    {
        target = m_target;
    }
}
