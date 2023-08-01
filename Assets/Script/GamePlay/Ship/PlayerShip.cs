using System.Collections.Generic;
using System.Linq;
using UnityEngine;




public enum ShipMainProperty
{
    Control,
    DamagePercnet,
    Critial,
    Luck,
    Range,
}

public enum ShipType
{
    Base,
    Lancer,
    Phoenix,
    Guardian,
    Ranger,
    Zealot,
    Immortal,
    Archon,
}


public enum InventoryEventType
{
    Checkin,
    CheckOut,
    Clear,
}


public enum ShipMovementState
{
    Idle,
    Move,
    Death,
}

public enum ShipConditionState
{
    Normal,
    Attack,
}

public class PlayerShip : BaseShip
{

    public bool isDebug;
    public int physicalResources;
    public int energyResources;



    public GameObject container;
    public SpriteRenderer sprite;
    public Transform shipMapCenter;
    public GameObject buildingsParent;

    public StateMachine<ShipMovementState> movementState;
    public StateMachine<ShipConditionState> conditionState;

    public CircleCollider2D pickupCollider;



    private ChunkPartMapInfo[,] ShipMapInfo = new ChunkPartMapInfo[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
    private List<UnitInfo> UnitInfoList = new List<UnitInfo>();

    private Dictionary<ShipMainProperty, ChangeValue<float>> _mainPropertyDic;


    public void LoadRuntimeData(RuntimeData data)
    {
        physicalResources = data.physicalResources;
        energyResources = data.energyResources;

        ShipMapInfo = data.ShipMap;
        UnitInfoList = data.UnitList;
    }


    public void SaveRuntimeData()
    {
        for (int row = 0; row < ShipMapInfo.GetLength(0); row++)
        {
            for (int colume = 0; colume < ShipMapInfo.GetLength(1); colume++)
            {
                if(ChunkMap[row, colume] == null)
                {
                    ShipMapInfo[row, colume] = null;
                    continue;
                }

                if( ChunkMap[row,colume].GetType() ==  typeof(Core) )
                {
                    ShipMapInfo[row, colume].type = ChunkType.Core;
                }
                else
                {
                    ShipMapInfo[row, colume].type = ChunkType.Base;
                }
                ShipMapInfo[row, colume].shipCoord = ChunkMap[row, colume].shipCoord;
                ShipMapInfo[row, colume].isOccupied = ChunkMap[row, colume].isOccupied;
                ShipMapInfo[row, colume].isBuildingPiovt = ChunkMap[row, colume].isBuildingPiovt;
                ShipMapInfo[row, colume].state = ChunkMap[row, colume].state;
            }
        }

        UnitInfoList.Clear();

        for (int i = 0; i < UnitList.Count; i++)
        {
            
            UnitInfo buildinfo = new UnitInfo(UnitList[i]);


            UnitInfoList.Add(buildinfo);
        }
    
        GameManager.Instance.gameEntity.runtimeData.physicalResources = physicalResources;
        GameManager.Instance.gameEntity.runtimeData.energyResources = energyResources;
        GameManager.Instance.gameEntity.runtimeData.ShipMap = ShipMapInfo;
        GameManager.Instance.gameEntity.runtimeData.UnitList = UnitInfoList;
    }

    public override void Initialization()
    {
        base.Initialization();


        if (movementState == null)
        {
            movementState = new StateMachine<ShipMovementState>(this.gameObject, false,false);
        }
        if(conditionState == null)
        {
            conditionState = new StateMachine<ShipConditionState>(this.gameObject, false, false);
        }
        //��ʼ��Ship��һЩ����
        RogueManager.Instance.AddNewShipPlug((RogueManager.Instance.currentShipSelection.itemconfig as ShipConfig).CorePlugID);


    }

    public override void CreateShip ()
    {
        //base.CreateShip();
        InitProperty();

        //��ʼ��
        _chunkMap = new Chunk[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
        _unitList = new List<Unit>();
        Vector2Int pos;
        //��ʼ��Chunk
        for (int row = 0; row < ShipMapInfo.GetLength(0); row++)
        {
            for (int colume = 0; colume < ShipMapInfo.GetLength(1); colume++)
            {
                if (ShipMapInfo[row, colume] == null)
                {
                    continue;
                }
                pos = GameHelper.CoordinateArrayToMap(new Vector2Int(row, colume), GameGlobalConfig.ShipMapSize);
                if (ShipMapInfo[row,colume].type == ChunkType.Core)
                {

                    core = new Core();
                   _chunkMap[row, colume] = core;
                }

                if(ShipMapInfo[row, colume].type == ChunkType.Base)
                {

                    _chunkMap[row, colume] = new Base();
                }

                _chunkMap[row, colume].shipCoord = ShipMapInfo[row, colume].shipCoord;
                _chunkMap[row, colume].state = ShipMapInfo[row, colume].state;
                _chunkMap[row, colume].isBuildingPiovt = ShipMapInfo[row, colume].isBuildingPiovt;
                _chunkMap[row, colume].isOccupied = ShipMapInfo[row, colume].isOccupied;
                _chunkMap[row, colume].state = DamagableState.Normal;
            }
        }
        //��ʼ��Building
        for (int i = 0; i < UnitInfoList.Count; i++)
        {
            RestoreUnitFromUnitInfo(UnitInfoList[i]);
        }
        //��ʼ��������
        if( mainWeapon == null)
        {
            var weaponID = (RogueManager.Instance.currentShipSelection.itemconfig as ShipConfig).MainWeaponID;
            BaseUnitConfig weaponconfig;
            DataManager.Instance.UnitConfigDataDic.TryGetValue(weaponID, out weaponconfig);
            Vector2Int[] _reletivemap = weaponconfig.GetReletiveCoord().AddToAll(core.shipCoord);
            mainWeapon = AddUnit(weaponconfig, _reletivemap, core.shipCoord, 0) as Weapon;
            mainWeapon.Initialization(this, weaponconfig as WeaponConfig);
        }

        //����PickUp�õ�Collider
        GameObject obj = new GameObject("PickUpCollider");
        obj.transform.SetParent(this.transform);
        obj.layer = LayerMask.NameToLayer("Trigger");
        obj.transform.localPosition = Vector3.zero;
        pickupCollider = obj.AddComponent<CircleCollider2D>();
        pickupCollider.radius = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.SuckerRange);

        controller.Initialization();
        movementState.ChangeState(ShipMovementState.Idle);
        conditionState.ChangeState(ShipConditionState.Normal);
    }

    public virtual void ResetShip()
    {
      
    }

    // Start is called before the first frame update
    protected override void Start()
    {
       
   
    }

    // Update is called once per frame
    protected override void Update()
    {

    }

    public virtual void OnDestroy()
    {
        Destroy(mainWeapon.gameObject);
    }

    public virtual void Death()
    {

    }

    public virtual void Ability()
    {

    }



    public void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        
    }


