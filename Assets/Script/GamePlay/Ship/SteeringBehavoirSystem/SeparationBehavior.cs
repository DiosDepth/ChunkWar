using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;

public class SeparationBehavior : SteeringBehavior
{
   
    [SerializeField] private float threshold = 2f;
    [SerializeField] private float decayCoefficient = -25f;
    [SerializeField] private float detacteRadius = 3f;
    [SerializeField] public LayerMask mask = 1 << 9;


    

    [BurstCompile]
    public struct SeparationBehaviorJob : IJobParallelFor
    {
        [ReadOnly] public float job_maxAcceleration;
        [ReadOnly] public float job_threshold;
        [ReadOnly] public float job_decayCoefficient;
        [ReadOnly] public float job_detacteRadius;
        [ReadOnly] public float3 job_selfpos;
        [ReadOnly] public NativeArray <float3> job_targetspos;
        public NativeArray<float3> job_linear ;
        public void Execute(int index)
        {
            if(math.distance(job_targetspos[index], job_selfpos) > 0.0001f &&
               math.distance(job_targetspos[index], job_selfpos) <= job_detacteRadius)
            {

                //获得从自己到其他agent的方向
                float3 direction = job_targetspos[index] - job_selfpos;
                float distance = math.length(direction);
                //如果方向的长度< threshold 就开始降低对应的steering强度
                if (distance < job_threshold)
                {
                    //通过距离指数进行衰变， 衰变系数通常为负数，也就是会翻转最后的方向
                    //吧所有周边的反转方向合并起来，获取最后的方向
                    float strength = math.min(job_decayCoefficient / (distance * distance), job_maxAcceleration);
                    direction = math.normalize(direction);

                    job_linear[0] += strength * direction;
                }
            }

        }
    }

    public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    {
        SteeringData steering = new SteeringData();

        NativeArray<float3> templiner = new NativeArray<float3>(1, Allocator.TempJob);
        SeparationBehaviorJob separationBehaviorJob = new SeparationBehaviorJob
        {
            job_maxAcceleration = steeringcontroller.maxAcceleration,
            job_threshold = threshold,
            job_decayCoefficient = decayCoefficient,
            job_detacteRadius = detacteRadius,
            job_selfpos = transform.position,
            job_targetspos = LevelManager.Instance.steeringBehaviorJob_aiShipPos,
            job_linear = templiner
        };

        JobHandle jobHandle = separationBehaviorJob.Schedule(LevelManager.Instance.steeringBehaviorJob_aiShipPos.Length, 20);
        jobHandle.Complete();
        steering.linear = templiner[0];
        templiner.Dispose();
        return steering;
    }

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
//    direction.Normalize();
//                steering.linear += strength* direction;
//}
//        }

//        return steering;
}
