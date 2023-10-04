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

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
    protected override void Death(UnitDeathInfo info)
    {
        base.Death(info);
        DestroyAIShipBillBoard();
        LevelManager.Instance.pickupList.AddRange(Drop());
        AIManager.Instance.RemoveAI(this);
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


        //��ʼ��
        _chunkMap = new Chunk[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
        _unitList = new List<Unit>();

        //����Chunk

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
                _chunkMap[row, colume] = new Chunk();
                _chunkMap[row, colume].shipCoord = pos;
                _chunkMap[row, colume].state = DamagableState.Normal;
                _chunkMap[row, colume].isBuildingPiovt = false;
                _chunkMap[row, colume].isOccupied = false;
            }
        }
         //����unit
        _unitList = buildingsParent.GetComponentsInChildren<Unit>(true).ToList<Unit>();
        BaseUnitConfig unitconfig;
        for (int i = 0; i < _unitList.Count; i++)
        {
            var unit = _unitList[i];
            unitconfig = DataManager.Instance.GetUnitConfig(_unitList[i].UnitID);
            unit.gameObject.SetActive(true);

            unit.Initialization(this, unitconfig);
            unit.SetUnitProcess(true);
            if (unit.IsCoreUnit)
            {
                CoreUnits.Add(unit);
            }
        }
        InitAIShipBillBoard();
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

    private void InitAIShipBillBoard()
    {
        if (!AIShipCfg.ShowHPBillboard)
            return;

        if(AIShipCfg.BillboardType == EnemyHPBillBoardType.Boss_UI)
        {
            RogueEvent.Trigger(RogueEventType.RegisterBossHPBillBoard, this);
        }
    }

    private void DestroyAIShipBillBoard()
    {
        if (!AIShipCfg.ShowHPBillboard)
            return;

        if(AIShipCfg.BillboardType == EnemyHPBillBoardType.Boss_UI)
        {
            RogueEvent.Trigger(RogueEventType.RemoveBossHPBillBoard, this);
        }
    }

    public override void GameOver()
    {
        base.GameOver();
    }
    public override void PauseGame()
    {
        base.PauseGame();
    }

    public override void UnPauseGame()
    {
        base.UnPauseGame();
    }
}
