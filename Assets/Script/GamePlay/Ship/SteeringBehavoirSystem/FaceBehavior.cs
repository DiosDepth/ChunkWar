using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;

[System.Serializable]
public class FaceBehavior : SteeringBehavior
{
    //ȷ��ʲôʱ��ʼ����target
    public float facetargetRadius;

    //[BurstCompile(Debug = true)]
    public struct FaceBehaviorJob :IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<float3> job_aiShipPos;
        [ReadOnly] public NativeArray<float3> job_aiShipVel;
        [ReadOnly] public NativeArray<float> job_aiShipRotationZ;
        [ReadOnly] public NativeArray<float> job_facetagetRadius;
        [ReadOnly] public float3 job_targetPos;
        [ReadOnly] public NativeArray<float> job_maxAngularAcceleration;
        [ReadOnly] public float job_deltatime;

        public NativeArray<SteeringBehaviorInfo> rv_Steerings;


        private SteeringBehaviorInfo steering;


        float fromangle;
        float targetangle;
        float resultangle;
        float diff;
        public void Execute(int startIndex, int count)
        {

            for (int i = startIndex;  i < startIndex + count; i++)
            {

                //if (math.distance(job_aiShipPos[i], job_targetPos) <= job_facetagetRadius[i])
                //{
                //    angle = math.degrees(math.atan2(job_targetPos.y - job_aiShipPos[i].y, job_targetPos.x - job_aiShipPos[i].x)) - 90;
                //}
                //else
                //{

                //    direction = math.normalize(job_aiShipVel[i]);
                //    angle = math.degrees(math.atan2(job_aiShipVel[i].y, job_aiShipVel[i].x)) - 90;
                //}

                targetangle = math.degrees(math.atan2(job_targetPos.y - job_aiShipPos[i].y, job_targetPos.x - job_aiShipPos[i].x)) - 90;
                //steering.angular = targetangle;

                //to 360
                if (targetangle < 0)
                {
                    targetangle = 360 + targetangle;
                }


                fromangle = job_aiShipRotationZ[i];

                //to 360
                if (fromangle < 0)
                {
                    fromangle = 360 + fromangle;
                }

                diff = (targetangle -fromangle) % 360;
                diff = (2 * diff) % 360 - diff;
                //get diff between tareget and from angle
                diff = math.lerp(0, diff, math.clamp(job_maxAngularAcceleration[i] * job_deltatime, 0, 1));

                resultangle = (fromangle + diff) % 360;
                
                steering.angular = resultangle;

                rv_Steerings[i] = steering;
            }
        }
    }

    //public override SteeringData GetSteering(AISteeringBehaviorController steeringcontroller)
    //{
    //    SteeringData steering = new SteeringData();
    //    Vector3 velocity = GetComponent<IBoid>().GetVelocity();
    //    float angle;
    //    if (velocity.magnitude == 0)
    //    {
    //        angle = (Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x) * Mathf.Rad2Deg) - 90;
    //        steering.angular = Mathf.LerpAngle(transform.rotation.eulerAngles.z, angle, steeringcontroller.maxAngularAcceleration * Time.deltaTime);
    //        return steering;
    //    }
    

    //        angle = (Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg)- 90;
    //        steering.angular = Mathf.LerpAngle(transform.rotation.eulerAngles.z, angle, steeringcontroller.maxAngularAcceleration * Time.deltaTime);
    //        steering.linear = Vector3.zero;
    //        return steering;
    //}
    
}