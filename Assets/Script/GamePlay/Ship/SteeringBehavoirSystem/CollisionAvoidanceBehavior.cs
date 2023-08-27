using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAvoidanceBehavior : SteeringBehavior
{
    
    [SerializeField] private float radius;

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
        float shortestTime = float.PositiveInfinity;
        Transform firstTarget = null;
        float firstMinSeparation = 0, firstDistance = 0, firstRadius = 0;
        Vector3 firstRelativePos = Vector3.zero, firstRelativeVel = Vector3.zero;

        for (int i = 0; i < LevelManager.Instance.aiShipList.Count; i++)
        {
            if (LevelManager.Instance.aiShipList[i].gameObject == gameObject)
            {
                continue;
            }
            Vector3 relativePos = transform.position - LevelManager.Instance.aiShipList[i].transform.position;
            Vector3 relativeVel = GetComponent<IBoid>().GetVelocity() - LevelManager.Instance.aiShipList[i].GetComponent<IBoid>().GetVelocity();
            float distance = relativePos.magnitude;
            float relativeSpeed = relativeVel.magnitude;

            if (relativeSpeed == 0)
                continue;

            float timeToCollision = -1 * Vector3.Dot(relativePos, relativeVel) / (relativeSpeed * relativeSpeed);

            Vector3 separation = relativePos + relativeVel * timeToCollision;
            float minSeparation = separation.magnitude;

            if (minSeparation > radius + radius)
                continue;

            if ((timeToCollision > 0) && (timeToCollision < shortestTime))
            {
                shortestTime = timeToCollision;
                firstTarget = LevelManager.Instance.aiShipList[i].transform;
                firstMinSeparation = minSeparation;
                firstDistance = distance;
                firstRelativePos = relativePos;
                firstRelativeVel = relativeVel;
                firstRadius = radius;
            }
        }

        if (firstTarget == null)
            return steering;

        if (firstMinSeparation <= 0 || firstDistance < radius + firstRadius)
            steering.linear = transform.position - firstTarget.position;
        else
            steering.linear = firstRelativePos + firstRelativeVel * shortestTime;

        steering.linear.Normalize();
        steering.linear *= steeringcontroller.maxAcceleration;

        return steering;
    }


}