using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum ChunkType
{
    None = 0, 
    Core = 1,
    Base = 2,

}


public enum ShipType
{
    Base,
    LightShip,
    SpeedShip,
    HeavyShip,
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

public class Ship : MonoBehaviour
{




    public int physicalResources;
    public int energyResources;
    public List<string> artifacts;

    public GameObject CorePrefab;
    public GameObject BasePrefab;
    public GameObject container;
    public GameObject chunksParent;
    public GameObject buildingsParent;

    public StateMachine<ShipMovementState> movementState;
    public StateMachine<ShipConditionState> conditionState;


    public Core core;

    public ShipController controller;
    public Chunk[,] ChunkMap { set { _chunkMap = value; } get { return _chunkMap; } }
    private Chunk[,] _chunkMap = new Chunk[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];

    public List<Building> BuildingList { set  {_buildingList = value; } get { return _buildingList; } }
    private List<Building> _buildingList = new List<Building>();

    private ChunkPartMapInfo[,] ShipMapInfo = new ChunkPartMapInfo[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
    private List<BuildingMapInfo> BuildInfoList = new List<BuildingMapInfo>();




    public void LoadRuntimeData(RuntimeData data)
    {
        physicalResources = data.physicalResources;
        energyResources = data.energyResources;
        artifacts = data.artifacts;
        ShipMapInfo = data.ShipMap;
        BuildInfoList = data.BuildingList;
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

        BuildInfoList.Clear();

        for (int i = 0; i < BuildingList.Count; i++)
        {
            
            BuildingMapInfo buildinfo = new BuildingMapInfo(BuildingList[i]);


            BuildInfoList.Add(buildinfo);
        }
    
        GameManager.Instance.gameEntity.runtimeData.physicalResources = physicalResources;
        GameManager.Instance.gameEntity.runtimeData.energyResources = energyResources;
        GameManager.Instance.gameEntity.runtimeData.artifacts = artifacts;
        GameManager.Instance.gameEntity.runtimeData.ShipMap = ShipMapInfo;
        GameManager.Instance.gameEntity.runtimeData.BuildingList = BuildInfoList;
    }

    public void Initialization()
    {
        if(movementState == null)
        {
            movementState = new StateMachine<ShipMovementState>(this.gameObject, false,false);
        }
        if(conditionState == null)
        {
            conditionState = new StateMachine<ShipConditionState>(this.gameObject, false, false);
        }
        movementState.ChangeState(ShipMovementState.Idle);
        conditionState.ChangeState(ShipConditionState.Normal);
    }

    public virtual void InitialShip ()
    {
        GameObject obj = null;
        Vector2Int pos;
        //初始化Chunk
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
                    obj = Instantiate(CorePrefab);
                    _chunkMap[row, colume] = obj.GetComponent<Core>();
                    core = obj.GetComponent<Core>();
                }
                if(ShipMapInfo[row, colume].type == ChunkType.Base)
                {
                    obj = Instantiate(BasePrefab);
                    _chunkMap[row, colume] = obj.GetComponent<Base>();
                }

                _chunkMap[row, colume].shipCoord = ShipMapInfo[row, colume].shipCoord;
                _chunkMap[row, colume].state = ShipMapInfo[row, colume].state;
                _chunkMap[row, colume].isBuildingPiovt = ShipMapInfo[row, colume].isBuildingPiovt;
                _chunkMap[row, colume].isOccupied = ShipMapInfo[row, colume].isOccupied;
                _chunkMap[row, colume].state = UnitState.Normal;

                if (obj != null)
                {
                    obj.transform.parent = chunksParent.transform;
                    //位置和数组的遍历方式相反
                    obj.transform.localPosition = new Vector3(pos.x, pos.y);
                }
            }
        }
        //初始化Building

        BaseUnitConfig unitconfig;
        Vector2Int occupiedarray;
        for (int i = 0; i < BuildInfoList.Count; i++)
        {
            DataManager.Instance.UnitConfigDataDic.TryGetValue(BuildInfoList[i].unitName, out unitconfig);
            obj = Instantiate(unitconfig.Prefab);
            Building building;
            if(obj!= null)
            {

                building = obj.GetComponent<Building>();

                if( building != null)
                {
                    building.direction = BuildInfoList[i].direction;
                    building.pivot = _buildingList[i].pivot;
                    building.occupiedCoords = _buildingList[i].occupiedCoords;
                    building.unitName = _buildingList[i].unitName;
                    building.state = UnitState.Normal;

                    for (int n = 0; n < building.occupiedCoords.Count; n++)
                    {
                        occupiedarray = GameHelper.CoordinateMapToArray(building.occupiedCoords[i], GameGlobalConfig.ShipMapSize);
                        _chunkMap[occupiedarray.x, occupiedarray.y].building = building;
                    }

                    obj.transform.parent = buildingsParent.transform;
                    obj.transform.localPosition = new Vector3(building.pivot.x, building.pivot.y, 0);
                    obj.transform.rotation = Quaternion.Euler(0, 0, -90 * building.direction);

                    _buildingList.Add(building);
                }
            }
        }
     
    }

    public virtual void ResetShip()
    {
      
    }

    // Start is called before the first frame update
    void Start()
    {
       
   
    }

    // Update is called once per frame
    public virtual void  Update()
    {
        HandleMovement();
        HandleRotation();
    }

    public virtual void HandleMovement()
    {
        
    }

    public virtual void HandleRotation()
    {

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
        Vector2Int arraycoord = CoordinateShipToArray(shipcoord);
        return _chunkMap[arraycoord.x, arraycoord.y];
    }

    public Vector2Int GetShipCoordinateFromWorldPos(Vector2 worldpos)
    {
        Vector2 reletivePos = worldpos - transform.position.ToVector2();
         Vector2Int roundPos = reletivePos.Round();

        return roundPos;
    }

    public Vector2 GetWorldPosFromShipCoordinate(Vector2Int shipcoord)
    {
        Vector3 coord = transform.TransformPoint(shipcoord.ToVector3());
        return coord.ToVector2();
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
        //遍历所有的初始Coord
        for (int i = 0; i < openlist.Count; i++)
        {
            // 以初始Coord为中心，进行四项扩散，每个扩散距离+1
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    //在扩散过程中去除自身的扩散情况
                    if ((x == 0 && y !=0) || (x !=0 && y ==0 ))
                    {
                        //获取当前扩散的coord
                        isoverlap = false;
                        tempcoord = new Vector2Int(openlist[i].x + x, openlist[i].y + y);
                        //检测和原始Coordlist是否有重合部分
                        for (int n = 0; n < openlist.Count; n++)
                        {
                            if(tempcoord == openlist[n])
                            {
                                isoverlap = true;
                                break;
                            }
                             //如果和原始Coord没有重合， 检测是否和已经计算过的Coord是否有重合。
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
    //重载ToString方法
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



}
