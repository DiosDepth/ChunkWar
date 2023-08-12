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
    public string beamVFX = "HitVFX";
    public float maxDistance;
    public GameObject target;




    private Coroutine startEmitCoroutine;



    private RaycastHit2D[] hitlist;


    public IEnumerator StartEmit()
    {
        
        if (beamtype == BeamType.Directional)
        {
            hitlist = Physics2D.RaycastAll(transform.position, _initialmoveDirection, maxDistance, mask);
            if(hitlist != null && hitlist?.Length > 0)
            {
                for (int i = 0; i < hitlist.Length; i++)
                {
                    if(hitlist[i].transform.gameObject != null)
                    {
                        yield return null;
                    }
                }
            }
        }
            
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
