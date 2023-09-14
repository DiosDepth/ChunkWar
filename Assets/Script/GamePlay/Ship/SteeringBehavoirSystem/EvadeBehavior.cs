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
        [ReadOnly] public NativeArray<float3> job_aiShipPos;
        [ReadOnly] public NativeArray<float3> job_aiShipVel;
        [ReadOnly] public NativeArray<float> job_maxPrediction;
        [ReadOnly] public NativeArray<float> job_maxAcceleration;
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

                direction = job_evadeTargetPos - job_aiShipPos[i];
                distance = math.length(direction);
                speed = math.length(job_aiShipVel[i]);

                if(distance <= job_maxPrediction[i])
                {
                    //if (speed <= (distance / job_maxPrediction[i]))
                    //    prediction = job_maxPrediction[i];
                    //else
                    //    prediction = distance / speed;
                    //这里需要获取Target的当前移动速率 也就是Velocity,可以通过target的Rigibody获取或者使用其他方法计算
                    //暂时使用Vector3.up 方向替代计算

                    //predictedPos = job_evadeTargetPos + job_evadeTargetVel * prediction;
                    steering.linear = job_aiShipPos[i] - job_evadeTargetPos;
                    steering.linear = math.normalize(steering.linear);
                    steering.linear = steering.linear * job_maxAcceleration[i];
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
    //    //这里需要获取Target的当前移动速率 也就是Velocity,可以通过target的Rigibody获取或者使用其他方法计算
    //    //暂时使用Vector3.up 方向替代计算

    //    Vector3 predictedTarget = target.position + (new Vector3(target.GetComponent<IBoid>().GetVelocity().x, target.GetComponent<IBoid>().GetVelocity().y, 0) * prediction);
    //    steering.linear = transform.position - predictedTarget ;
    //    steering.linear.Normalize();
    //    steering.linear *= steeringcontroller.maxAcceleration;
    //    steering.angular = 0;
    //    return steering;
    //}

}
