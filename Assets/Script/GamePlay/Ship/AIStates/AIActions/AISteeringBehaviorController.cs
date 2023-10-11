using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


public struct SteeringBehaviorInfo
{
    public float3 linear;
    public float angular;
}

public class AISteeringBehaviorController : BaseController, IBoid
{

    //public SteeringBehaviorController steeringBehaviorController;


    public float maxAcceleration = 10f;
    public float maxVelocity = 7f;
    public float maxAngularAcceleration = 3f;
    public float targetSerchingRadius = 15f;
    public float boidRadius = 1f;

    public bool isActiveEvade = false;
    [BoxGroup("evadeBehaviorInfo")]
    [ShowIf("isActiveEvade")]
    [SerializeField]
    public EvadeBehavior evadeBehaviorInfo;

    public bool isActiveArrive = false;
    [BoxGroup("arrivelBehaviorInfo")]
    [ShowIf("isActiveArrive")]
    [SerializeField]
    public ArriveBehavior arrivelBehaviorInfo;

    public bool isActiveFace = false;
    [BoxGroup("faceBehaviorInfo")]
    [ShowIf("isActiveFace")]
    [SerializeField]
    public FaceBehavior faceBehaviorInfo;

    public bool isActiveCohesion = false;
    [BoxGroup("cohesionBehaviorInfo")]
    [ShowIf("isActiveCohesion")]
    [SerializeField]
    public CohesionBehavior cohesionBehaviorInfo;

    public bool isActiveSeparation = false;
    [BoxGroup("separationBehaviorInfo")]
    [ShowIf("isActiveSeparation")]
    [SerializeField]
    public SeparationBehavior separationBehaviorInfo;

    public bool isActiveAligment = false;
    [BoxGroup("alignmentBehaviorInfo")]
    [ShowIf("isActiveAligment")]
    [SerializeField]
    public AlignmentBehavior alignmentBehaviorInfo;

    public bool isActiveCollisionAvoidance = false;
    [BoxGroup("collisionAvoidanceBehaviorInfo")]
    [ShowIf("isActiveCollisionAvoidance")]
    [SerializeField]
    public CollisionAvoidanceBehavior collisionAvoidanceBehaviorInfo;



    public float drag = 1f;
    public Vector3 velocity;
    public Vector3 lastpos;

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Vector3 GetVelocity()
    {
        return (transform.position - lastpos) / Time.fixedDeltaTime;
    }

    public float GetRadius()
    {
        return boidRadius;
    }


    public float GetRotationZ()
    {
        return math.degrees(math.atan2(transform.up.y, transform.up.x)) - 90;
        return transform.rotation.eulerAngles.z;
    }

