using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class AISteeringBehaviorController : AIAction,IBoid
{

   //public SteeringBehaviorController steeringBehaviorController;

    public Rigidbody2D rb;
    private SteeringBehavior[] steerings;
    public float maxAcceleration = 10f;
    public float maxAngularAcceleration = 3f;
    public float boidRadius = 1f;

    public bool arriveBehavior = false;
    public bool faceBehavior = false;
    public bool cohesionBehavior = false;
    public bool separationBehavior = false;
    public bool aligmentBehavior = false;

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
    public override void Initialization()
    {
        base.Initialization();
    }

    protected override void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //steerings = GetComponents<SteeringBehavior>();
        rb.drag = drag;
        lastpos = transform.position;
    }

    public virtual void Move(Vector3 movepos)
    {
        if(rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
       rb.MovePosition(movepos);
    }
    public override void OnEnterAction()
    {
        base.OnEnterAction();
    }

    public override void UpdateAction()
    {

    }

    public override void FixedUpdateAction()
    {

    }

    public override void OnExitAction()
    {
        base.OnExitAction();
        //_brain.seeingDic.Clear();
    }
}
