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
        Initialization();
        CreateShip();


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

    public override void Death()
    {
        base.Death();
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + deathVFXName, true, (vfx) =>
        {
            vfx.transform.position = this.transform.position;
            vfx.GetComponent<ParticleController>().SetActive();
            vfx.GetComponent<ParticleController>().PlayVFX();
            Destroy(this.gameObject);
        });
    }

    public override void InitProperty()
    {
        base.InitProperty();
    }
    public override void CreateShip()
    {
        base.CreateShip();
        _unitList = buildingsParent.GetComponentsInChildren<Unit>().ToList<Unit>();
        BaseUnitConfig unitconfig;
        for (int i = 0; i < _unitList.Count; i++)
        {
            unitconfig = DataManager.Instance.GetUnitConfig(_unitList[i].UnitID);
            _unitList[i].Initialization(this, unitconfig);
            _unitList[i].SetUnitActive(true);
            //_unitList[i].Initialization(this);
            //_unitList[i].SetUnitActive(true);
        }
        
    }
}
