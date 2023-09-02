using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;

public class FaceBehavior : SteeringBehavior
{

    [BurstCompile]
    public struct FaceBehaviorJob : IJob
    {
        [ReadOnly] public float3 job_selfPos;
        [ReadOnly] public float3 job_selfVel;
        [ReadOnly] public float job_selfRotationZ;
        [ReadOnly] public float3 job_targetPos;
        [ReadOnly] public float job_targetRadius;

        [ReadOnly] public float job_maxAngularAcceleration;
        [ReadOnly] public float job_deltatime;

        public NativeArray<float> JRD_angular;

        public void Execute()
        {
            float angular;
            if (math.length(job_selfVel) == 0)
            {
                angular = math.degrees(math.atan2(job_targetPos.y - job_selfPos.y, job_targetPos.x - job_selfPos.x)) - 90;
                JRD_angular[0] = Mathf.LerpAngle(job_selfRotationZ, angular, job_maxAngularAcceleration * job_deltatime);
            }
            angular = math.degrees(math.atan2(job_selfVel.y, job_selfVel.x)) - 90;
            JRD_angular[0] = Mathf.LerpAngle(job_selfRotationZ, angular, job_maxAngularAcceleration * job_deltatime);
        }
    }

    //public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    //{
    //    SteeringData steering = new SteeringData();
    //    Vector3 velocity = GetComponent<IBoid>().GetVelocity();
    //    float angle;
    //    if (velocity.magnitude == 0)
    //    {
    //        angle = (Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x) * Mathf.Rad2Deg) - 90;
    //        steering.angular = Mathf.LerpAngle(transform.rotation.eulerAngles.z, angle, steeringcontroller.maxAngularAcceleration * Time.deltaTime);
    //        return steering;
    //    }
    

    //        angle = (Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg)- 90;
    //        steering.angular = Mathf.LerpAngle(transform.rotation.eulerAngles.z, angle, steeringcontroller.maxAngularAcceleration * Time.deltaTime);
    //        steering.linear = Vector3.zero;
    //        return steering;
    //}
    
}
