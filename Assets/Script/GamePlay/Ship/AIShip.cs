using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AvaliableAIType
{
    AI_Flyings = 1,
    AI_Fighter = 2,
}


[ShowOdinSerializedPropertiesInInspector]
public class AIShip : BaseShip,IPoolable
{
    public AIShipConfig AIShipCfg;

    public AvaliableAIType AIType = AvaliableAIType.AI_Flyings;
    public override void Initialization()
    {
        base.Initialization();
        CreateShip();
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

    public override void Death()
    {
        base.Death();
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + deathVFXName, true, (vfx) =>
        {
            vfx.transform.position = this.transform.position;
            vfx.GetComponent<ParticleController>().PoolableSetActive(true);
            vfx.GetComponent<ParticleController>().PlayVFX();
            PoolableDestroy();
        });

    }

    public override void InitProperty()
    {
        base.InitProperty();
    }
    public override void CreateShip()
    {
        base.CreateShip();


        //初始化
        _chunkMap = new Chunk[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
        _unitList = new List<Unit>();

        //处理Chunk

        AIShipConfig aishipconfig =  DataManager.Instance.GetAIShipConfig((int)AIType);
        AIShipCfg = aishipconfig;
        baseShipCfg = aishipconfig;
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
        _unitList = buildingsParent.GetComponentsInChildren<Unit>(true).ToList<Unit>();
        BaseUnitConfig unitconfig;
        for (int i = 0; i < _unitList.Count; i++)
        {
            unitconfig = DataManager.Instance.GetUnitConfig(_unitList[i].UnitID);
            _unitList[i].gameObject.SetActive(true);
           
            _unitList[i].Initialization(this, unitconfig);
            _unitList[i].SetUnitProcess(true);
            //_unitList[i].Initialization(this);
            //_unitList[i].SetUnitActive(true);
        }
    }

    public void PoolableReset()
    {
        
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }

}
