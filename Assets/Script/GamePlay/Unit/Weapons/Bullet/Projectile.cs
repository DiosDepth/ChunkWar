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
    public Collider2D bulletCollider;
    public ProjectileMovementType movementType = ProjectileMovementType.Straight;
    public float lifeTime = 10;
    public float maxSpeed = 2.5f;
    public float rotSpeed = 180f;
    public float initialSpeed = 20f;
    public float acceleration = 0.25f;
    public bool IsinterceptTarget = true;
    /// <summary>
    /// 血量管理组件
    /// </summary>
    public GeneralHPComponet HpComponent;

    private Coroutine startmovingCoroutine;
    private float _movetimestamp;
    protected float _beforemovetimestamp;
    private Vector3 _move;

    private Vector2 _movedirection;
    protected Vector2 _tempmovement;
    // Start is called before the first frame update
    protected override void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {

    }
    public override void Shoot()
    {
        base.Shoot();
        PoolableSetActive();
    }

    public override void Initialization()
    {
        base.Initialization();
        bulletCollider = transform.GetComponentInChildren<Collider2D>();
        HpComponent = new GeneralHPComponet(100, 100);
    }


    public IEnumerator Straight(UnityAction callback)
    {
        _movetimestamp = Time.time + lifeTime;

        while(Time.time < _movetimestamp)
        {
            _tempmovement = transform.position.ToVector2() + (_movedirection * maxSpeed * Time.deltaTime);
          
            rb.MovePosition(_tempmovement);
            yield return null;
        }
        callback?.Invoke();
    }

    public IEnumerator FollowTarget(UnityAction callback)
    {
        _beforemovetimestamp = Time.time + 0.5f;
        float currentspeed = initialSpeed;
        

        while (Time.time < _beforemovetimestamp)
        {
            currentspeed += acceleration;
            _tempmovement = transform.position.ToVector2() + (_movedirection * currentspeed * Time.deltaTime);

            //transform.rotation = MathExtensionTools.CalculateRotation(this.transform.up, _movedirection, rotSpeed);
            rb.MovePosition(_tempmovement);
            yield return null;
        }
        _movetimestamp = Time.time + lifeTime;

        
        while (Time.time < _movetimestamp)
        {
            if(target != null)
            {
                _movedirection = transform.position.DirectionToXY(target.transform.position);
            }


            currentspeed += acceleration;
            transform.rotation = MathExtensionTools.CalculateRotation(this.transform.up, _movedirection, rotSpeed);

            _tempmovement = transform.position.ToVector2() + (transform.up.ToVector2() * currentspeed * Time.deltaTime);

            rb.MovePosition(_tempmovement);

            yield return null;
        }
        callback?.Invoke();
    }



    public override void PoolableReset()
    {
        base.PoolableReset();

        _movedirection = Vector3.zero;
    }

    public override void PoolableSetActive(bool isactive = true)
    {
        base.PoolableSetActive(isactive);

        if(movementType == ProjectileMovementType.Straight)
        {
            _movedirection = InitialmoveDirection;
            startmovingCoroutine = StartCoroutine(Straight(() =>
            {
                PoolableDestroy();
            }));
        }

        if(movementType == ProjectileMovementType.FollowTarget)
        {
            _movedirection = InitialmoveDirection;
            startmovingCoroutine = StartCoroutine(FollowTarget(() =>
            {
                PoolableDestroy();
            }));
        }


    }

    public override void PoolableDestroy()
    {
        StopCoroutine(startmovingCoroutine);
        base.PoolableDestroy();


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

        if(collision.gameObject.layer == LayerMask.NameToLayer("Unit"))
        {
            if (_owner is Weapon)
            {
                var damage = (_owner as Weapon).weaponAttribute.GetDamage();
                collision.GetComponent<IDamageble>()?.TakeDamage(ref damage);
            }
            Death();
        }
    }

    public override void Death()
    {
        base.Death(); 
    }


    public bool TakeDamage(ref DamageResultInfo info)
    {
        if (HpComponent == null)
            return false;

        bool isDie = HpComponent.ChangeHP(-info.Damage);
        if (isDie)
        {
            Death();
        }
        return isDie;
    }
}
