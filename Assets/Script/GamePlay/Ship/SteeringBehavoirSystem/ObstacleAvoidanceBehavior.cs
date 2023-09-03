using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleAvoidanceBehavior : SteeringBehavior
{
    [SerializeField] private float avoidDistance = 1f;
    [SerializeField] private float lookahead = 2f;
    [SerializeField] private float sideViewAngle = 45f;
    [SerializeField] public LayerMask mask;
    //public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    //{
    //    SteeringData steering = new SteeringData();
    //    Vector3[] rayVector = new Vector3[3];
    //    rayVector[0] = GetComponent<IBoid>().GetVelocity();
    //    rayVector[0].Normalize();
    //    rayVector[0] *= lookahead;
    //    float rayOrientation = Mathf.Atan2(GetComponent<IBoid>().GetVelocity().y,GetComponent<IBoid>().GetVelocity().x);
    //    float rightRayOrientation = rayOrientation + (sideViewAngle * Mathf.Deg2Rad);
    //    float leftRayOrientation = rayOrientation - (sideViewAngle * Mathf.Deg2Rad);

    //    rayVector[1] = new Vector3(Mathf.Cos(rightRayOrientation), Mathf.Sin(rightRayOrientation), 0);
    //    rayVector[1].Normalize();
    //    rayVector[1] *= lookahead;

    //    rayVector[2] = new Vector3(Mathf.Cos(leftRayOrientation), Mathf.Sin(leftRayOrientation), 0 );
    //    rayVector[2].Normalize();
    //    rayVector[2] *= lookahead;

    //    for (int i = 0; i < rayVector.Length; i++)
    //    {
    //        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayVector[i], lookahead, mask);
    //        if (hit)
    //        {
    //            Vector3 target = hit.point + (hit.normal * avoidDistance);
    //            steering.linear = target - transform.position;
    //            steering.linear.Normalize();
    //            steering.linear *= steeringcontroller.maxAcceleration;
    //            Debug.DrawLine(transform.position, transform.position + rayVector[i], Color.green, 1f);
    //            break;

    //        }
    //        Debug.DrawLine(transform.position, transform.position + rayVector[i], Color.red);
    //    }
    //    return steering;
    //}
}
