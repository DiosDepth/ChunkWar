using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beamemit : Bullet
{
    public LayerMask mask = 1 << 7;



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
