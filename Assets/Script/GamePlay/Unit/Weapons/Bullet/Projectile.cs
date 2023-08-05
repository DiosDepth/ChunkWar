using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ParticleSystemJobs;

public enum ProjectileMovementType
{
    Straight,
    StraightToPos,
    FollowTarget,
}


public class Projectile : Bullet, IDamageble
{

    public Rigidbody2D rb;
    public ProjectileMovementType movementType = ProjectileMovementType.Straight;
    public float lifeTime = 10;
    public float speed = 2.5f;
   
    /// <summary>
    /// 血量管理组件
    /// </summary>
    public GeneralHPComponet HpComponent;

    public Vector2 InitialmoveDirection { get { return _initialmoveDirection; } set { _initialmoveDirection = value; } }
    private Vector2 _initialmoveDirection = Vector2.up;

    private Coroutine startmovingCoroutine;
    private float _timestamp;
    private Vector3 _move;

    private Vector2 _movedirection;
    // Start is called before the first frame update
    protected override void Start()
    {
        SetActive();
    }

    // Update is called once per frame
    protected override void Update()
    {

    }

    public override void Initialization()
    {
        base.Initialization();
        HpComponent = new GeneralHPComponet(100, 100);
    }


    public IEnumerator Straight(UnityAction callback)
    {
        _timestamp = Time.time + lifeTime;

        while(Time.time < _timestamp)
        {
            Vector2 move = transform.position.ToVector2() + (_movedirection * speed * Time.deltaTime);
          
            rb.MovePosition(move);
            yield return null;
        }
        callback?.Invoke();
    }



    public override void Reset()
    {
        base.Reset();
    }

    public override void SetActive(bool isactive = true)
    {
        base.SetActive(isactive);

        if(movementType == ProjectileMovementType.Straight)
        {
            _movedirection = InitialmoveDirection;
            startmovingCoroutine = StartCoroutine(Straight(() =>
            {
                Destroy();
            }));
        }


    }

    public override void Destroy()
    {
        StopCoroutine(startmovingCoroutine);
        base.Destroy();


    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        //PoolManager.Instance.GetObjectAsync(PoolManager.Instance.VFXPath + "DestroyVFX", true, (obj) => 
        //{
        //    obj.transform.position = this.transform.position;
        //    obj.GetComponent<ParticleController>().SetActive();
        //    obj.GetComponent<ParticleController>().PlayVFX();

        //});
        //Destroy();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == this.tag)
        {
            return;
        }
        PoolManager.Instance.GetObjectAsync(PoolManager.Instance.VFXPath + "DestroyVFX", true, (obj) =>
        {
            obj.transform.position = this.transform.position;
            obj.GetComponent<ParticleController>().SetActive();
            obj.GetComponent<ParticleController>().PlayVFX();

        });
        Destroy();
    }

    public bool TakeDamage(int value)
    {
        if (HpComponent == null)
            return false;

        return HpComponent.ChangeHP(value);
    }
}
