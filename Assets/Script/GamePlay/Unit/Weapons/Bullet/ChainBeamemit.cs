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
    private float _tempRaytFrequenceStamp = float.MinValue;
    private Vector2 _direction;
    private bool _isChained;
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

        float beamPointPos = 0;
        LeanTween.value(0, maxDistance, emittime).setOnUpdate((value) =>
        {
            //ÿһ��ʱ�䷢������
            if (Time.time - _tempRaytFrequenceStamp >= RayCastFrequence)
            {
                _tempRaytFrequenceStamp = Time.time;

                _direction = MathExtensionTools.DirectionToXY(transform.position, initialTarget.transform.position);
                hitlist = Physics2D.RaycastAll(transform.position, _direction, maxDistance, mask);

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
            }

            if( hit)
            {
                //�жϾ���
                _targetDistance = MathExtensionTools.DistanceXY(hit.transform.position, transform.position);

                if ( value > _targetDistance)
                {
                    beamPointPos = _targetDistance;
                    maxDistance = beamPointPos;
                }
                else
                {
                    beamPointPos = value;
                }
            }
            else
            {
                beamPointPos = value;
            }
            

            beamline.SetPosition(1, new Vector3(0, beamPointPos, 0));

        }).setOnComplete(() =>
        {
            if(hit)
            {
                PlayVFX(HitVFX, hit.transform.position);

                //�����˺�
                if (_owner is Weapon)
                {
                    var damage = (_owner as Weapon).weaponAttribute.GetDamage();
                    damage.HitPoint = hit.point;
                    damage.attackerUnit = _owner;
                    var target = hit.collider.GetComponent<IDamageble>();
                    if(target != null)
                    {
                        damage.Target = target;
                        target.TakeDamage(damage);
                    }
                }
                _isChained = true;
            }

            LeanTween.delayedCall(duration, () =>
            {
                //��������
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

    public override void UpdateBullet()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!_isUpdate) { return; }
        if (firepoint == null) { return; }
        base.UpdateBullet();
        transform.position = firepoint.transform.position;
        if(initialTarget == null)
        {
            return;
        }

        _direction = MathExtensionTools.DirectionToXY(transform.position, initialTarget.transform.position);

        transform.rotation = MathExtensionTools.GetRotationFromDirection(_direction);
        if (!_isChained) { return; }

        if (hit)
        {
            _targetDistance = MathExtensionTools.DistanceXY(hit.transform.position, transform.position);
            beamline.SetPosition(1, new Vector3(0, _targetDistance, 0));
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        UpdateBullet();
    }



    public override void PoolableReset()
    {
        beamline.startWidth = 1;
        beamline.endWidth = 1;
        beamline.SetPosition(1, Vector3.zero);
        beamline.SetPosition(0, Vector3.zero);
        hit = new RaycastHit2D();
        maxDistance = (_owner as Weapon).weaponAttribute.WeaponRange;
        _tempRaytFrequenceStamp = float.MinValue;
        _isChained = false;

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
