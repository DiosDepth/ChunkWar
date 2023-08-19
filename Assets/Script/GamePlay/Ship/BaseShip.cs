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

    public ShipMovementState movementState;
    public ShipConditionState conditionState;
    public ShipStateEvent(ShipMovementState m_movmentstate , ShipConditionState m_conditionstate)
    {
        movementState = m_movmentstate;
        conditionState = m_conditionstate;
    }

    public static ShipStateEvent e;
    public static void Trigger(ShipMovementState m_movmentstate, ShipConditionState m_conditionstate)
    {
        e.movementState = m_movmentstate;
        e.conditionState = m_conditionstate;
        EventCenter.Instance.TriggerEvent<ShipStateEvent>(e);
    }
}

[ShowOdinSerializedPropertiesInInspector]
public class BaseShip : MonoBehaviour,IDropable
{

    public Core core;
    public BaseController controller;
    public GameObject buildingsParent;
    public string deathVFXName = "ExplodeVFX";
    

    public StateMachine<ShipMovementState> movementState;
    public StateMachine<ShipConditionState> conditionState;

    public Chunk[,] ChunkMap { set { _chunkMap = value; } get { return _chunkMap; } }
    [ShowInInspector]
    [ListDrawerSettings(DraggableItems = true)]
    protected Chunk[,] _chunkMap;


    public List<Unit> UnitList { set { _unitList = value; } get { return _unitList; } }
    [ShowInInspector]
    [ListDrawerSettings(DraggableItems = true)]
    protected List<Unit> _unitList = new List<Unit>();

    protected BaseShipConfig baseShipCfg;


    public virtual void Initialization()
    {
        buildingsParent = this.transform.Find("Buildings").gameObject;
        if(buildingsParent == null)
        {
            Debug.LogError(this.gameObject.name + " can't find building parent");
        }
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

    protected virtual void Awake()
    {
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

    public virtual void CreateShip()
    {

    }

    public virtual void ResetShip()
    {

    }
    public virtual void Death()
    {
        conditionState.ChangeState(ShipConditionState.Death);
        ShipStateEvent.Trigger(movementState.CurrentState, conditionState.CurrentState);
    }

    public virtual void Ability()
    {

    }

    protected virtual void OnDestroy()
    {
        for (int i = 0; i < _unitList.Count; i++)
        {
            Destroy(_unitList[i].gameObject);
        }
    }

    public virtual void InitProperty()
    {

    }

    public virtual List<PickableItem> Drop()
    {
        Vector2 pos;
        PickUpData data;
        List<PickableItem> itemlist = new List<PickableItem>() ;
        if(baseShipCfg.DropList?.Count <= 0)
        {
            return null;
        }
        for (int i = 0; i < baseShipCfg.DropList.Count; i++)
        {
            for (int n = 0; n < baseShipCfg.DropList[i].count; n++)
            {
                pos = MathExtensionTools.GetRadomPosFromOutRange(0, baseShipCfg.MapSize.Lager(), this.transform.position.ToVector2());
                DataManager.Instance.PickUpDataDic.TryGetValue(baseShipCfg.DropList[i].pickuptype.ToString(), out data);
                if(data == null)
                {
                    Debug.LogWarning(this.gameObject.name  +" [ " + baseShipCfg.DropList[i].pickuptype.ToString() + " ] Can't find drop data in datamanger");
                    continue;
                }
                PoolManager.Instance.GetObjectSync(data.PrefabPath, true, (obj) =>
                {
                    obj.transform.position = pos;
                    PickableItem item = obj.GetComponent<PickableItem>();
                    itemlist.Add(item);
                });
            }
        }
        return itemlist;
    }
}
