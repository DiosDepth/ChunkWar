using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum OwnerType
{
    Player,
    AI,
}

public struct ShipStateEvent
{
    public BaseShip Ship;
    public ShipMovementState movementState;
    public ShipConditionState conditionState;
    public bool IsPlayer;
    public bool MovementChange;

    public UnitDeathInfo KillInfo;

    public ShipStateEvent(BaseShip ship, UnitDeathInfo info, ShipMovementState m_movmentstate , ShipConditionState m_conditionstate, bool isPlayer, bool movementChange = false)
    {
        Ship = ship;
        movementState = m_movmentstate;
        conditionState = m_conditionstate;
        IsPlayer = isPlayer;
        MovementChange = movementChange;
        KillInfo = info;
    }

    public static ShipStateEvent e;
    public static void Trigger(BaseShip ship, UnitDeathInfo info, ShipMovementState m_movmentstate, ShipConditionState m_conditionstate, bool isPlayer, bool movementChange = false)
    {
        e.Ship = ship;
        e.movementState = m_movmentstate;
        e.conditionState = m_conditionstate;
        e.IsPlayer = isPlayer;
        e.MovementChange = movementChange;
        e.KillInfo = info;
        EventCenter.Instance.TriggerEvent<ShipStateEvent>(e);
    }
}

[ShowOdinSerializedPropertiesInInspector]
public class BaseShip : MonoBehaviour, IDropable, IPauseable
{

    public BaseController controller;
    protected GameObject buildingsParent;
    public string deathVFXName = "ExplodeVFX";
    

    public StateMachine<ShipMovementState> movementState;
    public StateMachine<ShipConditionState> conditionState;

    public Chunk[,] ChunkMap { set { _chunkMap = value; } get { return _chunkMap; } }

    protected Chunk[,] _chunkMap;

    /// <summary>
    /// 核心Units
    /// </summary>
    public List<Unit> CoreUnits;

    public List<Unit> UnitList { set { _unitList = value; } get { return _unitList; } }
    [ShowInInspector]
    [ListDrawerSettings(DraggableItems = true)]
    protected List<Unit> _unitList = new List<Unit>();

    protected BaseShipConfig baseShipCfg;

    public virtual void Initialization()
    {
        GameManager.Instance.RegisterPauseable(this);
        controller = this.GetComponent<BaseController>();
        if(controller == null)
        {
            Debug.LogError(this.gameObject.name + " missing controller component");
        }
        else
        {
            controller.Initialization();
        }

    }
    /// <summary>
    /// 获取所有武器
    /// </summary>
    /// <returns></returns>
    /// 

    public List<T> GetAllShipUnitByType<T>() where T : Unit
    {
        return _unitList.FindAll(x => x is T).ConvertAll(x => x as T);
    }

    /// <summary>
    /// 移除Unit
    /// </summary>
    /// <param name="unit"></param>
    public void RemoveUnit(Unit unit)
    {
        _unitList.Remove(unit);
    }

    public Unit GetUnitBySlotPosition(Vector2Int pos)
    {
        for(int i = 0; i < _unitList.Count; i++)
        {
            var coor = _unitList[i].occupiedCoords;
            if (coor.Contains(pos))
                return _unitList[i];
        }
        return null;
    }

