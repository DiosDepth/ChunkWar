using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;


public class AlignmentBehavior : SteeringBehavior
{
    [SerializeField] private float alignDistance = 8f;
    [SerializeField] private float detacteRadius = 5f;
    private IBoid selfboid;
    void Start()
    {
        selfboid = GetComponent<IBoid>();
    }

    public struct AligmentBehaviorJob : IJob
    {
       
        [ReadOnly] public float job_alignDistance;
        [ReadOnly] public float job_detacteRadius;
        [ReadOnly] public float job_maxAcceleration;
        [ReadOnly] public NativeArray<float3> job_targetsPos;
        [ReadOnly] public NativeArray<float3> job_targetsVel;
        [ReadOnly] public NativeArray<float> job_targetsRadius;
        [ReadOnly] public float3 job_selfPos;
        [ReadOnly] public float3 job_selfVel;
        [ReadOnly] public float job_selfRadius;
        

        public NativeArray<int> JRD_count;
        public NativeArray<float3> JRD_linear;
        public void Execute()
        {

            for (int i = 0; i < job_targetsPos.Length; i++)
            {
                if (math.distance(job_targetsPos[i], job_selfPos) > 0.0001f &&
                    math.distance(job_targetsPos[i], job_selfPos) <= job_detacteRadius)
                {

                    float3 targetDir = job_targetsPos[i] - job_selfPos;
                    if (math.length(targetDir) < job_alignDistance)
                    {
                        //如果在范围内，则加权他们的velocity
                        JRD_linear[0] += job_targetsVel[i];
                        JRD_count[0]++;
                    }
                }
            }

            if(JRD_count[0] >0)
            {
                //全部加权完成后， 取加权后的steering量级平均值，加上最大值的限制 就是列队情况下的steering值
                JRD_linear[0] = JRD_linear[0] / JRD_count[0];
                if (math.length(JRD_linear[0] )> job_maxAcceleration)
                {
                    JRD_linear[0] = math.normalize(JRD_linear[0]) * job_maxAcceleration;
                }
            }

        }
    }
    




    public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    {
        SteeringData steering = new SteeringData();
        NativeArray<int> count = new NativeArray<int>(1, Allocator.Temp);
        NativeArray<float3> linear = new NativeArray<float3>(1, Allocator.Temp);



        return steering;
    }

}

//steering.linear = Vector3.zero;
//        int count = 0;
//        for (int i = 0; i<LevelManager.Instance.aiShipList.Count; i++)
//        {
//            if (LevelManager.Instance.aiShipList[i].gameObject == gameObject)
//            {
//                continue;
//            }
//            //检测其他agent是否在周边列队范围内，
//            Vector3 targetDir = LevelManager.Instance.aiShipList[i].transform.position - transform.position;
//_boidagent = LevelManager.Instance.aiShipList[i].GetComponent<IBoid>();
//            if (targetDir.magnitude<alignDistance)
//            {
//                //如果在范围内，则加权他们的velocity
//                steering.linear += new Vector3(_boidagent.GetVelocity().x, _boidagent.GetVelocity().y, 0);
//count++;
//            }
//        }
//        if (count > 0)
//        {
//            //全部加权完成后， 取加权后的steering量级平均值，加上最大值的限制 就是列队情况下的steering值
//            steering.linear = steering.linear / count;
//            if (steering.linear.magnitude > steeringcontroller.maxAcceleration)
//            {
//                steering.linear = steering.linear.normalized* steeringcontroller.maxAcceleration;
//            }
//        }