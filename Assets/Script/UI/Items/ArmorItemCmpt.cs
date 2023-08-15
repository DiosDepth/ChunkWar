using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorItemCmpt : MonoBehaviour, IPoolable
{

    private Transform FillTrans;

    public void Awake()
    {
        FillTrans = transform.Find("Fill");
    }

    public void SetActive(bool active)
    {
        FillTrans.SafeSetActive(active);
    }

    public void PoolableDestroy()
    {

    }

    public void PoolableReset()
    {

    }

    public void PoolableSetActive(bool isactive = true)
    {

    }
}
