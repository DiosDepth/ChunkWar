using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



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

    private BulletBeamConfig _beamCfg;
    private Material _lineMat;

    protected override void Awake()
    {
        base.Awake();
        _lineMat = transform.SafeGetComponent<LineRenderer>().material;
    }

    public override void SetUp(BulletConfig cfg, Action hitAction = null)
    {
        base.SetUp(cfg, hitAction);
        _beamCfg = cfg as BulletBeamConfig;
        maxDistance = _beamCfg.MaxDistance;
        width = _beamCfg.Width;
        emittime = _beamCfg.EmiteTime;
        duration = _beamCfg.Duration;
        deathtime = _beamCfg.DeathTime;
    }

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
        _lineMat.mainTextureScale = new Vector2(maxDistance / 10f, 1);
        LeanTween.value(0, maxDistance, emittime).setOnUpdate((value)=> 
        {
            beamline.SetPosition(1, new Vector3(0, value, 0));

        }).setOnComplete(()=> 
        {
            //��������
            hitlist = Physics2D.RaycastAll(transform.position, _initialmoveDirection, maxDistance, mask);

            bool hitTarget = false;
            int transfixionIndex = 0;
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
                        if (i > transfixionCount)
                            return;

                        hitTarget = true;
                        PlayVFX(_bulletCfg.HitEffect, hitlist[i].point);

                        //�����˺�
                        if (_owner is Weapon)
                        {
                            var damage = (_owner as Weapon).weaponAttribute.GetDamage();
                            ///TODO Value
                            damage.Damage = Mathf.RoundToInt(damage.Damage * Mathf.Pow(tempFalloff, transfixionIndex));
                            damage.attackerUnit = _owner;
                            damage.HitPoint = hitlist[i].point;
                            var target = hitlist[i].collider.GetComponent<IDamageble>();
                            if(target != null)
                            {
                                damage.Target = target;
                                target.TakeDamage(damage);
                                transfixionIndex++;
                            }
                        }
                    }
                }
            }

            if (hitTarget)
            {
                OnHitEffect();
            }

            //�ӳټ���Ȼ�����ټ���
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
