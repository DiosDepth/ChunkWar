using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;


[System.Serializable]
public class CircleBehavior : SteeringBehavior
{
   
    [SerializeField] public float circleRadius = 7f;
    [SerializeField] public float angleSpeed = 5f;



    [BurstCompile]
    public struct CircleBehaviorJobs : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<BoidJobData> job_boidData;
        [ReadOnly] public NativeArray<SteeringControllerJobData> job_steeringControllerData;
        [ReadOnly] public float3 job_targetsPos;

        public NativeArray<SteeringBehaviorInfo> rv_Steerings;

        private SteeringBehaviorInfo steering;

        private float3 dir;
        private float3 desireddir;
        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex+ count; i++)
            {
                dir = math.normalize(job_boidData[i].position - job_targetsPos);

                desireddir.x = math.cos(job_steeringControllerData[i].circleData.circle_angleSpeed) * dir.x  - math.sin(job_steeringControllerData[i].circleData.circle_angleSpeed) * dir.y ;
                desireddir.y = math.sin(job_steeringControllerData[i].circleData.circle_angleSpeed) * dir.x + math.cos(job_steeringControllerData[i].circleData.circle_angleSpeed) * dir.y;

                desireddir = desireddir * job_steeringControllerData[i].circleData.circle_radius;


                steering.linear = desireddir - dir;

                if (math.length(steering.linear) > job_steeringControllerData[i].maxAcceleration)
                {
                    steering.linear = math.normalize(steering.linear);
                    steering.linear *= job_steeringControllerData[i].maxAcceleration;
                }

                rv_Steerings[i] = steering;
            }
        }
    }

    [BurstCompile]
    public struct CircleBehaviorJobs_OneOnOne : IJobParallelForBatch
    {

        [ReadOnly] public NativeArray<BoidJobData> job_boidData;
        [ReadOnly] public NativeArray<SteeringControllerJobData> job_steeringControllerData;
        [ReadOnly] public NativeArray<CircleMoveRefJobData> job_circleMoveJobData;

        public NativeArray<SteeringBehaviorInfo> rv_Steerings;

        private SteeringBehaviorInfo steering;

        private float3 dir;
        private float3 desiredpos;
        private float rad;
        float3 desiredvelocity;
        float3 velocity;
        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; i++)
            {
               //dir = float3.zero;
                dir = job_boidData[i].position - job_circleMoveJobData[i].position;
                if(math.length(dir) != 0)
                {
                    dir = math.normalize(dir);
                    rad = math.radians(job_steeringControllerData[i].circleData.circle_angleSpeed);
                    desiredpos.x = math.cos(rad) * dir.x - math.sin(rad) * dir.y;
                    desiredpos.y = math.sin(rad) * dir.x + math.cos(rad) * dir.y;
                    desiredpos = math.normalize(desiredpos) * job_steeringControllerData[i].circleData.circle_radius + job_circleMoveJobData[i].position;
                }
                else
                {
                    desiredpos = job_circleMoveJobData[i].facedirection * job_steeringControllerData[i].circleData.circle_radius + job_circleMoveJobData[i].position;
                }

                desiredvelocity = desiredpos - job_boidData[i].position;
                steering.linear = desiredvelocity - job_boidData[i].velocity;
    
                if (math.length(steering.linear) > job_steeringControllerData[i].maxAcceleration)
                {
                    steering.linear = math.normalize(steering.linear);
                    steering.linear *= job_steeringControllerData[i].maxAcceleration;
                }

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