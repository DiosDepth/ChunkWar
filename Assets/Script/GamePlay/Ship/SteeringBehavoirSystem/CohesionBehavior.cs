using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;

[System.Serializable]
public class CohesionBehavior : SteeringBehavior
{
    [SerializeField] public float viewAngle = 60f;
    [SerializeField] public float cohesionRadius = 10f;
    [SerializeField] public LayerMask mask = 1 << 9;


    public struct CohesionBehaviorJob : IJob
    {
        public void Execute()
        {
            
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
