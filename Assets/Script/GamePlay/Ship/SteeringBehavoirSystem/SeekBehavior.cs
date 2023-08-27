using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekBehavior : SteeringBehavior
{
  
    public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    {
        SteeringData steering = new SteeringData();
        steering.linear = target.position - transform.position;
        steering.linear.Normalize();
        steering.linear *= steeringcontroller.maxAcceleration;
        steering.angular = 0;
        return steering;
    }

}