    //---------------------------------------------------------------------------------------Tools

    public Chunk GetChunkFromShipCoordinate(Vector2Int shipcoord)
    {
        if(!IsUnderShipSize(shipcoord))
        {
            return null;
        }
       
        Vector2Int arraycoord = GameHelper.CoordinateMapToArray(shipcoord, GameGlobalConfig.ShipMapSize);
        return _chunkMap[arraycoord.x, arraycoord.y];
    }



    public Vector2Int CoordinateArrayToShip(Vector2Int arraycoord)
    {
        return new Vector2Int(arraycoord.y - GameGlobalConfig.ShipMapSize, GameGlobalConfig.ShipMapSize - arraycoord.x );
    }

    public Vector2Int CoordinateShipToArray(Vector2Int shipcoord)
    {
        return new Vector2Int(GameGlobalConfig.ShipMapSize - shipcoord.y, GameGlobalConfig.ShipMapSize + shipcoord.x);
    }

    public Vector2Int WorldPosToCoordinateArray(Vector2 worldpos)
    {
        return new Vector2Int();
    }

    public Vector2Int[] GerRangeByShipCoord(Vector2Int shipcoord, int range = 1)
    {
        List<Vector2Int> list = new List<Vector2Int>();
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if ( Mathf.Abs(x) + Mathf.Abs(y) <= range)
                {
                    if(x != 0 || y != 0)
                    {
                        list.Add(new Vector2Int(x + shipcoord.x, y + shipcoord.y));
                    }
              
                }
            }
        }
        return list.ToArray();
    }

    public Vector2Int[] GetOutLineByShipcoordList(Vector2Int[] shipcoord)
    {
        List<Vector2Int> openlist = new List<Vector2Int>();
        List<Vector2Int> closelist = new List<Vector2Int>();
        openlist = shipcoord.ToList();
        Vector2Int tempcoord;
        bool isoverlap = false;
        //�������еĳ�ʼCoord
        for (int i = 0; i < openlist.Count; i++)
        {
            // �Գ�ʼCoordΪ���ģ�����������ɢ��ÿ����ɢ����+1
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    //����ɢ������ȥ���������ɢ���
                    if ((x == 0 && y !=0) || (x !=0 && y ==0 ))
                    {
                        //��ȡ��ǰ��ɢ��coord
                        isoverlap = false;
                        tempcoord = new Vector2Int(openlist[i].x + x, openlist[i].y + y);
                        //����ԭʼCoordlist�Ƿ����غϲ���
                        for (int n = 0; n < openlist.Count; n++)
                        {
                            if(tempcoord == openlist[n])
                            {
                                isoverlap = true;
                                break;
                            }
                             //�����ԭʼCoordû���غϣ� ����Ƿ���Ѿ��������Coord�Ƿ����غϡ�
                        }

                        if(!isoverlap)
                        {
                            for (int n = 0; n < closelist.Count; n++)
                            {
                                if (tempcoord == closelist[n])
                                {
                                    isoverlap = true;
                                    break;
                                }
                            }
                        }

                        if(!isoverlap)
                        {
                            closelist.Add(tempcoord);
                        }
                    }
                }
            }
        }

        return closelist.ToArray();
    }

    public Unit AddUnit(BaseConfig m_unitconfig, Vector2Int[] m_unitmap, Vector2Int m_poscoord, int m_direction)
    {
        GameObject obj;
        Vector2Int buildarray;
        obj = Instantiate(m_unitconfig.Prefab);
        obj.transform.parent = buildingsParent.transform;
        obj.transform.localPosition = new Vector3(m_poscoord.x + shipMapCenter.localPosition.x, m_poscoord.y + shipMapCenter.localPosition.y);
        Unit tempunit = obj.GetComponent<Unit>();
        tempunit.direction = m_direction;
        obj.transform.rotation = Quaternion.Euler(0, 0, -90 * tempunit.direction);
        //����Building��Prefab���ҹ���ź�


        //���ö�Ӧ��ChunMap��Ϣ�������Ƿ�ΪPiovt�� �Ƿ�ռ�õȡ�
        if (m_unitmap.Length > 0)
        {
            for (int i = 0; i < m_unitmap.Length; i++)
            {
                buildarray = GameHelper.CoordinateMapToArray(m_unitmap[i], GameGlobalConfig.ShipMapSize);


                ChunkMap[buildarray.x, buildarray.y].unit = tempunit;

                if (m_unitmap[i] == m_poscoord)
                {
                    ChunkMap[buildarray.x, buildarray.y].isBuildingPiovt = true;

                    tempunit.pivot = m_unitmap[i];
                }
                ChunkMap[buildarray.x, buildarray.y].isOccupied = true;

                tempunit.occupiedCoords.Add(m_unitmap[i]);
            }
        }

        _unitList.Add(tempunit);
        return tempunit;
    }

    public void RestoreUnitFromUnitInfo(UnitInfo m_unitInfo)
    {
        BaseUnitConfig unitconfig;
        Vector2Int occupiedarray;
        GameObject obj;
        Unit tempunit;

        DataManager.Instance.UnitConfigDataDic.TryGetValue(m_unitInfo.UnitID, out unitconfig);
        obj = Instantiate(unitconfig.Prefab);


        if (obj != null)
        {
            tempunit = obj.GetComponent<Unit>();

            if (tempunit != null)
            {
                tempunit.direction = m_unitInfo.direction;
                tempunit.pivot = m_unitInfo.pivot;
                tempunit.occupiedCoords = m_unitInfo.occupiedCoords;
                tempunit.state = DamagableState.Normal;


                 
                for (int n = 0; n < tempunit.occupiedCoords.Count; n++)
                {
                    occupiedarray = GameHelper.CoordinateMapToArray(tempunit.occupiedCoords[n], GameGlobalConfig.ShipMapSize);
                    _chunkMap[occupiedarray.x, occupiedarray.y].unit = tempunit;
                }


                if(unitconfig.unitType == UnitType.MainWeapons)
                {
                    mainWeapon = tempunit as Weapon;
                }

                obj.transform.parent = buildingsParent.transform;
                obj.transform.localPosition = new Vector3(tempunit.pivot.x + shipMapCenter.localPosition.x, tempunit.pivot.y + shipMapCenter.localPosition.y, 0);
                obj.transform.rotation = Quaternion.Euler(0, 0, -90 * tempunit.direction);

                _unitList.Add(tempunit);
            }
        }
    }

    public void RemoveUnit(Unit m_unit)
    {
        if (!UnitList.Contains(m_unit)) { return; }

        Vector2Int temparrycoord;
        for (int i = 0; i < m_unit.occupiedCoords.Count; i++)
        {
            temparrycoord = GameHelper.CoordinateMapToArray(m_unit.occupiedCoords[i], GameGlobalConfig.UnitMapSize);
            _chunkMap[temparrycoord.x, temparrycoord.y].isOccupied = false;
            _chunkMap[temparrycoord.x, temparrycoord.y].unit = null;
            _chunkMap[temparrycoord.x, temparrycoord.y].isBuildingPiovt = false;
        }

        UnitList.Remove(m_unit);
        GameObject.Destroy(m_unit.gameObject);
    }

    public void RemoveUnit(Chunk m_chunk)
    {
        if(m_chunk == null || !m_chunk.isOccupied || m_chunk.unit == null) { return; }

        RemoveUnit(m_chunk.unit);

    }


    public bool IsUnderShipSize(Vector2Int shipcoord)
    {
        if(shipcoord.x.IsInRange(-GameGlobalConfig.ShipMapSize, GameGlobalConfig.ShipMapSize) && shipcoord.y.IsInRange(-GameGlobalConfig.ShipMapSize, GameGlobalConfig.ShipMapSize))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    //����ToString����
    public override string ToString()
    {
        string line = "";
        for (int x = 0; x < ShipMapInfo.GetLength(0); x++)
        {
            line += "{ ";
            for (int y = 0; y < ShipMapInfo.GetLength(1); y++)
            {
                if (y == ShipMapInfo.GetLength(1) - 1)
                {
                    line += ShipMapInfo[x, y] + "} \n";
                }
                else
                {
                    line += ShipMapInfo[x, y] + ",";
                }

            }
        }
        return line;
    }

    public void OnDrawGizmos()
    {
        if (!isDebug) { return; }
        for (int row = 0; row < ShipMapInfo.GetLength(0); row++)
        {
            for (int colume = 0; colume < ShipMapInfo.GetLength(1); colume++)
            {
                if(ShipMapInfo[row, colume] != null)
                {
                    if(ShipMapInfo[row,colume].type == ChunkType.Core)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(GameHelper.GetWorldPosFromReletiveCoord(shipMapCenter, ShipMapInfo[row, colume].shipCoord), Vector3.one);
                    }
                    else
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawCube(GameHelper.GetWorldPosFromReletiveCoord(shipMapCenter, ShipMapInfo[row, colume].shipCoord), Vector3.one);
                    }
                }
                if (_chunkMap[row, colume] != null)
                {
                    if(_chunkMap[row,colume].isOccupied == true)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawCube(GameHelper.GetWorldPosFromReletiveCoord(shipMapCenter, ShipMapInfo[row, colume].shipCoord), Vector3.one);
                    }
                }
            }


        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "PickUps")
        {
            collision.GetComponentInParent<PickableItem>().PickUp(this.gameObject);
        }
    }

    private void InitProperty()
    {
        _mainPropertyDic = new Dictionary<ShipMainProperty, ChangeValue<float>>();

    }
}
