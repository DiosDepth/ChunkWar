using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[System.Serializable]
public class CollisionAvoidanceBehavior : SteeringBehavior
{
    //[SerializeField] private float detacteRadius = 5f;

    [BurstCompile]
    public struct CollisionAvoidanceBehaviorJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<BoidJobData> job_boidData;
        [ReadOnly] public NativeArray<SteeringControllerJobData> job_steeringControllerData;

        [ReadOnly] public NativeArray<BoidJobData> job_avoidenceData;
   

        public NativeArray<SteeringBehaviorInfo> rv_steering;
        private SteeringBehaviorInfo steering;

        private float3 relativePos;
        private float3 relativeVel;
        private float relativeSpeed;
        private float distance;
        private float timeToCollision;
        private float3 separation;
        private float minSeparation;
        private float shortestTime;

        //运算之后的临时值
        private float3 firstTargetPos;
        private float firstMinSeparation;
        private float firstDistance;
        private float3 firstRelativePos;
        private float3 firstRelativeVel;
        private float firstRadius;

        public void Execute(int startIndex, int count)
        {
            
            for (int i = startIndex; i < startIndex + count; i++)
            {
                shortestTime = float.PositiveInfinity;
                for (int n = 0; n < job_avoidenceData.Length; n++)
                {

                    relativePos = job_boidData[i].position - job_avoidenceData[n].position;
                    relativeVel = job_boidData[i].velocity - job_avoidenceData[n].position;
                    relativeSpeed = math.length(relativeVel);
                    distance = math.length(relativePos);

                    if (relativeSpeed == 0)
                        continue;

                    timeToCollision = -1 * math.dot(relativePos, relativeVel) / (relativeSpeed * relativeSpeed);


                    separation = relativePos + relativeVel * timeToCollision;
                    minSeparation = math.length(separation);

                    if (minSeparation > job_boidData[i].boidRadius + job_avoidenceData[n].boidRadius)
                        continue;

                    if ((timeToCollision > 0) && (timeToCollision < shortestTime))
                    {
                        shortestTime = timeToCollision;
                        firstTargetPos = job_avoidenceData[n].position;
                        firstMinSeparation = minSeparation;
                        firstDistance = distance;
                        firstRelativePos = relativePos;
                        firstRelativeVel = relativeVel;
                        firstRadius = job_avoidenceData[n].boidRadius;
                    }
                }

                if (shortestTime == float.PositiveInfinity)
                {
                    steering.linear = float3.zero;
                    steering.angular = 0;
                    rv_steering[i] = steering;
                }
                else
                {
                    if (firstMinSeparation <= 0 || firstDistance < job_boidData[i].boidRadius + firstRadius)
                        steering.linear = job_boidData[i].position - firstTargetPos;
                    else
                        steering.linear = firstRelativePos + firstRelativeVel * shortestTime;

                    steering.linear = math.normalize(steering.linear);
                    steering.linear *= steering.linear * job_steeringControllerData[i].maxAcceleration;

                    rv_steering[i] = steering;
                }
            }
        }
    }


    //public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    //{
    //    SteeringData steering = new SteeringData();

    //    ///
    //    ///  0 = shortestTime, 1 = firstMinSeparation, 2 =firstDistance, 3=firstRadius
    //    ///
    //    NativeArray<float> JRD_float = new NativeArray<float>(4, Allocator.TempJob);
     
    //    JRD_float[0] = float.PositiveInfinity;
    //    JRD_float[1] = 0;
    //    JRD_float[2] = 0;
    //    JRD_float[3] = 0;

    //    NativeArray<bool> JRD_havefirsttarget = new NativeArray<bool>(1, Allocator.TempJob);
    //    JRD_havefirsttarget[0] = false;

    //    ///
    //    ///0 = firstTargetPos, 1 = firstRelativePos, 2 = firstRelativeVel
    //    ///
    //    NativeArray<float3> JRD_float3 = new NativeArray<float3>(3, Allocator.TempJob);
    //    JRD_float3[0] = Vector3.zero;
    //    JRD_float3[1] = Vector3.zero;
    //    JRD_float3[2] = Vector3.zero;


    //    //start Job
    //    CollisionAvoidanceBehaviorJob collisionAvoidanceBehaviorJob = new CollisionAvoidanceBehaviorJob
    //    {
    //        job_detacteRadius = detacteRadius,
    //        job_selfPos = transform.position,
    //        job_selfRadius = selfboid.GetRadius(),
    //        job_selfVel = selfboid.GetVelocity(),

    //        job_targetsPos = AIManager.Instance.steeringBehaviorJob_aiShipPos,
    //        job_targetsRadius = AIManager.Instance.steeringBehaviorJob_aiShipRadius,
    //        job_targetsVel = AIManager.Instance.steeringBehaviorJob_aiShipVelocity,

    //        JRD_float = JRD_float,
    //        JRD_float3 = JRD_float3,
    //        JRD_havefirsttarget = JRD_havefirsttarget,

    //    };

    //    JobHandle jobHandle = collisionAvoidanceBehaviorJob.Schedule(AIManager.Instance.ShipCount, 20);

    //    jobHandle.Complete();


    //    if (!JRD_havefirsttarget[0])
    //    {
    //        JRD_float.Dispose();
    //        JRD_float3.Dispose();
    //        JRD_havefirsttarget.Dispose();
    //        return steering;
    //    }
               

    //    if (JRD_float[1] <= 0 || JRD_float[2] < selfboid.GetRadius() + JRD_float[3])
    //    {
    //        steering.linear = transform.position - new Vector3(JRD_float3[0].x, JRD_float3[0].y, JRD_float3[0].z);
    //    }
    //    else
    //    {
    //        steering.linear = JRD_float3[1] + JRD_float3[2] * JRD_float[0];
    //    }
               
    //    steering.linear.Normalize();
    //    steering.linear *= steeringcontroller.maxAcceleration;

    //    JRD_float.Dispose();
    //    JRD_float3.Dispose();
    //    JRD_havefirsttarget.Dispose();
    //    return steering;
    //}


}


