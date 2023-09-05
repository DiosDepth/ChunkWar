using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


public struct SteeringBehaviorInfo
{
    public float3 linear;
    public float angular;
}

public class AISteeringBehaviorController : MonoBehaviour, IBoid
{

    //public SteeringBehaviorController steeringBehaviorController;

    public Rigidbody2D rb;
    public float maxAcceleration = 10f;
    public float maxAngularAcceleration = 3f;
    public float boidRadius = 1f;


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



    public float drag = 1f;
    public Vector3 velocity;
    public Vector3 lastpos;

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    public float GetRadius()
    {
        return boidRadius;
    }


    public float GetRotationZ()
    {
        return transform.rotation.eulerAngles.z;
    }

    public void SetVelocity(Vector3 m_vect)
    {
        velocity = m_vect;

    }
    public void UpdateIBoid()
    {
        velocity = (transform.position - lastpos) / Time.deltaTime;
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


    [BurstCompatible]
    public struct CalculateDeltaMovePosJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<float> job_aiShipMaxAcceleration;
        [ReadOnly] public NativeArray<float> Job_aiShipDrag;
        [ReadOnly] public NativeArray<float3> job_aiShipVelocity;
        [ReadOnly] public NativeArray<float3> job_aiShipPos;
        [ReadOnly] public float job_deltatime;

        [ReadOnly] public NativeArray<bool> job_isVelZero;
        [ReadOnly] public NativeArray<SteeringBehaviorInfo> job_arriveSteering;
        [ReadOnly] public NativeArray<float> job_arriveWeight;


        [ReadOnly] public NativeArray<SteeringBehaviorInfo> job_faceSteering;
        [ReadOnly] public NativeArray<float> job_faceWeight;

        [ReadOnly] public NativeArray<SteeringBehaviorInfo> job_cohesionSteering;
        [ReadOnly] public NativeArray<SteeringBehaviorInfo> job_separationSteering;
        [ReadOnly] public NativeArray<SteeringBehaviorInfo> job_alignmentSteering;



        public NativeArray<SteeringBehaviorInfo> rv_deltainfo;

        SteeringBehaviorInfo deltainfo;
        float3 accelaration;
        float angle;
        float3 deltamovement;
        public void Execute(int startIndex, int count)
        {
            // apply steering data to ai ship

            for (int i = startIndex; i < startIndex + count; i++)
            {
                accelaration += job_arriveSteering[i].linear * job_arriveWeight[i];
                angle += job_faceSteering[i].angular * job_faceWeight[i];
                //后面需要叠加其他的behavior

                if (math.length(accelaration) > job_aiShipMaxAcceleration[i])
                {
                    accelaration = math.normalize(accelaration);
                    accelaration *= job_aiShipMaxAcceleration[i];
                }

                deltamovement = job_aiShipVelocity[i] + accelaration * 0.5f * job_deltatime * Job_aiShipDrag[i];
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


