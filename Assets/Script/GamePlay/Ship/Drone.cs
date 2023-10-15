using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : BaseShip, IPoolable
{
    public override void Initialization()
    {
        base.Initialization();
    }

    public void PoolableDestroy()
    {
        throw new System.NotImplementedException();
    }

    public void PoolableReset()
    {
        throw new System.NotImplementedException();
    }

    public void PoolableSetActive(bool isactive = true)
    {
        throw new System.NotImplementedException();
    }
}