    protected virtual void Awake()
    {
        CoreUnits = new List<Unit>();
        buildingsParent = transform.Find("Buildings").gameObject;
        if (movementState == null)
        {
            movementState = new StateMachine<ShipMovementState>(this.gameObject, false, false);
        }
        if (conditionState == null)
        {
            conditionState = new StateMachine<ShipConditionState>(this.gameObject, false, false);
        }
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    protected virtual void OnDestroy()
    {
        GameManager.Instance.UnRegisterPauseable(this);
        for (int i = 0; i < _unitList.Count; i++)
        {
            Destroy(_unitList[i].gameObject);
        }
    }

    public virtual void CreateShip()
    {

    }

    public virtual void ResetShip()
    {

    }

    public void CheckDeath(Unit coreUnit, UnitDeathInfo info)
    {
        CoreUnits.Remove(coreUnit);
        if(CoreUnits.Count <= 0)
        {
            Death(info);
        }
    }

    protected virtual void Death(UnitDeathInfo info)
    {
        GameManager.Instance.UnRegisterPauseable(this);
        conditionState.ChangeState(ShipConditionState.Death);
        ShipStateEvent.Trigger(this, info, movementState.CurrentState, conditionState.CurrentState, this is PlayerShip);
    }

    public virtual void Ability()
    {

    }



    public virtual void InitProperty()
    {

    }

    public virtual List<PickableItem> Drop()
    {
        List<PickableItem> itemlist = new List<PickableItem>() ;
        if(baseShipCfg.DropList?.Count <= 0)
        {
            return null;
        }
        for (int i = 0; i < baseShipCfg.DropList.Count; i++)
        {
            var dropInfo = baseShipCfg.DropList[i];
            var dropRate = GameHelper.CalculateDropRate(dropInfo.dropRate);
            bool isDrop = Utility.RandomResultWithOne(0, dropRate);
            if (!isDrop)
                continue;

            if (dropInfo.pickuptype == AvaliablePickUp.WastePickup)
            {
                itemlist.AddRange(HandleWasteDropPickUp(dropInfo));
            }
            else if (dropInfo.pickuptype == AvaliablePickUp.Wreckage)
            {
                var wreckageDrop = HandleWreckageDropPickUp(dropInfo);
                if(wreckageDrop != null)
                {
                    itemlist.Add(wreckageDrop);
                }
            }
        }
        return itemlist;
    }

    /// <summary>
    /// 装备残骸掉落
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private PickableItem HandleWreckageDropPickUp(DropInfo info)
    {
        var dropRateCfg = baseShipCfg.UnitDropRate;
        List<GeneralRarityRandomItem> randomLst = new List<GeneralRarityRandomItem>();
        foreach(var item in dropRateCfg)
        {
            GeneralRarityRandomItem random = new GeneralRarityRandomItem
            {
                Rarity = item.Key,
                Weight = (int)item.Value * 10
            };
            randomLst.Add(random);
        }

        var randomResult = Utility.GetRandomList<GeneralRarityRandomItem>(randomLst, 1);
        if(randomResult.Count == 1)
        {
            var result = randomResult[0];

            var pickUpData = DataManager.Instance.GetWreckagePickUpData(result.Rarity);
            if (pickUpData == null)
                return null;

            PickUpWreckage item = null;

            PoolManager.Instance.GetObjectSync(pickUpData.PrefabPath, true, (obj) =>
            {
                obj.transform.position = GetDropPosition();
                item = obj.GetComponent<PickUpWreckage>();
                item.DropRarity = pickUpData.Rarity;
                item.EXPAdd = pickUpData.EXPAdd;
            });
            return item;
        }
        return null;
    }

    /// <summary>
    /// 处理残骸掉落
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private List<PickableItem> HandleWasteDropPickUp(DropInfo info)
    {
        List<PickableItem> outLst = new List<PickableItem>();

        ///Calculate DropCount
        var dropCountAdd = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.EnemyDropCountPercent);
        var dropRatio = Mathf.Clamp(dropCountAdd / 100f + 1, 0, float.MaxValue);
        float dropCount = info.count * dropRatio;
        var dropResult = GameHelper.GeneratePickUpdata(dropCount);

        foreach(var result in dropResult)
        {
            int count = result.Value;
            var data = result.Key;
            ///Drop
            for (int i = 0; i < count; i++) 
            {
                PoolManager.Instance.GetObjectSync(data.PrefabPath, true, (obj) =>
                {
                    obj.transform.position = GetDropPosition();
                    PickUpWaste item = obj.GetComponent<PickUpWaste>();
                    item.WasteGain = data.CountRef;
                    item.EXPGain = data.EXPAdd;
                    outLst.Add(item);
                });
            }
        }
        return outLst;
    }

    private Vector2 GetDropPosition()
    {
        float MaxSize = Mathf.Max(baseShipCfg.MapSize.Lager(), 2);
        Vector2 shipPos = transform.position.ToVector2();
        return MathExtensionTools.GetRadomPosFromOutRange(0.5f, MaxSize, shipPos);
    }

    public virtual void GameOver()
    {
        for (int i = 0; i < CoreUnits.Count; i++)
        {
            CoreUnits[i].GameOver();
        }

    }
    public virtual void PauseGame()
    {
      
    }

    public virtual void UnPauseGame()
    {

    }
}
