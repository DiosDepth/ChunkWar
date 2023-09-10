using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

[System.Serializable]
public class CohesionBehavior : SteeringBehavior
{
    [SerializeField] public float viewAngle = 60f;


    [BurstCompile]

    public struct CohesionBehaviorJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<float> job_viewAngle;
        [ReadOnly] public int job_shipcount;
        [ReadOnly] public NativeArray<float3> job_aiShipPos;
        [ReadOnly] public NativeArray<float3> job_aiShipVel;

       
        [ReadOnly] public NativeArray<float3> job_aiShipPosInRange;

        [ReadOnly] public NativeArray<int> job_InRangeLength;

        [ReadOnly] public NativeArray<float> job_maxAcceleration;

        public NativeArray<SteeringBehaviorInfo> rv_Steerings;

        private SteeringBehaviorInfo steering;
 
        private float3 targetdir;
        private float3 centerOfMass;
        private int countindex;
        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; i++)
            {

                countindex = 0;
                centerOfMass = float3.zero;
                for (int n = 0; n < job_InRangeLength[i]; n++)
                {

                    targetdir = job_aiShipPosInRange[i * job_shipcount + n] - job_aiShipPos[i];
                    if( math.degrees( math.acos( math.dot(math.normalize(targetdir), math.normalize(job_aiShipVel[i])))) < job_viewAngle[i])
                    {
                        centerOfMass += job_aiShipPosInRange[i * job_shipcount + n]; ;
                        countindex++;
                    }
                }

                if(countindex > 0)
                {
                    centerOfMass = centerOfMass / countindex;
                    steering.linear = centerOfMass - job_aiShipPos[i];
                    steering.linear = math.normalize(steering.linear);
                    steering.linear *= job_maxAcceleration[i];
                }

                rv_Steerings[i] = steering;

            }
        }
    }



    //public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    //{
    //    SteeringData steering = new SteeringData();
    //    Vector3 centerOfMass = Vector3.zero; int count = 0;


    //    RaycastHit2D[] _Hits;
    //    _Hits = Physics2D.CircleCastAll(transform.position, cohesionRadius, Vector2.zero, 0f, mask);


    //    if (_Hits.Length > 0)
    //    {


    //        for (int i = 0; i < _Hits.Length; i++)
    //        {
    //            if (_Hits[i].transform.gameObject == gameObject)
    //            {
    //                continue;
    //            }
    //            Vector3 targetDir = _Hits[i].transform.position - transform.position;
    //            if (Vector3.Angle(targetDir, transform.forward) < viewAngle)
    //            {
    //                centerOfMass += _Hits[i].transform.transform.position;
    //                count++;
    //            }

    //        }
    //    }



    //    if (count > 0)
    //    {
    //        centerOfMass = centerOfMass / count;
    //        steering.linear = centerOfMass - transform.position;
    //        steering.linear.Normalize();
    //        steering.linear *= steeringcontroller.maxAcceleration;
    //    }
    //    return steering;
    //}
}
