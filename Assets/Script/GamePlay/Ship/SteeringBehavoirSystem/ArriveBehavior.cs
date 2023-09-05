using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;


[System.Serializable]
public class ArriveBehavior : SteeringBehavior
{
   
    [SerializeField] public float arriveRadius = 1.5f;
    [SerializeField] public float slowRadius = 5f;




    [BurstCompatible]
    public struct ArriveBehaviorJobs : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<float3> job_aiShipPos;
        [ReadOnly] public NativeArray<float3>  job_aiShipVel;
        [ReadOnly] public NativeArray<float> job_aiShipRadius;
        [ReadOnly] public NativeArray<float> job_arriveRadius;
        [ReadOnly] public NativeArray<float> job_slowRadius;
        [ReadOnly] public NativeArray<float> job_maxAcceleration;

        [ReadOnly] public float3 job_targetPos;
        [ReadOnly] public float job_targetRadius;


        public NativeArray<bool> rv_isVelZero;
        public NativeArray<SteeringBehaviorInfo> rv_Steerings;



        private SteeringBehaviorInfo steering;
        private float3 direction;
        private float distance;
        private float speed;
        float3 velocity;
        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex+ count; i++)
            {
                direction = job_targetPos - job_aiShipPos[i];
                distance  = math.length(direction);

                if (distance < job_arriveRadius[i])
                {
                    rv_isVelZero[i] = true;
                    steering.linear = float3.zero;
                    steering.angular = 0;
                    rv_Steerings[i] = steering;
                    continue;
                }

                if (distance > job_slowRadius[i])
                {
                    speed = job_maxAcceleration[i];
                }
                else
                {
                    speed = job_maxAcceleration[i] * (distance / (job_slowRadius[i]));
                }

                velocity = math.normalize(direction);
                velocity *= speed;

                steering.linear = velocity - job_aiShipVel[i];
                if(math.length(steering.linear) > job_maxAcceleration[i])
                {
                    steering.linear = math.normalize(steering.linear);
                    steering.linear *= job_maxAcceleration[i];
                }

                steering.angular = 0;
                rv_Steerings[i] = steering;
            }

        }
    }

    //public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    //{
    //    SteeringData steering = new SteeringData();
    //    Vector3 direction = target.position - transform.position;
    //    float distance = direction.magnitude;
    //    if (distance < targetRadius)
    //    {
    //        steeringcontroller.SetVelocity(Vector3.zero);
    //        return steering;
    //    }

    //    float targetSpeed;
    //    if (distance > slowRadius)
    //        targetSpeed = steeringcontroller.maxAcceleration;
    //    else targetSpeed = steeringcontroller.maxAcceleration * (distance / slowRadius);
    //    Vector3 targetVelocity = direction;
    //    targetVelocity.Normalize();
    //    targetVelocity *= targetSpeed;
    //    steering.linear = targetVelocity - new Vector3(steeringcontroller.GetComponent<IBoid>().GetVelocity().x, steeringcontroller.GetComponent<IBoid>().GetVelocity().y, 0);
    //    if (steering.linear.magnitude > steeringcontroller.maxAcceleration)
    //    {
    //        steering.linear.Normalize();
    //        steering.linear *= steeringcontroller.maxAcceleration;
    //    }
    //    steering.angular = 0;
    //    return steering;
    //}


}