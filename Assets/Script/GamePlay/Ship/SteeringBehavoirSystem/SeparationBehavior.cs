using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeparationBehavior : SteeringBehavior
{
   
    [SerializeField] private float threshold = 2f;
    [SerializeField] private float decayCoefficient = -25f;
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

        for (int i = 0; i < LevelManager.Instance.aiShipList.Count; i++)
        {
            if (LevelManager.Instance.aiShipList[i].gameObject == gameObject)
            {
                continue;
            }
            Vector3 direction = LevelManager.Instance.aiShipList[i].transform.position - transform.position;
            float distance = direction.magnitude;
            if (distance < threshold)
            {
                float strength = Mathf.Min(decayCoefficient / (distance * distance), steeringcontroller.maxAcceleration);
                direction.Normalize();
                steering.linear += strength * direction;
            }
        }

        return steering;
    }
}
