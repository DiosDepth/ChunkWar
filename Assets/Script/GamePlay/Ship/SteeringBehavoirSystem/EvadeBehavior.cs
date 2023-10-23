using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[System.Serializable]
public class EvadeBehavior : SteeringBehavior
{
    [SerializeField] public float maxPrediction = 2f;
    public struct EvadeBehaviorJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<BoidJobData> job_boidData;
        [ReadOnly] public NativeArray<SteeringControllerJobData> job_steeringControllerData;
        [ReadOnly] public float3 job_evadeTargetPos;
        [ReadOnly] public float3 job_evadeTargetVel;

        public NativeArray<SteeringBehaviorInfo> rv_steering;

        private SteeringBehaviorInfo steering;

        float3 direction;
        float distance;
        float speed;
        float prediction;
        float3 predictedPos;
        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; i++)
            {

                direction = job_evadeTargetPos - job_boidData[i].position;
                distance = math.length(direction);
                speed = math.length(job_boidData[i].velocity);

                if(distance <= job_steeringControllerData[i].evadeData.evade_maxPrediction)
                {
                    //if (speed <= (distance / job_maxPrediction[i]))
                    //    prediction = job_maxPrediction[i];
                    //else
                    //    prediction = distance / speed;
                    //������Ҫ��ȡTarget�ĵ�ǰ�ƶ����� Ҳ����Velocity,����ͨ��target��Rigibody��ȡ����ʹ��������������
                    //��ʱʹ��Vector3.up �����������

                    //predictedPos = job_evadeTargetPos + job_evadeTargetVel * prediction;
                    steering.linear = job_boidData[i].position - job_evadeTargetPos;
                    steering.linear = math.normalize(steering.linear);
                    steering.linear = steering.linear * job_steeringControllerData[i].maxAcceleration;
                    steering.angular = 0;

                    rv_steering[i] = steering;
                }
                else
                {
                    steering.linear = float3.zero;
                    steering.angular = 0;
                    rv_steering[i] = steering;
                }


            }
        }
    }

    public struct EvadeBehaviorJob_OneOnOne : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<BoidJobData> job_boidData;
        [ReadOnly] public NativeArray<SteeringControllerJobData> job_steeringControllerData;
        [ReadOnly] public NativeArray<BoidJobData> job_evadeTargetBoidData;


        public NativeArray<SteeringBehaviorInfo> rv_steering;

        private SteeringBehaviorInfo steering;

        float3 direction;
        float distance;
        float speed;
        float prediction;
        float3 predictedPos;
        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; i++)
            {

                direction = job_evadeTargetBoidData[i].position - job_boidData[i].position;
                distance = math.length(direction);
                speed = math.length(job_boidData[i].velocity);

                if (distance <= job_steeringControllerData[i].evadeData.evade_maxPrediction)
                {
                    //if (speed <= (distance / job_maxPrediction[i]))
                    //    prediction = job_maxPrediction[i];
                    //else
                    //    prediction = distance / speed;
                    //������Ҫ��ȡTarget�ĵ�ǰ�ƶ����� Ҳ����Velocity,����ͨ��target��Rigibody��ȡ����ʹ��������������
                    //��ʱʹ��Vector3.up �����������

                    //predictedPos = job_evadeTargetPos + job_evadeTargetVel * prediction;
                    steering.linear = job_boidData[i].position - job_evadeTargetBoidData[i].position;
                    steering.linear = math.normalize(steering.linear);
                    steering.linear = steering.linear * job_steeringControllerData[i].maxAcceleration;
                    steering.angular = 0;

                    rv_steering[i] = steering;
                }
                else
                {
                    steering.linear = float3.zero;
                    steering.angular = 0;
                    rv_steering[i] = steering;
                }


            }
        }
    }

    //public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    //{
    //    SteeringData steering = new SteeringData();
    //    Vector3 direction = target.position - transform.position;
    //    float distance = direction.magnitude;
    //    float speed = GetComponent<IBoid>().GetVelocity().magnitude;
    //    float prediction;
    //    if (speed <= (distance / maxPrediction)) prediction = maxPrediction;
    //    else prediction = distance / speed;
    //    //������Ҫ��ȡTarget�ĵ�ǰ�ƶ����� Ҳ����Velocity,����ͨ��target��Rigibody��ȡ����ʹ��������������
    //    //��ʱʹ��Vector3.up �����������

    //    Vector3 predictedTarget = target.position + (new Vector3(target.GetComponent<IBoid>().GetVelocity().x, target.GetComponent<IBoid>().GetVelocity().y, 0) * prediction);
    //    steering.linear = transform.position - predictedTarget ;
    //    steering.linear.Normalize();
    //    steering.linear *= steeringcontroller.maxAcceleration;
    //    steering.angular = 0;
    //    return steering;
    //}

}
