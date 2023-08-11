using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BulletType
{
    Missile,
    Ballistic,
    Energy,
}


public enum AvaliableBulletType
{
    None,
    BaseAIBullet,
    BasePlayerBullet,
    BasePlayerBullet_02,
}
public class Bullet : MonoBehaviour,IPoolable
{
    public BulletType type;
    public OwnerType ownertype;
    public Collider2D bulletCollider;
    public string hitVFXName = "HitVFX";
    protected Unit _owner;

    public Vector2 InitialmoveDirection { get { return _initialmoveDirection; } set { _initialmoveDirection = value; } }
    protected Vector2 _initialmoveDirection = Vector2.up;

    protected virtual void OnEnable()
    {
 
    }
    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }


    public virtual void PoolableReset()
    {
 
    }

    public virtual void SetOwner(Unit owner)
    {
        _owner = owner;
    }
    public virtual void Initialization()
    {
       
        bulletCollider = transform.GetComponentInChildren<Collider2D>();
    }


    public virtual void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public virtual void PoolableSetActive(bool isactive = true)
    {

        this.gameObject.SetActive(isactive);
    }

}
