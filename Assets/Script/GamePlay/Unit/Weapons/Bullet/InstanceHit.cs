using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    //private float tempFalloff = 0.5f;

    private BulletInstanceHitConfig _instanceHitCfg;

    public override void SetUp(BulletConfig cfg, Action hitAction = null)
    {
        base.SetUp(cfg, hitAction);
        _instanceHitCfg = cfg as BulletInstanceHitConfig;

        width = _instanceHitCfg.Width;
        emittime = _instanceHitCfg.EmitTime;
        duration = _instanceHitCfg.Duration;
        deathtime = _instanceHitCfg.DeathTime;
    }

    public override void Shoot()
    {
        PoolableSetActive();
        //tempFalloff = (_owner as Weapon).weaponAttribute.TransfixionReduce / 100f;
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

    public override void SetOwner(Unit owner)
    {
        base.SetOwner(owner);
        InitMaxDistance();
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
    //        //��������
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
    //                    // ������Ч����

    //                    PlayVFX(HitVFX, hitlist[i].point);

    //                    //�����˺�
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
    //        //�ӳټ���Ȼ�����ټ���
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

        bool hittarget = false;
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

                    // ������Ч����
                    float distance = MathExtensionTools.DistanceXY( this.transform.position.ToVector2(), hitlist[i].point);

                    LeanTween.value(0, distance, emittime).setOnUpdate((value) =>
                    {
                        InstanceHitLine.SetPosition(1, new Vector3(0, value, 0));


                    }).setOnComplete(() =>
                    {
                        //�����˺�
                        if (_owner is Weapon)
                        {
                            var damage = (_owner as Weapon).weaponAttribute.GetDamage();
                            ///TODO Value
                            //damage.Damage = Mathf.RoundToInt(damage.Damage * Mathf.Pow(tempFalloff, i));
                            damage.attackerUnit = _owner;
                            damage.HitPoint = hitlist[i].point;
                            var target = hitlist[i].collider.GetComponent<IDamageble>();
                            if(target != null)
                            {
                                damage.Target = target;
                                target.TakeDamage(damage);
                            }

                            PlayVFX(_bulletCfg.HitEffect, hitlist[i].point);
                            hittarget = true;
                        }

                        //�ӳټ���Ȼ�����ټ���
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
                    //�ӳٱ�֤����
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
 
        }

        if (hittarget)
        {
            OnHitEffect();
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

        //�ӳٱ�֤����
        LeanTween.delayedCall(duration * 0.25f, () =>
        {
            LeanTween.value(0, maxDistance, deathtime).setOnUpdate((value) =>
            {
                InstanceHitLine.SetPosition(0, new Vector3(0, value, 0));
            });
        });


    }

    public override void PlayVFX(GeneralEffectConfig cfg, Vector2 pos)
    {
        base.PlayVFX(cfg, pos);
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

    private void InitMaxDistance()
    {
        maxDistance = (_owner as Weapon).weaponAttribute.BaseRange;
    }
}
