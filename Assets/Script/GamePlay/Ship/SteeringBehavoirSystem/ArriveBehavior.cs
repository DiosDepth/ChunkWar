using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArriveBehavior : SteeringBehavior
{
   
    [SerializeField] private float targetRadius = 1.5f;
    [SerializeField] private float slowRadius = 5f;
    public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    {
        SteeringData steering = new SteeringData();
        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;
        if (distance < targetRadius)
        {
            steeringcontroller.SetVelocity(Vector3.zero);
            return steering;
        }

        float targetSpeed;
        if (distance > slowRadius)
            targetSpeed = steeringcontroller.maxAcceleration;
        else targetSpeed = steeringcontroller.maxAcceleration * (distance / slowRadius);
        Vector3 targetVelocity = direction;
        targetVelocity.Normalize();
        targetVelocity *= targetSpeed;
        steering.linear = targetVelocity - new Vector3(steeringcontroller.GetComponent<IBoid>().GetVelocity().x, steeringcontroller.GetComponent<IBoid>().GetVelocity().y, 0);
        if (steering.linear.magnitude > steeringcontroller.maxAcceleration)
        {
            steering.linear.Normalize();
            steering.linear *= steeringcontroller.maxAcceleration;
        }
        steering.angular = 0;
        return steering;
    }
}