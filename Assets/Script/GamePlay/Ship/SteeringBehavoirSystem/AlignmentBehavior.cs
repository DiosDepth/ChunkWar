using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignmentBehavior : SteeringBehavior
{
    [SerializeField] private float alignDistance = 8f;
    void Start()
    {
        //SteeringBehaviorBase[] agents = FindObjectsOfType<SteeringBehaviorBase>();
        //targets = new Transform[agents.Length - 1];
        //int count = 0;
        //foreach (SteeringBehaviorBase agent in agents)
        //{
        //    if (agent.gameObject != gameObject)
        //    {
        //        targets[count] = agent.transform; count++;
        //    }
        //}
    }

    public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    {
        SteeringData steering = new SteeringData();
        steering.linear = Vector3.zero; int count = 0;
        for (int i = 0; i < LevelManager.Instance.aiShipList.Count; i++)
        {
            if (LevelManager.Instance.aiShipList[i].gameObject == gameObject)
            {
                continue;
            }
            Vector3 targetDir = LevelManager.Instance.aiShipList[i].transform.position - transform.position;
            if (targetDir.magnitude < alignDistance)
            {
                steering.linear += new Vector3(LevelManager.Instance.aiShipList[i].GetComponent<IBoid>().GetVelocity().x, LevelManager.Instance.aiShipList[i].GetComponent<IBoid>().GetVelocity().y, 0);
                count++;
            }

        }
        if (count > 0)
        {
            steering.linear = steering.linear / count;
            if (steering.linear.magnitude > steeringcontroller.maxAcceleration)
            {
                steering.linear = steering.linear.normalized * steeringcontroller.maxAcceleration;
            }
        }
        return steering;
    }

}