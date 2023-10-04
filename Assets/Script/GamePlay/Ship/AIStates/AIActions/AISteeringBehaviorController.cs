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
    public float maxAngularAcceleration = 3f;
    public float targetSerchingRadius = 15f;
    public float boidRadius = 1f;

    public bool evadeBehavior = false;
    [SerializeField]
    public EvadeBehavior evadeBehaviorInfo;
    public bool arriveBehavior = false;
    [SerializeField]
    public ArriveBehavior arrivelBehaviorInfo;
    public bool faceBehavior = false;
    [SerializeField]
    public FaceBehavior faceBehaviorInfo;
    public bool cohesionBehavior = false;
    [SerializeField]
    public CohesionBehavior cohesionBehaviorInfo;
    public bool separationBehavior = false;
    [SerializeField]
    public SeparationBehavior separationBehaviorInfo;
    public bool aligmentBehavior = false;
    [SerializeField]
    public AlignmentBehavior alignmentBehaviorInfo;

    public bool collisionAvoidanceBehavior = false;
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
    public struct CalculateTargetsPosByRadiusJob : IJobParallelForBatch
    {
        //[ReadOnly] public NativeArray<float3> job_selfPos;
        public float job_threshold;
        [ReadOnly] public NativeArray<float> job_SerchingRadius;
        [ReadOnly] public NativeArray<float3> job_aiShipPos;
        [ReadOnly] public NativeArray<float3> job_aiShipVel;
        

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
        [ReadOnly] public NativeArray<float> job_aiShipMaxAcceleration;
        [ReadOnly] public NativeArray<float> Job_aiShipDrag;
        [ReadOnly] public NativeArray<float3> job_aiShipVelocity;
        [ReadOnly] public NativeArray<float3> job_aiShipPos;
        [ReadOnly] public float job_deltatime;


        [ReadOnly] public NativeArray<SteeringBehaviorInfo> job_evadeSteering;
        [ReadOnly] public NativeArray<float> job_evadeWeight;

        [ReadOnly] public NativeArray<bool> job_isVelZero;
        [ReadOnly] public NativeArray<SteeringBehaviorInfo> job_arriveSteering;
        [ReadOnly] public NativeArray<float> job_arriveWeight;


        [ReadOnly] public NativeArray<SteeringBehaviorInfo> job_faceSteering;
        [ReadOnly] public NativeArray<float> job_faceWeight;

        [ReadOnly] public NativeArray<SteeringBehaviorInfo> job_cohesionSteering;
        [ReadOnly] public NativeArray<float> job_cohesionWeight;

        [ReadOnly] public NativeArray<SteeringBehaviorInfo> job_separationSteering;
        [ReadOnly] public NativeArray<float> job_separationWeight;
      


        [ReadOnly] public NativeArray<SteeringBehaviorInfo> job_alignmentSteering;
        [ReadOnly] public NativeArray<float> job_alignmentWeight;

        [ReadOnly] public NativeArray<SteeringBehaviorInfo> job_collisionAvoidanceSteering;
        [ReadOnly] public NativeArray<float> job_collisionAvoidanceWeight;

        public NativeArray<SteeringBehaviorInfo> rv_deltainfo;

        SteeringBehaviorInfo deltainfo;
        float3 accelaration;
        float angle;
        float3 deltamovement;
        float3 vel;
        public void Execute(int startIndex, int count)
        {
            // apply steering data to ai ship

            for (int i = startIndex; i < startIndex + count; i++)
            {
                angle = 0;
                accelaration = Vector3.zero;

                if(math.length(job_evadeSteering[i].linear) == 0)
                {
                    if (!job_isVelZero[i])
                    {
                        accelaration += job_arriveSteering[i].linear * job_arriveWeight[i];
                    }
               
                    accelaration += job_cohesionSteering[i].linear * job_cohesionWeight[i];
                    accelaration += job_alignmentSteering[i].linear * job_alignmentWeight[i];
                    accelaration += job_separationSteering[i].linear * job_separationWeight[i];
                    accelaration += job_collisionAvoidanceSteering[i].linear * job_collisionAvoidanceWeight[i];
                }
                else
                {
                    accelaration += job_evadeSteering[i].linear * job_evadeWeight[i];
                }


                angle += job_faceSteering[i].angular * job_faceWeight[i];

                //后面需要叠加其他的behavior

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

                if ( math.length(vel) <= 0.01f)
                {
                    vel = 0;
                }

                if(math.length(job_evadeSteering[i].linear) == 0)
                {
                    deltamovement = vel + accelaration * 0.5f * job_deltatime * Job_aiShipDrag[i];
                }
                else
                {
                    deltamovement = vel + accelaration * 0.5f * job_deltatime * Job_aiShipDrag[i];
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


