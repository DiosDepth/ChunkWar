using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseShip : MonoBehaviour
{
    public Core core;
    public Weapon mainWeapon;

    public ShipController controller;
    public GameObject buildingsParent;


    public StateMachine<ShipMovementState> movementState;
    public StateMachine<ShipConditionState> conditionState;

    public Chunk[,] ChunkMap { set { _chunkMap = value; } get { return _chunkMap; } }
    protected Chunk[,] _chunkMap;

    public List<Unit> UnitList { set { _unitList = value; } get { return _unitList; } }
    protected List<Unit> _unitList = new List<Unit>();

    public virtual void Initialization()
    {

    }

    protected virtual void Awake()
    {

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

    public virtual void OnDestroy()
    {

    }
}
