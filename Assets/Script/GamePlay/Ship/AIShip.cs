using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ShowOdinSerializedPropertiesInInspector]
public class AIShip : BaseShip
{

    public override void Initialization()
    {
        base.Initialization();
    }

    protected override void Awake()
    {
        base.Awake();
    }
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
    public override void CreateShip()
    {
        base.CreateShip();
        _unitList = buildingsParent.GetComponentsInChildren<Unit>().ToList<Unit>();

        for (int i = 0; i < _unitList.Count; i++)
        {
            //_unitList[i].Initialization(this);
            //_unitList[i].SetUnitActive(true);
        }
        
    }
}
