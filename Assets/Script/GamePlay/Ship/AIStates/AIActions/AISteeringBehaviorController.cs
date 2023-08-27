using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISteeringBehaviorController : AIAction,IBoid
{

   //public SteeringBehaviorController steeringBehaviorController;

    private Rigidbody2D rb;
    private SteeringBehavior[] steerings;
    public float maxAcceleration = 10f;
    public float maxAngularAcceleration = 3f;


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
        rb = GetComponent<Rigidbody2D>();
        steerings = GetComponents<SteeringBehavior>();
        rb.drag = drag;

        for (int i = 0; i < steerings.Length; i++)
        {
            steerings[i].SetTarget(RogueManager.Instance.currentShip.transform);
            //steerings[i].SetEnvAgents(LevelManager.Instance.aiShipList);
            // steerings[i].SetTarget(AIManager.Instance.target);
        }

        lastpos = transform.position;
        //rb.velocity = Vector3.up;

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
        Vector3 accelaration = Vector3.zero;
        Vector3 deltamovement = Vector3.zero;
        float rotation = 0f;
        foreach (SteeringBehavior behavior in steerings)
        {
            SteeringData steering = behavior.GetSteering(this);
            accelaration += steering.linear * behavior.GetWeight();
            rotation += steering.angular * behavior.GetWeight();
        }

        if (accelaration.magnitude > maxAcceleration)
        {
            accelaration.Normalize();
            accelaration *= maxAcceleration;
        }

        deltamovement = (velocity + accelaration * 0.5f * Time.fixedDeltaTime * drag);

        rb.MovePosition(transform.position + deltamovement * Time.fixedDeltaTime);
        //rb.AddForce(accelaration);

        UpdateIBoid();

        if (rotation != 0)
        {
            rb.rotation = rotation; //Quaternion.Euler(0, rotation, 0);
        }
    }

    public override void OnExitAction()
    {
        base.OnExitAction();
        //_brain.seeingDic.Clear();
    }
}
