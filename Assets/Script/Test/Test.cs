using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ditzelgames;

public class Test : MonoBehaviour
{
    
   public const float defaultg = -9.81f;
   public Rigidbody2D rg;


   private Vector3 startPos;
   private Vector3 endPos;
   private Vector3 targetPos;

   public Transform target;
   public Vector2 velocity;

   public float torque = 1;
   public float initialV = 0;
   public float finalV = 0;
   public float time = 1;
   public float gravity = -9.81f;
   public float distance = 0;
   public float Hight = 0;
   public float TotalTime = 0;
   private bool startrecode = false;
   private bool hightrecod = true;

   private float defaultFixedDeltaTime;

   // Start is called before the first frame update
     void Start()
    {
        rg = GetComponent<Rigidbody2D>();
        Debug.Log(Physics2D.gravity.magnitude);
        defaultFixedDeltaTime = Time.fixedDeltaTime;

    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(InputCenter.Instance.LaunchButton.State.CurrentState == InputInfo.ButtonState.ButtonUp)
        {

            //g = GetG(V0, d, t);
            rg.gravityScale = gravity / defaultg;
            initialV = GetV0(finalV, -1*rg.gravityScale * Physics2D.gravity.magnitude, distance);
            time = GetT(initialV, finalV, gravity);
            //V0 = GetV0WithTime(g, d, t);
            rg.velocity = transform.up * initialV;
            startPos = transform.position;
            startrecode = true;
            hightrecod = true;

            endPos = startPos;
            TotalTime = 0;
        }*/
/*
        if(InputCenter.Instance.AxisHorizontalButtonState == InputInfo.ButtonState.ButtonPressed)
        {
            if(InputCenter.Instance.HorizontalAxisValue> 0)
            {
                rg.AddTorque(-1* torque * Time.deltaTime);
            }

            if (InputCenter.Instance.HorizontalAxisValue < 0)
            {
                rg.AddTorque(torque * Time.deltaTime);
            }

        }

        if(InputCenter.Instance.SlowMotionButton.State.CurrentState == InputInfo.ButtonState.ButtonPressed)
        {
            Time.timeScale = 0.2f;
            Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;

        }
        else
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = defaultFixedDeltaTime;

        }*/

    }

    private void FixedUpdate()
    {
        if (startrecode && endPos.y < transform.position.y)
        {
            TotalTime += Time.fixedDeltaTime;
            endPos = transform.position;
            Hight = Vector3.Distance(startPos, endPos);
            if(Hight >= distance && hightrecod)
            {
                Debug.Log("Spent " + TotalTime + " to reach " + Hight);
                hightrecod = false;
            }
        }


        //Debug.Log("Hight : " + Vector3.Distance(startPos, endPos));

    }

    public float GetV0(float m_Vf, float m_g, float m_d)
    {
        return Mathf.Sqrt((m_Vf * m_Vf) - (2 * m_g * m_d));
    }

    public float GetV0WithTime(float m_g, float m_d, float m_t)
    {
        return (m_d - m_g * m_t * m_t * 0.5f) / m_t;
    }

    public float GetV0WhitDistance(float m_g, float m_d)
    {
        return -2 * m_g * m_d;
    }

    public float GetT(float m_V0, float m_Vf, float m_g)
    {
        return (m_Vf - m_V0) / m_g;
    }

    public float GetTWhitVf(float m_Vf, float m_g, float m_d)
    {
        return Mathf.Sqrt(-2 * m_d / m_g);
    }

    public float GetG(float m_V0, float m_d, float m_t)
    {
        return (2 * m_d - 2 * m_V0 * m_t) / (m_t * m_t);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(startPos, 0.2f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(endPos, 0.2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(new Vector3(startPos.x, startPos.y + distance, startPos.z), 0.2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * -1 * 2);
    }
    
}

