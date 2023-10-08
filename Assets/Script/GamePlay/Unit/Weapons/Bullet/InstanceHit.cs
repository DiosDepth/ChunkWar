using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceHit : Bullet
{
    public LayerMask mask = 1 << 7;


    public float maxDistance;
    public float width = 0.25f;
    public float emittime = 0.15f;
    public float duration = 0.75f;
    public float deathtime = 0.15f;

    public LineRenderer InstanceHitLine;

    private Coroutine startEmitCoroutine;
    private RaycastHit2D[] hitlist;
    private Collider2D[] overlaplist;

    private float tempFalloff = 0.5f;

    private BulletInstanceHitConfig _instanceHitCfg;

    public override void SetUp(BulletConfig cfg)
    {
        base.SetUp(cfg);
        _instanceHitCfg = cfg as BulletInstanceHitConfig;
        maxDistance = _instanceHitCfg.MaxDistance;
        width = _instanceHitCfg.Width;
        emittime = _instanceHitCfg.EmitTime;
        duration = _instanceHitCfg.Duration;
        deathtime = _instanceHitCfg.DeathTime;
    }

    public override void Shoot()
    {
        PoolableSetActive();
        tempFalloff = (_owner as Weapon).weaponAttribute.TransfixionReduce / 100f;
        switch ((_owner as Weapon).aimingtype)
        {
            case WeaponAimingType.Directional:
                DirectionalInstanceHit();
                //DirectionalInstanceHit();
                break;
            case WeaponAimingType.TargetDirectional:
                DirectionalInstanceHit();
                break;
            case WeaponAimingType.TargetBased:

                break;
        }
    }


    //public void DirectionalInstanceHit()
    //{
    //    Debug.Log("InstanceHit");

    //    InstanceHitLine.startWidth = width;
    //    InstanceHitLine.endWidth = width;
    //    LeanTween.value(0, maxDistance, emittime).setOnUpdate((value) =>
    //    {
    //        InstanceHitLine.SetPosition(1, new Vector3(0, value, 0));

    //    }).setOnComplete(() =>
    //    {
    //        //创建射线
    //        hitlist = Physics2D.RaycastAll(transform.position, _initialmoveDirection, maxDistance, mask);

    //        if (hitlist != null && hitlist?.Length > 0)
    //        {
    //            for (int i = 0; i < hitlist.Length; i++)
    //            {
    //                if (hitlist[i].collider.gameObject != null)
    //                {
    //                    if (hitlist[i].collider.tag == this.tag)
    //                    {
    //                        continue;
    //                    }
    //                    // 创建特效表现

    //                    PlayVFX(HitVFX, hitlist[i].point);

    //                    //产生伤害
    //                    if (_owner is Weapon)
    //                    {
    //                        var damage = (_owner as Weapon).weaponAttribute.GetDamage();
    //                        ///TODO Value
    //                        damage.Damage = Mathf.RoundToInt(damage.Damage * Mathf.Pow(tempFalloff, i));
    //                        hitlist[i].collider.GetComponent<IDamageble>()?.TakeDamage(damage);
    //                    }
    //                }
    //            }
    //        }
    //        //延迟激光然后销毁激光
    //        LeanTween.delayedCall(duration, () =>
    //        {
    //            LeanTween.value(width, 0, deathtime).setOnUpdate((value) =>
    //            {
    //                InstanceHitLine.startWidth = value;
    //                InstanceHitLine.endWidth = value;
    //            }).setOnComplete(() =>
    //            {
    //                Death(null);
    //            });
    //        });
    //    });

    //}


    public void DirectionalInstanceHit()
    {

        InstanceHitLine.startWidth = 0;
        InstanceHitLine.endWidth = width;

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
                    float distance = MathExtensionTools.DistanceXY( this.transform.position.ToVector2(), hitlist[i].point);

                    LeanTween.value(0, distance, emittime).setOnUpdate((value) =>
                    {
                        InstanceHitLine.SetPosition(1, new Vector3(0, value, 0));


                    }).setOnComplete(() =>
                    {
                        PlayVFX(HitVFX, hitlist[i].point);
                        //产生伤害
                        if (_owner is Weapon)
                        {
                            var damage = (_owner as Weapon).weaponAttribute.GetDamage();
                            ///TODO Value
                            damage.Damage = Mathf.RoundToInt(damage.Damage * Mathf.Pow(tempFalloff, i));
                            damage.attackerUnit = _owner;
                            damage.HitPoint = hitlist[i].point;
                            var target = hitlist[i].collider.GetComponent<IDamageble>();
                            if(target != null)
                            {
                                damage.Target = target;
                                target.TakeDamage(damage);
                            }
                        }

                        //延迟激光然后销毁激光
                        LeanTween.delayedCall(duration, () =>
                        {
                            //LeanTween.value(0, distance, deathtime).setOnUpdate((value) =>
                            //{
                            //    InstanceHitLine.SetPosition(0, new Vector3(0, value, 0));
                            //});

                                LeanTween.value(width, 0, deathtime).setOnUpdate((value) =>
                            {
                                InstanceHitLine.startWidth = value;
                                InstanceHitLine.endWidth = value;
                            }).setOnComplete(() =>
                            {
                                Death(null);
                            });
                        });
                    });
                    //延迟保证长度
                    LeanTween.delayedCall(duration * 0.25f, () =>
                    {
                        LeanTween.value(0, distance, deathtime).setOnUpdate((value) =>
                        {
                            InstanceHitLine.SetPosition(0, new Vector3(0, value, 0));
                        });
                    });
                    return;
                }
            }
            return;
        }

        LeanTween.value(0, maxDistance, emittime).setOnUpdate((value) =>
        {
            InstanceHitLine.SetPosition(1, new Vector3(0, value, 0));

        }).setOnComplete(() => 
        {
            LeanTween.delayedCall(duration, () =>
            {
                //LeanTween.value(0, maxDistance, deathtime).setOnUpdate((value) =>
                //{
                //    InstanceHitLine.SetPosition(0, new Vector3(0, value, 0));
                //});

                LeanTween.value(width, 0, deathtime).setOnUpdate((value) =>
                {
                    InstanceHitLine.startWidth = value;
                    InstanceHitLine.endWidth = value;
                }).setOnComplete(() =>
                {
                    Death(null);
                });
            });

        });

        //延迟保证长度
        LeanTween.delayedCall(duration * 0.25f, () =>
        {
            LeanTween.value(0, maxDistance, deathtime).setOnUpdate((value) =>
            {
                InstanceHitLine.SetPosition(0, new Vector3(0, value, 0));
            });
        });


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
        InstanceHitLine.startWidth = 1;
        InstanceHitLine.endWidth = 1;
        InstanceHitLine.SetPosition(1, Vector3.zero);
        InstanceHitLine.SetPosition(0, Vector3.zero);
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
