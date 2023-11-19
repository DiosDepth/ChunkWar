using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class DummyShip : BaseShip,IPoolable,IBoid
{

    private Vector3 lastpos;
    private Vector3 velocity;
    private float boidRadius = 1;
    // Start is called before the first frame update

    protected override void Awake()
    {

    }
    public override void Initialization()
    {
        //base.Initialization();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableReset()
    {

    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }

    public override void GameOver()
    {
        base.GameOver();
    }

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
    }

    public void SetVelocity(Vector3 m_vect)
    {
        velocity = m_vect;
    }

    public void UpdateBoid()
    {
        lastpos = transform.position;
    }
}
