using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum OwnerType
{
    Player,
    AI,
}

[ShowOdinSerializedPropertiesInInspector]
public class BaseShip : MonoBehaviour
{

    public Core core;
    public BaseController controller;
    public GameObject buildingsParent;


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
}
