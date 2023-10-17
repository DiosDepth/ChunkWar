using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;

[System.Serializable]
public class SeparationBehavior : SteeringBehavior
{
   
    [SerializeField] public float threshold = 2f;
    [SerializeField] public float decayCoefficient = -25f;


    [BurstCompile]
    public struct SeparationBehaviorJob : IJobParallelForBatch
    {
        [ReadOnly] public int job_shipcount;
        [ReadOnly] public NativeArray<BoidJobData> job_boidData;
        [ReadOnly] public NativeArray<SteeringControllerJobData> job_steeringControllerData;
        [ReadOnly] public NativeArray<float3> job_searchingTargetPosFlatArray;
        [ReadOnly] public NativeArray<int> job_searchingTargetCount;



        public NativeArray<SteeringBehaviorInfo> rv_Steerings;

        private SteeringBehaviorInfo steering;
        float3 direction;
        float distance;
        float strength;
        public void Execute(int startIndex, int count)
        {
  
            for (int i = startIndex; i < startIndex + count; i++)
            {
                steering.linear = float3.zero;
                for (int n = 0; n < job_searchingTargetCount[i]; n++)
                {
                    direction = job_searchingTargetPosFlatArray[i * job_shipcount + n] - job_boidData[i].position;
                    distance = math.length(direction);

                    if(distance < job_steeringControllerData[i].separation_threshold)
                    {
                        strength = math.min(job_steeringControllerData[i].separation_decayCoefficient / (distance * distance), job_steeringControllerData[i].maxAcceleration);
                        steering.linear += strength * math.normalize(direction);
                    }
                }
                rv_Steerings[i] = steering;
            }
        }
    }
}


    //public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    //{
    //    SteeringData steering = new SteeringData();

    //    NativeArray<float3> templiner = new NativeArray<float3>(1, Allocator.TempJob);
    //    SeparationBehaviorJob separationBehaviorJob = new SeparationBehaviorJob
    //    {
    //        job_maxAcceleration = steeringcontroller.maxAcceleration,
    //        job_threshold = threshold,
    //        job_decayCoefficient = decayCoefficient,
    //        job_detacteRadius = detacteRadius,
    //        job_selfpos = transform.position,
    //        job_targetspos = AIManager.Instance.steeringBehaviorJob_aiShipPos,
    //        job_linear = templiner
    //    };

    //    JobHandle jobHandle = separationBehaviorJob.Schedule(AIManager.Instance.steeringBehaviorJob_aiShipPos.Length, 20);
    //    jobHandle.Complete();
    //    steering.linear = templiner[0];
    //    templiner.Dispose();
    //    return steering;
    //}

    //    SteeringData steering = new SteeringData();

    //        for (int i = 0; i<LevelManager.Instance.aiShipList.Count; i++)
    //        {
    //            if (LevelManager.Instance.aiShipList[i].gameObject == gameObject)
    //            {
    //                continue;
    //            }
    //            //获得从自己到其他agent的方向
    //            Vector3 direction = LevelManager.Instance.aiShipList[i].transform.position - transform.position;
    //    float distance = direction.magnitude;
    //            //如果方向的长度< threshold 就开始降低对应的steering强度
    //            if (distance<threshold)
    //            {
    //                //通过距离指数进行衰变， 衰变系数通常为负数，也就是会翻转最后的方向
    //                //吧所有周边的反转方向合并起来，获取最后的方向
    //                float strength = Mathf.Min(decayCoefficient / (distance * distance), steeringcontroller.maxAcceleration);
    //                  direction.Normalize();
    //                steering.linear += strength* direction;
    //}
    //        }

    //        return steering;

