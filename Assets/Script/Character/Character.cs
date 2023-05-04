using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public enum CharacterState
{
    Falling,
    Bunce,
    Rising,
    SuperBunce,
    SuperRising,
    RushDown,
    RushingDown,
    Death,
}


public class Character : MonoBehaviour
{
    public const float defaultg = -9.81f;


    public StateMachine<CharacterState> state;
    [Header("---PlayerSettings---")]
    public float rushDownSpeed = 35;
    public float buncingDistance = 5;
    public float superBuncingDistance = 15;
    public float HmaxMoveSpeed = 5;
    public float VmaxMoveSpeed = 15f;
    public float HAcceleration = 10;
    public Vector3 posBeforeRushDown;



    public Rigidbody2D rb;
    public ChaController controller;
    public Vector3 movement;
    public Vector3 rushmovement;
    public Vector3 buncingmovement;

    private float initialV;
    private bool isUpdate = false;

    public bool istestmode = false;
    // Start is called before the first frame update

    protected virtual void Awake()
    {
        if(istestmode)
        {
            Initialization();

            CameraManager.Instance.ChangeVCameraFollowTarget(this.transform);
        }

    }
    void Start()
    {

    }

    public float GetV0WhitDistance(float m_g, float m_d)
    {
        return -2 * m_g * m_d;
    }
    public virtual void Initialization()
    {
        this.gameObject.SetActive(true);
        state = new StateMachine<CharacterState>(this.gameObject, true);
        state.ChangeState(CharacterState.SuperBunce);
        isUpdate = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (isUpdate)
        EveryFram();

    }

    public void EveryFram()
    {
        movement = Vector3.zero;
        switch (state.CurrentState)
        {
          
            case CharacterState.Falling:

                break;
            case CharacterState.Bunce:

                initialV = GetV0WhitDistance(defaultg, buncingDistance);
                buncingmovement = BuncingUp(initialV);
                movement += buncingmovement;
                state.ChangeState(CharacterState.Rising);
                break;
            case CharacterState.Rising:
                buncingmovement = Vector3.zero;
                break;
            case CharacterState.SuperBunce:

                initialV = GetV0WhitDistance(defaultg, superBuncingDistance);

                buncingmovement = BuncingUp(initialV);
                movement += buncingmovement;
                state.ChangeState(CharacterState.SuperRising);
                break;
            case CharacterState.SuperRising:
                buncingmovement = Vector3.zero;
                break;
            case CharacterState.RushDown:
                rushmovement = Vector3.down * rushDownSpeed ;
                movement += rushmovement;
                break;
            case CharacterState.RushingDown:
                rushmovement = Vector3.zero;
                break;
            case CharacterState.Death:

                break;
        }


        movement += HAcceleration * controller.movement * Time.deltaTime ;

        Movement(movement);

        if(rb.velocity.y <= 0 && state.CurrentState != CharacterState.RushDown && state.CurrentState != CharacterState.RushingDown)
        {
            state.ChangeState(CharacterState.Falling);
        }

    }


    public Vector3 BuncingUp(float velocity)
    {
        return Vector3.up * velocity;
    }

    public void Movement(Vector3 m_movement)
    {
        Vector3 vloc = rb.velocity;
        vloc += m_movement;

        vloc = new Vector2(Mathf.Clamp(vloc.x, -1*HmaxMoveSpeed, HmaxMoveSpeed), Mathf.Clamp(vloc.y, -1 * VmaxMoveSpeed, VmaxMoveSpeed));
        rb.velocity = vloc;
        
    }

    public void Death()
    {
        Debug.Log("Death");
        PoolManager.Instance.GetObject(PoolManager.Instance.VFXPath + "DestroyVFX", true, (obj) =>
        {
            obj.transform.position = this.transform.position;
            obj.GetComponent<ParticleController>().SetActive();
            obj.GetComponent<ParticleController>().PlayVFX();
            GameObject.Destroy(this.gameObject);
        });
        isUpdate = false;
        GameEvent.Trigger(GameState.GameOver);
  
    }


    public void OnCollisionEnter2D(Collision2D collision)
    {
        rb.velocity = Vector3.zero;
        if(state.CurrentState == CharacterState.Falling)
        {
            state.ChangeState(CharacterState.Bunce);
        }

        if(state.CurrentState == CharacterState.RushingDown || state.CurrentState == CharacterState.RushDown)
        {
            state.ChangeState(CharacterState.SuperBunce);
        }

        if(state.CurrentState == CharacterState.Rising || state.CurrentState == CharacterState.SuperRising)
        {
            state.ChangeState(CharacterState.Falling);
        }

        ScoreEvent.Trigger(ScoreEventType.Change, 1);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "DeadZone")
        {
            Death();
        }
        
    }
}
