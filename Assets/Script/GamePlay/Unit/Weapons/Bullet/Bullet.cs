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
    BaseBullet_AI,
    BaseHommingBullet_AI,
    BaseBullet_Player,
    BaseBullet02_Player,
    BaseBeam_Player,
    BaseHommingBullet_Player,
}
public class Bullet : MonoBehaviour,IPoolable
{
    public BulletType type;
    public OwnerType ownertype;

    public string ShootVFX = "HitVFX";
    public string HitVFX = "HitVFX";
    public string DeathVFX = "HitVFX";
    protected Unit _owner;

    public GameObject target;
    public Vector2 InitialmoveDirection { get { return _initialmoveDirection; } set { _initialmoveDirection = value; } }
    protected Vector2 _initialmoveDirection = Vector2.up;



    public virtual void Initialization()
    {
        if(ownertype == OwnerType.AI && _owner is AIWeapon)
        {
            AIManager.Instance.AddBullet(this);
        }

    }
    public virtual void SetOwner(Unit owner)
    {
        _owner = owner;
    }

    public virtual void SetTarget(GameObject m_target)
    {
        target = m_target;
    }
    public virtual void Shoot()
    {

    }

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

    public virtual void Death()
    { 
        PlayVFX(DeathVFX, this.transform.position);
        if(ownertype == OwnerType.AI && _owner is AIWeapon)
        {
            AIManager.Instance.aibulletsList.Remove(this);
        }

        PoolableDestroy();
    }

    public virtual void PlayVFX(string m_vfxname, Vector3 pos)
    {
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + m_vfxname, true, (obj) =>
        {
            obj.transform.position = pos;
            obj.GetComponent<ParticleController>().PoolableSetActive();
            obj.GetComponent<ParticleController>().PlayVFX();
        });
    }


    public virtual void PoolableReset()
    {
        target = null;
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
