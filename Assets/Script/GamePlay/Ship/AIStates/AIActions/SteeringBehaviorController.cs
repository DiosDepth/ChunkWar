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

public class SteeringBehaviorController : BaseController
{

    //public SteeringBehaviorController steeringBehaviorController;


    public float maxAcceleration = 10f;
    public float maxVelocity = 7f;
    public float maxAngularAcceleration = 1f;
    public float maxAngularVelocity = 3f;
    public float targetSerchingRadius = 15f;


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


    


    public void SetAIConfig(AIShipConfig cfg)
    {
        boidRadius = cfg.boidRadius;
        maxAcceleration = cfg.MaxAcceleration;
        maxAngularAcceleration = cfg.MaxAngularAcceleration;
        maxVelocity = cfg.MaxVelocity;
        maxAngularVelocity = cfg.MaxAngularVelocity;
        targetSerchingRadius = cfg.targetSerchingRadius;
    }



    // Start is called before the first frame update


    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //steerings = GetComponents<SteeringBehavior>();

    }

    public override void Initialization()
    {
        base.Initialization();
        rb.drag = drag;
        lastpos = transform.position;
    }
    [BurstCompile]
    public struct CalculateSteeringTargetsPosByRadiusJob : IJobParallelForBatch
    {

        //[ReadOnly] public NativeArray<float3> job_selfPos;
        [Unity.Collections.ReadOnly] public float job_threshold;
        [Unity.Collections.ReadOnly] public NativeArray<BoidJobData> job_boidData;
        [Unity.Collections.ReadOnly] public NativeArray<SteeringControllerJobData> job_steeringControllerData;

       
        

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
                for (int n = 0; n < job_boidData.Length; n++)
                {

                    distance = math.distance(job_boidData[i].position, job_boidData[n].position);
                    if (distance <= job_steeringControllerData[i].targetSerchingRadius && distance > job_threshold)
                    {
                        rv_findedTargetsPosPreShip[i * job_boidData.Length + n] = job_boidData[n].position;
                        rv_findedTargetVelPreShip[i * job_boidData.Length + n] = job_boidData[n].position;
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
        [Unity.Collections.ReadOnly] public float job_deltatime;
        [Unity.Collections.ReadOnly] public NativeArray<BoidJobData> job_boidData;
        [Unity.Collections.ReadOnly] public NativeArray<SteeringControllerJobData> job_steeringControllerData;
        [Unity.Collections.ReadOnly] public NativeArray<SteeringBehaviorInfo> job_evadeSteering;
        [Unity.Collections.ReadOnly] public NativeArray<bool> job_isVelZero;
        [Unity.Collections.ReadOnly] public NativeArray<SteeringBehaviorInfo> job_arriveSteering;
        [Unity.Collections.ReadOnly] public NativeArray<SteeringBehaviorInfo> job_faceSteering;
        [Unity.Collections.ReadOnly] public NativeArray<SteeringBehaviorInfo> job_cohesionSteering;
        [Unity.Collections.ReadOnly] public NativeArray<SteeringBehaviorInfo> job_separationSteering;
        [Unity.Collections.ReadOnly] public NativeArray<SteeringBehaviorInfo> job_alignmentSteering;
        [Unity.Collections.ReadOnly] public NativeArray<SteeringBehaviorInfo> job_collisionAvoidanceSteering;

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
                    if (job_steeringControllerData[i].arriveData.arrive_isActive) { accelaration += job_arriveSteering[i].linear * job_steeringControllerData[i].arriveData.arrive_weight; }

                }

                if (job_steeringControllerData[i].cohesionData.cohesion_isActive) { accelaration += job_cohesionSteering[i].linear * job_steeringControllerData[i].cohesionData.cohesion_weight; }
                if (job_steeringControllerData[i].alignmentData.alignment_isActive) { accelaration += job_alignmentSteering[i].linear * job_steeringControllerData[i].alignmentData.alignment_weight; }
                if (job_steeringControllerData[i].separationData.separation_isActive) { accelaration += job_separationSteering[i].linear * job_steeringControllerData[i].separationData.separation_weight; }
                if (job_steeringControllerData[i].collisionAvoidenceData.collisionavoidance_isActive) { accelaration += job_collisionAvoidanceSteering[i].linear * job_steeringControllerData[i].collisionAvoidenceData.collisionavoidance_weight; }
                if (job_steeringControllerData[i].evadeData.evade_isActive) { accelaration += job_evadeSteering[i].linear * job_steeringControllerData[i].evadeData.evade_weight; }

                angle += job_faceSteering[i].angular * job_steeringControllerData[i].faceData.face_weight;

                if (math.length(accelaration) > job_steeringControllerData[i].maxAcceleration)
                {
                    accelaration = math.normalize(accelaration);
                    accelaration *= job_steeringControllerData[i].maxAcceleration;
                }

                if (math.length(job_evadeSteering[i].linear) == 0 && job_isVelZero[i])
                {
                    vel = float3.zero;
                }
                else
                {
                    vel = job_boidData[i].velocity;
                }

                if (math.length(vel) <= 0.01f)
                {
                    vel = 0;
                }

                deltamovement = vel + accelaration * 0.5f * job_deltatime * job_steeringControllerData[i].drag;

                if(math.length(deltamovement) >= job_steeringControllerData[i].maxVelocity)
                {
                    deltamovement = math.normalize(deltamovement) * job_steeringControllerData[i].maxVelocity;
                }

                deltamovement = job_boidData[i].position + deltamovement * job_deltatime;

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


