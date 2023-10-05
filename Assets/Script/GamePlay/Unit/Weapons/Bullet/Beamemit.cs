using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Beamemit : Bullet
{
    public LayerMask mask = 1 << 7;

    
    public float maxDistance;
    public float width = 0.25f;
    public float emittime = 0.15f;
    public float duration = 0.75f;
    public float deathtime = 0.15f;

    public LineRenderer beamline;



    private Coroutine startEmitCoroutine;
    private RaycastHit2D[] hitlist;
    private Collider2D[] overlaplist;

    private float tempFalloff = 0.5f;

    public override void Shoot()
    {
        PoolableSetActive();
        tempFalloff = (_owner as Weapon).weaponAttribute.TransfixionReduce / 100f;
        switch ((_owner as Weapon).aimingtype)
        {
            case WeaponAimingType.Directional:
                DirectionalBeam();
                break;
            case WeaponAimingType.TargetDirectional:
                DirectionalBeam();
                break;
            case WeaponAimingType.TargetBased:
                TargetBaseBeam();
                break;
        }


    }


    public void DirectionalBeam()
    {
        Debug.Log("BeamShoot");

        beamline.startWidth = width;
        beamline.endWidth = width;
        LeanTween.value(0, maxDistance, emittime).setOnUpdate((value)=> 
        {
            beamline.SetPosition(1, new Vector3(0, value, 0));

        }).setOnComplete(()=> 
        {
            //创建射线
            hitlist = Physics2D.RaycastAll(transform.position, _initialmoveDirection, maxDistance, mask);

            if (hitlist != null && hitlist?.Length > 0)
            {
                for (int i = 0; i < hitlist.Length; i++)
                {
                    if (hitlist[i].collider.gameObject != null)
                    {
                        if (hitlist[i].collider.tag == this.tag)
                        {
                            continue;
                        }
                        // 创建特效表现

                        PlayVFX(HitVFX, hitlist[i].point);

                        //产生伤害
                        if (_owner is Weapon)
                        {
                            var damage = (_owner as Weapon).weaponAttribute.GetDamage();
                            ///TODO Value
                            damage.Damage = Mathf.RoundToInt(damage.Damage * Mathf.Pow(tempFalloff, i));
                            damage.attackerUnit = _owner;
                            damage.HitPoint = hitlist[i].point;
                            hitlist[i].collider.GetComponent<IDamageble>()?.TakeDamage(damage);
                        }
                    }
                }
            }
            //延迟激光然后销毁激光
            LeanTween.delayedCall(duration, () =>
            {
                LeanTween.value(width, 0, deathtime).setOnUpdate((value) =>
                {
                    beamline.startWidth = value;
                    beamline.endWidth = value;
                }).setOnComplete(() =>
                {
                    Death(null);
                });
            });
        });

    }


    public void TargetBaseBeam()
    {


    }

    public override void PlayVFX(string m_vfxname, Vector3 pos)
    {
        base.PlayVFX(m_vfxname, pos);
    }
    public override void Death(UnitDeathInfo info)
    {

        PoolableDestroy();
    }

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


    public override void PoolableReset()
    {
        beamline.startWidth = 1;
        beamline.endWidth = 1;
        beamline.SetPosition(1, Vector3.zero);
        base.PoolableReset();
    }

    public override void Initialization()
    {
        base.Initialization();
    }

    public override void PoolableDestroy()
    {
        base.PoolableDestroy();
    }

    public override void PoolableSetActive(bool isactive = true)
    {
        base.PoolableSetActive(isactive);
    }
}
