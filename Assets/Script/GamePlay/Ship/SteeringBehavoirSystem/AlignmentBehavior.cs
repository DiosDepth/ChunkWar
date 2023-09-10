using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;


[System.Serializable]
public class AlignmentBehavior : SteeringBehavior
{
    [SerializeField] public float alignDistance = 8f;



    //    public struct AligmentBehaviorJob : IJob
    //    {

    //        [ReadOnly] public float job_alignDistance;
    //        [ReadOnly] public float job_detacteRadius;
    //        [ReadOnly] public float job_maxAcceleration;
    //        [ReadOnly] public NativeArray<float3> job_targetsPos;
    //        [ReadOnly] public NativeArray<float3> job_targetsVel;
    //        [ReadOnly] public NativeArray<float> job_targetsRadius;
    //        [ReadOnly] public float3 job_selfPos;
    //        [ReadOnly] public float3 job_selfVel;
    //        [ReadOnly] public float job_selfRadius;


    //        public NativeArray<int> JRD_count;
    //        public NativeArray<float3> JRD_linear;
    //        public void Execute()
    //        {

    //            for (int i = 0; i < job_targetsPos.Length; i++)
    //            {
    //                if (math.distance(job_targetsPos[i], job_selfPos) > 0.0001f &&
    //                    math.distance(job_targetsPos[i], job_selfPos) <= job_detacteRadius)
    //                {

    //                    float3 targetDir = job_targetsPos[i] - job_selfPos;
    //                    if (math.length(targetDir) < job_alignDistance)
    //                    {
    //                        //����ڷ�Χ�ڣ����Ȩ���ǵ�velocity
    //                        JRD_linear[0] += job_targetsVel[i];
    //                        JRD_count[0]++;
    //                    }
    //                }
    //            }

    //            if (JRD_count[0] > 0)
    //            {
    //                //ȫ����Ȩ��ɺ� ȡ��Ȩ���steering����ƽ��ֵ���������ֵ������ �����ж�����µ�steeringֵ
    //                JRD_linear[0] = JRD_linear[0] / JRD_count[0];
    //                if (math.length(JRD_linear[0]) > job_maxAcceleration)
    //                {
    //                    JRD_linear[0] = math.normalize(JRD_linear[0]) * job_maxAcceleration;
    //                }
    //            }

    //        }
    //    }
    //}


    [BurstCompile]
    public struct AlignmentBehaviorJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<float> job_alignDistance;
        [ReadOnly] public NativeArray<float> job_maxAcceleration;
        [ReadOnly] public NativeArray<float3> job_aiShipPos;
        [ReadOnly] public int job_shipcount;



        [ReadOnly] public NativeArray<float3> job_aiShipPosInRange;
        [ReadOnly] public NativeArray<float3> job_aiShipVelInRange;
        [ReadOnly] public NativeArray<int> job_lengthInRange;


        public NativeArray<SteeringBehaviorInfo> rv_Steerings;
        private SteeringBehaviorInfo steering;

        private int countindex;
        private float3 direction;
        //        [ReadOnly] public NativeArray<float3> job_targetsVel;
        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; i++)
            {
                countindex = 0;
                steering.linear = float3.zero;
                for (int n = 0; n < job_lengthInRange[i]; n++)
                {
                    direction = job_aiShipPosInRange[i * job_shipcount + n] - job_aiShipPos[i];
                    if( math.length(direction) <= job_alignDistance[i])
                    {
                        steering.linear += job_aiShipVelInRange[i * job_shipcount + n];
                        countindex++;
                    }
                }
                if(countindex > 0)
                {
                    steering.linear = steering.linear / countindex;
                    if (math.length(steering.linear) > job_maxAcceleration[i])
                    {
                        steering.linear = math.normalize(steering.linear) * job_maxAcceleration[i];
                    }
                }

                rv_Steerings[i] = steering;
            }
        }
    }
}



//    public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
//    {
//        SteeringData steering = new SteeringData();
//        NativeArray<int> count = new NativeArray<int>(1, Allocator.Temp);
//        NativeArray<float3> linear = new NativeArray<float3>(1, Allocator.Temp);



//        return steering;
//    }

//}
//steering.linear = Vector3.zero;
//        int count = 0;
//        for (int i = 0; i<LevelManager.Instance.aiShipList.Count; i++)
//        {
//            if (LevelManager.Instance.aiShipList[i].gameObject == gameObject)
//            {
//                continue;
//            }
//            //�������agent�Ƿ����ܱ��жӷ�Χ�ڣ�
//            Vector3 targetDir = LevelManager.Instance.aiShipList[i].transform.position - transform.position;
//_boidagent = LevelManager.Instance.aiShipList[i].GetComponent<IBoid>();
//            if (targetDir.magnitude<alignDistance)
//            {
//                //����ڷ�Χ�ڣ����Ȩ���ǵ�velocity
//                steering.linear += new Vector3(_boidagent.GetVelocity().x, _boidagent.GetVelocity().y, 0);
//count++;
//            }
//        }
//        if (count > 0)
//        {
//            //ȫ����Ȩ��ɺ� ȡ��Ȩ���steering����ƽ��ֵ���������ֵ������ �����ж�����µ�steeringֵ
//            steering.linear = steering.linear / count;
//            if (steering.linear.magnitude > steeringcontroller.maxAcceleration)
//            {
//                steering.linear = steering.linear.normalized* steeringcontroller.maxAcceleration;
//            }
//        }