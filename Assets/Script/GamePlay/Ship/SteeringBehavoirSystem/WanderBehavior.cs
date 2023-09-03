using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderBehavior : SteeringBehavior
{
    [SerializeField] private float wanderRate = 0.4f;
    [SerializeField] private float wanderOffset = 1.5f;
    [SerializeField] private float wanderRadius = 4f;
    private float wanderOrientation = 0f;
    private float RandomBinomial()
    {
        return Random.value - Random.value;
    }
    private Vector3 OrientationToVector(float orientation)
    {
        return new Vector3(Mathf.Cos(orientation), Mathf.Sin(orientation),0);
    }

    //public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    //{
    //    SteeringData steering = new SteeringData();
    //    wanderOrientation += RandomBinomial() * wanderRate;
    //    float characterOrientation = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
    //    float targetOrientation = wanderOrientation + characterOrientation;
    //    Vector3 targetPosition = transform.position + (wanderOffset * OrientationToVector(characterOrientation));
    //    targetPosition += wanderRadius * OrientationToVector(targetOrientation); steering.linear = targetPosition - transform.position; steering.linear.Normalize();
    //    steering.linear *= steeringcontroller.maxAcceleration;
    //    return steering;
    //}
}