//SteeringData steering = new SteeringData();
//float shortestTime = float.PositiveInfinity;
//Transform firstTarget = null;
//float firstMinSeparation = 0;
//float firstDistance = 0;
//float firstRadius = 0;
//Vector3 firstRelativePos = Vector3.zero;
//Vector3 firstRelativeVel = Vector3.zero;


//RaycastHit2D[] _Hits;
//_Hits = Physics2D.CircleCastAll(transform.position, detacteRadius, Vector2.zero, 0f, mask);

       
//        if (_Hits.Length >0)
//        {
  
//            //获取所有的周边iBoid

//            for (int i = 0; i<_Hits.Length; i++)
//            {
//                if (_Hits[i].transform.gameObject == gameObject)
//                {
//                    continue;
//                }

//                Vector3 relativePos = transform.position - _Hits[i].transform.position;
//Vector3 relativeVel = GetComponent<IBoid>().GetVelocity() - _Hits[i].transform.GetComponent<IBoid>().GetVelocity();
//float distance = relativePos.magnitude;
//float relativeSpeed = relativeVel.magnitude;

//                if (relativeSpeed == 0)
//                    continue;

//                float timeToCollision = -1 * Vector3.Dot(relativePos, relativeVel) / (relativeSpeed * relativeSpeed);

//Vector3 separation = relativePos + relativeVel * timeToCollision;
//float minSeparation = separation.magnitude;

//                if (minSeparation > GetComponent<IBoid>().GetRadius() + _Hits[i].transform.GetComponent<IBoid>().GetRadius())
//                    continue;

//                if ((timeToCollision > 0) && (timeToCollision<shortestTime))
//                {
//                    shortestTime = timeToCollision;
//                    firstTarget = _Hits[i].transform;
//                    firstMinSeparation = minSeparation;
//                    firstDistance = distance;
//                    firstRelativePos = relativePos;
//                    firstRelativeVel = relativeVel;
//                    firstRadius = _Hits[i].transform.GetComponent<IBoid>().GetRadius();
//                }
//            }
//        }
//        if (firstTarget == null)
//            return steering;

//        if (firstMinSeparation <= 0 || firstDistance<GetComponent<IBoid>().GetRadius() + firstRadius)
//            steering.linear = transform.position - firstTarget.position;
//        else
//            steering.linear = firstRelativePos + firstRelativeVel* shortestTime;

//steering.linear.Normalize();
//        steering.linear *= steeringcontroller.maxAcceleration;

//        return steering;