using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitUpgradeNodeCmpt : MonoBehaviour
{
    private Transform emptyTrans;
    private Transform fillTrans;

    public void Awake()
    {
        emptyTrans = transform.Find("Empty");
        fillTrans = transform.Find("Fill");
    }

    public void SetVisiable(bool visiable)
    {
        transform.SafeSetActive(visiable);
    }

    public void SetUp(bool hasValue)
    {
        emptyTrans.SafeSetActive(!hasValue);
        fillTrans.SafeSetActive(hasValue);
        SetVisiable(true);
    }
}
