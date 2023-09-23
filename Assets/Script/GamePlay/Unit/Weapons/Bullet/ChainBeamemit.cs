using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainBeamemit : Bullet
{
    public LayerMask mask = 1 << 7;


    public float maxDistance;
    public float width = 0.25f;
    public float emittime = 0.15f;
    public float duration = 0.75f;
    public float deathtime = 0.15f;

    public float RayCastFrequence = 0.25f;
    public LineRenderer beamline;



    private Coroutine startEmitCoroutine;
    private RaycastHit2D[] hitlist;
    private RaycastHit2D hit;
    private Collider2D[] overlaplist;

  
    private float _targetDistance;
    private float _tempRaytFrequenceStamp;
    private Vector2 _direction;

    public override void Shoot()
    {
        PoolableSetActive();
        duration = (_owner as Weapon).weaponAttribute.FireCD;
        maxDistance = (_owner as Weapon).weaponAttribute.WeaponRange;

        switch ((_owner as Weapon).aimingtype)
        {
            case WeaponAimingType.Directional:
                break;
            case WeaponAimingType.TargetDirectional:
                break;
            case WeaponAimingType.TargetBased:
                TargetBaseBeam();
                break;
        }


    }



    public void TargetBaseBeam()
    {
        beamline.startWidth = width;
        beamline.endWidth = width;
        _targetDistance = MathExtensionTools.DistanceXY(target.transform.position, transform.position);

        _tempRaytFrequenceStamp = Time.time;

        float beamPointPos = 0;
        LeanTween.value(0, maxDistance, emittime).setOnUpdate((value) =>
        {
            //每一段时间发射射线
            if (Time.time - _tempRaytFrequenceStamp >= RayCastFrequence)
            {
                _tempRaytFrequenceStamp = Time.time;

                hitlist = Physics2D.RaycastAll(transform.position, _initialmoveDirection, maxDistance, mask);

                //Find First hit Target;
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
                            hit = hitlist[i];
                            break;
                        }
                    }
                }
                if( hit.transform.gameObject != null)
                {
                    //判断距离
                    _targetDistance = MathExtensionTools.DistanceXY(hit.transform.position, transform.position);

                    if ( value > _targetDistance)
                    {
                        beamPointPos = _targetDistance;
                        maxDistance = beamPointPos;
                    }

                    PlayVFX(HitVFX, hitlist[i].point);

                    //产生伤害
                    if (_owner is Weapon)
                    {
                        var damage = (_owner as Weapon).weaponAttribute.GetDamage();
                        hit.collider.GetComponent<IDamageble>()?.TakeDamage(ref damage);
                    }
                    _isUpdate = true;
                }
                else
                {
                    beamPointPos = value;
                }
            }

            beamline.SetPosition(1, new Vector3(0, beamPointPos, 0));

        }).setOnComplete(() =>
        {
            LeanTween.delayedCall(duration, () =>
            {
                //创建射线
                LeanTween.value(width, 0, deathtime).setOnUpdate((value) =>
                {
                    beamline.startWidth = value;
                    beamline.endWidth = value;
                }).setOnComplete(() =>
                {
                    Death();
                });
            });


        });

    }

    public override void PlayVFX(string m_vfxname, Vector3 pos)
    {
        base.PlayVFX(m_vfxname, pos);
    }
    public override void Death()
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
        if(!_isUpdate)
        {
            return;
        }
        base.Update();
        transform.position = firepoint.transform.position;
        //_targetDistance = MathExtensionTools.DistanceXY(target.transform.position, transform.position);
        _direction = MathExtensionTools.DirectionToXY(transform.position, target.transform.position);

        transform.rotation = MathExtensionTools.GetRotationFromDirection(_direction);
        //update start Position
        //update End Position
        //update rotation

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
