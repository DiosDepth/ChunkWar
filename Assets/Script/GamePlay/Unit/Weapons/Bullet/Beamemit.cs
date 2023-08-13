using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum BeamType
{
    Directional,
    TargetBase,
}
public class Beamemit : Bullet
{
    public LayerMask mask = 1 << 7;
    public BeamType beamtype;
    
    public float maxDistance;
    public float width = 0.25f;
    public float emittime = 0.15f;
    public float duration = 0.75f;
    public float deathtime = 0.15f;

    public LineRenderer beamline;
    public GameObject target;


    private Coroutine startEmitCoroutine;
    private RaycastHit2D[] hitlist;


    public override void Shoot()
    {
        if(beamtype == BeamType.Directional)
        {
            DirectionalBeam();
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
            //��������
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
                        // ������Ч����

                        PlayVFX(HitVFX, hitlist[i].point);

                        //�����˺�
                        if (_owner is Weapon)
                        {
                            int damage = (_owner as Weapon).weaponAttribute.GetDamage();
                            hitlist[i].collider.GetComponent<Unit>()?.TakeDamage(-damage);
                        }
                    }
                }
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
