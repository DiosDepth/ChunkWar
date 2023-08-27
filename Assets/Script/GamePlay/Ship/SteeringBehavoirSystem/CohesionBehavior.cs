using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CohesionBehavior : SteeringBehavior
{
    
    [SerializeField] private float viewAngle = 60f;
    void Start()
    {
        //SteeringBehaviorBase[] agents = FindObjectsOfType<SteeringBehaviorBase>();
        //targets = new Transform[agents.Length - 1];
        //int count = 0;
        //foreach (SteeringBehaviorBase agent in agents)
        //{
        //    if (agent.gameObject != gameObject)
        //    {
        //        targets[count] = agent.transform;
        //        count++;
        //    }
        //}
    }

    public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    {
        SteeringData steering = new SteeringData();
        Vector3 centerOfMass = Vector3.zero; int count = 0;
        for (int i = 0; i < LevelManager.Instance.aiShipList.Count; i++)
        {
            if (LevelManager.Instance.aiShipList[i].gameObject == gameObject)
            {
                continue;
            }
            Vector3 targetDir = LevelManager.Instance.aiShipList[i].transform.position - transform.position;
            if (Vector3.Angle(targetDir, transform.forward) < viewAngle)
            {
                centerOfMass += LevelManager.Instance.aiShipList[i].transform.position;
                count++;
            }

        }
        if (count > 0)
        {
            centerOfMass = centerOfMass / count;
            steering.linear = centerOfMass - transform.position;
            steering.linear.Normalize();
            steering.linear *= steeringcontroller.maxAcceleration;
        }
        return steering;
    }
}