    public void SetVelocity(Vector3 m_vect)
    {
        velocity = m_vect;

    }
    public void UpdateIBoid()
    {
        velocity = (transform.position - lastpos) / Time.fixedDeltaTime;
        lastpos = transform.position;
    }
    // Start is called before the first frame update


    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //steerings = GetComponents<SteeringBehavior>();
        rb.drag = drag;
        lastpos = transform.position;
    }


    [BurstCompile]
    public struct CalculateSteeringTargetsPosByRadiusJob : IJobParallelForBatch
    {
        //[ReadOnly] public NativeArray<float3> job_selfPos;
        public float job_threshold;
        [Unity.Collections.ReadOnly] public NativeArray<float> job_SerchingRadius;
        [Unity.Collections.ReadOnly] public NativeArray<float3> job_aiShipPos;
        [Unity.Collections.ReadOnly] public NativeArray<float3> job_aiShipVel;
        

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float3> rv_findedTargetsPosPreShip;
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float3> rv_findedTargetVelPreShip;
        public NativeArray<int> rv_findedTargetCountPreShip;
        int index;
        float distance;


        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; i++)
            {
                index = 0;
                for (int n = 0; n < job_aiShipPos.Length; n++)
                {

                    distance = math.distance(job_aiShipPos[i], job_aiShipPos[n]);
                    if (distance <= job_SerchingRadius[i] && distance > job_threshold)
                    {
                        rv_findedTargetsPosPreShip[i * job_aiShipPos.Length + n] = job_aiShipPos[n];
                        rv_findedTargetVelPreShip[i * job_aiShipPos.Length + n] = job_aiShipPos[n];
                        index++;
                    }
                }
                rv_findedTargetCountPreShip[i] = index;
            }
        }
    }




    [BurstCompile]
    public struct CalculateDeltaMovePosJob : IJobParallelForBatch
    {
        [Unity.Collections.ReadOnly] public NativeArray<float> job_aiShipMaxAcceleration;
        [Unity.Collections.ReadOnly] public NativeArray<float> job_aiShipMaxVelocity;
        [Unity.Collections.ReadOnly] public NativeArray<float> Job_aiShipDrag;
        [Unity.Collections.ReadOnly] public NativeArray<float3> job_aiShipVelocity;
        [Unity.Collections.ReadOnly] public NativeArray<float3> job_aiShipPos;
        [Unity.Collections.ReadOnly] public float job_deltatime;


        [Unity.Collections.ReadOnly] public NativeArray<SteeringBehaviorInfo> job_evadeSteering;
        [Unity.Collections.ReadOnly] public NativeArray<float> job_evadeWeight;
        [Unity.Collections.ReadOnly] public NativeArray<bool> job_evadeIsActive;

        [Unity.Collections.ReadOnly] public NativeArray<bool> job_isVelZero;
        [Unity.Collections.ReadOnly] public NativeArray<SteeringBehaviorInfo> job_arriveSteering;
        [Unity.Collections.ReadOnly] public NativeArray<float> job_arriveWeight;
        [Unity.Collections.ReadOnly] public NativeArray<bool> job_arriveIsActive;

        [Unity.Collections.ReadOnly] public NativeArray<SteeringBehaviorInfo> job_faceSteering;
        [Unity.Collections.ReadOnly] public NativeArray<float> job_faceWeight;
        [Unity.Collections.ReadOnly] public NativeArray<bool> job_faceIsActive;

        [Unity.Collections.ReadOnly] public NativeArray<SteeringBehaviorInfo> job_cohesionSteering;
        [Unity.Collections.ReadOnly] public NativeArray<float> job_cohesionWeight;
        [Unity.Collections.ReadOnly] public NativeArray<bool> job_cohesionIsActive;

        [Unity.Collections.ReadOnly] public NativeArray<SteeringBehaviorInfo> job_separationSteering;
        [Unity.Collections.ReadOnly] public NativeArray<float> job_separationWeight;
        [Unity.Collections.ReadOnly] public NativeArray<bool> job_separationIsActive;

        [Unity.Collections.ReadOnly] public NativeArray<SteeringBehaviorInfo> job_alignmentSteering;
        [Unity.Collections.ReadOnly] public NativeArray<float> job_alignmentWeight;
        [Unity.Collections.ReadOnly] public NativeArray<bool> job_alignmentIsActive;

        [Unity.Collections.ReadOnly] public NativeArray<SteeringBehaviorInfo> job_collisionAvoidanceSteering;
        [Unity.Collections.ReadOnly] public NativeArray<float> job_collisionAvoidanceWeight;
        [Unity.Collections.ReadOnly] public NativeArray<bool> job_collisonAvidanceIsActive;

        public NativeArray<SteeringBehaviorInfo> rv_deltainfo;

        SteeringBehaviorInfo deltainfo;
        float3 accelaration;
        float angle;
        float3 deltamovement;
        float3 vel;

        public void Execute(int startIndex, int count)
        {
            for (int i = startIndex; i < startIndex + count; i++)
            {
                float angle = 0;
                float3 accelaration = float3.zero;

                if (!job_isVelZero[i])
                {
                    if (job_arriveIsActive[i]) { accelaration += job_arriveSteering[i].linear * job_arriveWeight[i]; }

                }

                if (job_cohesionIsActive[i]) { accelaration += job_cohesionSteering[i].linear * job_cohesionWeight[i]; }
                if (job_alignmentIsActive[i]) { accelaration += job_alignmentSteering[i].linear * job_alignmentWeight[i]; }
                if (job_separationIsActive[i]) { accelaration += job_separationSteering[i].linear * job_separationWeight[i]; }
                if (job_collisonAvidanceIsActive[i]) { accelaration += job_collisionAvoidanceSteering[i].linear * job_collisionAvoidanceWeight[i]; }
                if (job_evadeIsActive[i]) { accelaration += job_evadeSteering[i].linear * job_evadeWeight[i]; }

                angle += job_faceSteering[i].angular * job_faceWeight[i];

                if (math.length(accelaration) > job_aiShipMaxAcceleration[i])
                {
                    accelaration = math.normalize(accelaration);
                    accelaration *= job_aiShipMaxAcceleration[i];
                }

                if (math.length(job_evadeSteering[i].linear) == 0 && job_isVelZero[i])
                {
                    vel = float3.zero;
                }
                else
                {
                    vel = job_aiShipVelocity[i];
                }

                if (math.length(vel) <= 0.01f)
                {
                    vel = 0;
                }

                deltamovement = vel + accelaration * 0.5f * job_deltatime * Job_aiShipDrag[i];

                if(math.length(deltamovement) >= job_aiShipMaxVelocity[i])
                {
                    deltamovement = math.normalize(deltamovement) * job_aiShipMaxVelocity[i];
                }

                deltamovement = job_aiShipPos[i] + deltamovement * job_deltatime;

                deltainfo.linear = deltamovement;

                if (angle != 0)
                {
                    deltainfo.angular = angle;
                }
                rv_deltainfo[i] = deltainfo;
            }



        }






        //public void Execute(int startIndex, int count)
        //{
        //    // apply steering data to ai ship

        //    for (int i = startIndex; i < startIndex + count; i++)
        //    {
        //        angle = 0;
        //        accelaration = Vector3.zero;

        //        if(math.length(job_evadeSteering[i].linear) == 0)
        //        {
        //            if (!job_isVelZero[i])
        //            {
        //                accelaration += job_arriveSteering[i].linear * job_arriveWeight[i];
        //            }

        //            accelaration += job_cohesionSteering[i].linear * job_cohesionWeight[i];
        //            accelaration += job_alignmentSteering[i].linear * job_alignmentWeight[i];
        //            accelaration += job_separationSteering[i].linear * job_separationWeight[i];
        //            accelaration += job_collisionAvoidanceSteering[i].linear * job_collisionAvoidanceWeight[i];
        //        }
        //        else
        //        {
        //            accelaration += job_evadeSteering[i].linear * job_evadeWeight[i];
        //        }


        //        angle += job_faceSteering[i].angular * job_faceWeight[i];

        //        //后面需要叠加其他的behavior

        //        if (math.length(accelaration) > job_aiShipMaxAcceleration[i])
        //        {
        //            accelaration = math.normalize(accelaration);
        //            accelaration *= job_aiShipMaxAcceleration[i];
        //        }


        //        if (math.length(job_evadeSteering[i].linear) == 0 && job_isVelZero[i])
        //        {
        //            vel = float3.zero;
        //        }
        //        else
        //        {
        //            vel = job_aiShipVelocity[i];
        //        }

        //        if ( math.length(vel) <= 0.01f)
        //        {
        //            vel = 0;
        //        }

        //        if(math.length(job_evadeSteering[i].linear) == 0)
        //        {
        //            deltamovement = vel + accelaration * 0.5f * job_deltatime * Job_aiShipDrag[i];
        //        }
        //        else
        //        {
        //            deltamovement = vel + accelaration * 0.5f * job_deltatime * Job_aiShipDrag[i];
        //        }

        //        deltamovement = job_aiShipPos[i] + deltamovement * job_deltatime;

        //        deltainfo.linear = deltamovement;

        //        if (angle != 0)
        //        {
        //            deltainfo.angular = angle;
        //        }
        //        rv_deltainfo[i] = deltainfo;
        //    }
        //}
    }
    public virtual void Move(Vector3 movepos)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        rb.MovePosition(movepos);
    }
}


