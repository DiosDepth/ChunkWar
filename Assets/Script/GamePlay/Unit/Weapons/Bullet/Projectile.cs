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
    public bool IsinterceptTarget = true;
    /// <summary>
    /// 血量管理组件
    /// </summary>
    public GeneralHPComponet HpComponent;



    private Coroutine startmovingCoroutine;
    private float _timestamp;
    private Vector3 _move;

    private Vector2 _movedirection;
    // Start is called before the first frame update
    protected override void Start()
    {
        PoolableSetActive();
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



    public override void PoolableReset()
    {
        base.PoolableReset();
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
                int damage = (_owner as Weapon).weaponAttribute.GetDamage();
                collision.GetComponent<Unit>()?.TakeDamage(-damage);
            }
            Death();
        }



    }

    public virtual void Death()
    {
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + hitVFXName, true, (obj) =>
        {
            obj.transform.position = this.transform.position;
            obj.GetComponent<ParticleController>().PoolableSetActive();
            obj.GetComponent<ParticleController>().PlayVFX();
        });
        PoolableDestroy();
    }


    public bool TakeDamage(int value)
    {
        if (HpComponent == null)
            return false;

        bool isDie = HpComponent.ChangeHP(value);
        if (isDie)
        {
            Death();
        }
        return isDie;
    }
}
