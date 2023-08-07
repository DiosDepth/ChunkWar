using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AvaliableAIType
{
    AI_Flyings = 1,
}


[ShowOdinSerializedPropertiesInInspector]
public class AIShip : BaseShip
{
    public AIShipConfig AIShipCfg;

    public AvaliableAIType AIType = AvaliableAIType.AI_Flyings;
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
        //处理Chunk

        AIShipConfig aishipconfig =  DataManager.Instance.GetAIShipConfig((int)AIType);
        AIShipCfg = aishipconfig;
        Vector2Int pos;

        for (int row = 0; row < aishipconfig.Map.GetLength(0); row++)
        {
            for (int colume = 0; colume < aishipconfig.Map.GetLength(1); colume++)
            {
                if (aishipconfig.Map[row, colume] == 0)
                {
                    continue;
                }
                pos = GameHelper.CoordinateArrayToMap(new Vector2Int(row, colume), GameGlobalConfig.ShipMapSize);
                if (aishipconfig.Map[row, colume] == 1)
                {

                    core = new Core();
                    _chunkMap[row, colume] = core;
                }

                if (aishipconfig.Map[row, colume] == 2)
                {

                    _chunkMap[row, colume] = new Base();
                }

                _chunkMap[row, colume].shipCoord = pos;
                _chunkMap[row, colume].state = DamagableState.Normal;
                _chunkMap[row, colume].isBuildingPiovt = false;
                _chunkMap[row, colume].isOccupied = false;
            }
        }
         //处理unit
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
