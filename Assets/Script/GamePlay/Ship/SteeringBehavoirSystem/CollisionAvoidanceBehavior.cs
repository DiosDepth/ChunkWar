using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class CollisionAvoidanceBehavior : SteeringBehavior
{
    [SerializeField] private float detacteRadius = 5f;
    private IBoid selfboid;


    private void Start()
    {
        selfboid = GetComponent<IBoid>();
    }



    [BurstCompile]
    public struct CollisionAvoidanceBehaviorJob : IJobParallelFor
    {
        [ReadOnly] public float job_detacteRadius;
        [ReadOnly] public NativeArray<float3> job_targetsPos;
        [ReadOnly] public NativeArray<float3> job_targetsVel;
        [ReadOnly] public NativeArray<float> job_targetsRadius;
        [ReadOnly] public float3 job_selfPos;
        [ReadOnly] public float3 job_selfVel;
        [ReadOnly] public float job_selfRadius;


        //0 = shortestTime, 1 = firstMinSeparation, 2 =firstDistance, 3=firstRadius
        public NativeArray<float> JRD_float;
        //0 = firstTargetPos, 1 = firstRelativePos, 2 = firstRelativeVel
        public NativeArray<float3> JRD_float3;

        public NativeArray<bool> JRD_havefirsttarget;


        public void Execute(int index)
        {
            if (math.distance(job_targetsPos[index], job_selfPos) > 0.0001f &&
                 math.distance(job_targetsPos[index], job_selfPos) <= job_detacteRadius)
            {
                float3 relativePos = job_selfPos - job_targetsPos[index];
                float3 relativeVel = job_selfVel - job_targetsVel[index];

                float distance = math.length(relativePos);
                float relativeSpeed = math.length(relativeVel);

                if(relativeSpeed == 0)
                {
                    return;
                }

                float timeToCollision = -1 * math.dot(relativePos, relativeVel) / (relativeSpeed * relativeSpeed);
                float3 separation = relativePos + relativeVel * timeToCollision;
                float minSeparation = math.length(separation);

                if(minSeparation > job_selfRadius + job_targetsRadius[index])
                {
                    return;
                }

                if ((timeToCollision > 0) && (timeToCollision < JRD_float[0]))
                {
                    JRD_float[0] = timeToCollision;
                    JRD_havefirsttarget[0] = true;
                    JRD_float[1] = minSeparation;
                    JRD_float[2] = distance;
                    JRD_float[3] = job_targetsRadius[index];
                    JRD_float3[0] = job_targetsPos[index];
                    JRD_float3[1] = relativePos;
                    JRD_float3[2] = relativeVel;
                }
            }
        }
    }


    public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    {
        SteeringData steering = new SteeringData();

        ///
        ///  0 = shortestTime, 1 = firstMinSeparation, 2 =firstDistance, 3=firstRadius
        ///
        NativeArray<float> JRD_float = new NativeArray<float>(4, Allocator.TempJob);
     
        JRD_float[0] = float.PositiveInfinity;
        JRD_float[1] = 0;
        JRD_float[2] = 0;
        JRD_float[3] = 0;

        NativeArray<bool> JRD_havefirsttarget = new NativeArray<bool>(1, Allocator.TempJob);
        JRD_havefirsttarget[0] = false;

        ///
        ///0 = firstTargetPos, 1 = firstRelativePos, 2 = firstRelativeVel
        ///
        NativeArray<float3> JRD_float3 = new NativeArray<float3>(3, Allocator.TempJob);
        JRD_float3[0] = Vector3.zero;
        JRD_float3[1] = Vector3.zero;
        JRD_float3[2] = Vector3.zero;


        //start Job
        CollisionAvoidanceBehaviorJob collisionAvoidanceBehaviorJob = new CollisionAvoidanceBehaviorJob
        {
            job_detacteRadius = detacteRadius,
            job_selfPos = transform.position,
            job_selfRadius = selfboid.GetRadius(),
            job_selfVel = selfboid.GetVelocity(),

            job_targetsPos = AIManager.Instance.steeringBehaviorJob_aiShipPos,
            job_targetsRadius = AIManager.Instance.steeringBehaviorJob_aiShipRadius,
            job_targetsVel = AIManager.Instance.steeringBehaviorJob_aiShipVelocity,

            JRD_float = JRD_float,
            JRD_float3 = JRD_float3,
            JRD_havefirsttarget = JRD_havefirsttarget,

        };

        JobHandle jobHandle = collisionAvoidanceBehaviorJob.Schedule(AIManager.Instance.ShipCount, 20);

        jobHandle.Complete();


        if (!JRD_havefirsttarget[0])
        {
            JRD_float.Dispose();
            JRD_float3.Dispose();
            JRD_havefirsttarget.Dispose();
            return steering;
        }
               

        if (JRD_float[1] <= 0 || JRD_float[2] < selfboid.GetRadius() + JRD_float[3])
        {
            steering.linear = transform.position - new Vector3(JRD_float3[0].x, JRD_float3[0].y, JRD_float3[0].z);
        }
        else
        {
            steering.linear = JRD_float3[1] + JRD_float3[2] * JRD_float[0];
        }
               
        steering.linear.Normalize();
        steering.linear *= steeringcontroller.maxAcceleration;

        JRD_float.Dispose();
        JRD_float3.Dispose();
        JRD_havefirsttarget.Dispose();
        return steering;
    }


}


//     SteeringData steering = new SteeringData();
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