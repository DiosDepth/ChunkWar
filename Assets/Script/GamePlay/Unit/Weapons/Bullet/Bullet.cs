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
}
public class Bullet : PoolableObject
{
    public BulletType type;
    public OwnerType ownertype;
    public Collider2D bulletCollider;
    public string hitVFXName = "HitVFX";
    protected Unit _owner;


    protected override void OnEnable()
    {
        base.OnEnable();
    }
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }


    public override void Reset()
    {
        base.Reset();
    }

    public virtual void SetOwner(Unit owner)
    {
        _owner = owner;
    }
    public override void Initialization()
    {
        base.Initialization();
        bulletCollider = transform.GetComponentInChildren<Collider2D>();
    }


    public override void Destroy()
    {
        base.Destroy();
    }

    public override void SetActive(bool isactive = true)
    {
        base.SetActive(isactive);
    }

}
