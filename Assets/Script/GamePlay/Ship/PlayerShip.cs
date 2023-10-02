using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.XInput;

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

}

public enum ShipConditionState
{
    Normal,
    Freeze, // can't move and attack
    Immovable, // can't move
    Death,
}

public class PlayerShip : BaseShip
{

    public bool isDebug;


    public GameObject container;
    public SpriteRenderer sprite;
    
    public ShipMainWeapon mainWeapon;

    public CircleCollider2D pickupCollider;

    protected ChunkPartMapInfo[,] ShipMapInfo = new ChunkPartMapInfo[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
    protected List<UnitInfo> UnitInfoList = new List<UnitInfo>();

    
    /// <summary>
    /// �������ڵ�chunk
    /// </summary>
    protected Chunk _coreChunk;

    public PlayerShipConfig playerShipCfg;

    [HideInInspector]
    public Transform shipMapCenter;
    /// <summary>
    /// ��ǰ����Դ
    /// </summary>
    public int TotalEnergy
    {
        get;
        protected set;
    }

    /// <summary>
    /// ��ǰʹ����Դ
    /// </summary>
    public int CurrentUsedEnergy
    {
        get;
        protected set;
    }

    /// <summary>
    /// ��Դ���ذٷֱ�
    /// </summary>
    public float EnergyPercent
    {
        get
        {
            return (CurrentUsedEnergy * 100) / (float)TotalEnergy;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        shipMapCenter = transform.Find("ShipMapCenter");
    }

    public void LoadRuntimeData(ShipMapData data)
    {
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


        RogueManager.Instance.ShipMapData.ShipMap = ShipMapInfo;
        RogueManager.Instance.ShipMapData.UnitList = UnitInfoList;
    }

    public override void Initialization()
    {
        
        var shipCfg = RogueManager.Instance.currentShipSelection.itemconfig as PlayerShipConfig;
        playerShipCfg = shipCfg;
        baseShipCfg = shipCfg;
        base.Initialization();
        ///Init ShipClass
        CreateShip();
        InitShipClass();
        ActiveShipUnit();

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (mainWeapon != null)
        {
            Destroy(mainWeapon.gameObject);
        }

    }




    public override void CreateShip()
    {
        InitProperty();

        //��ʼ��
        _chunkMap = new Chunk[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
        _unitList = new List<Unit>();

        //��ʼ��Chunk
        for (int row = 0; row < ShipMapInfo.GetLength(0); row++)
        {
            for (int colume = 0; colume < ShipMapInfo.GetLength(1); colume++)
            {
                if (ShipMapInfo[row, colume] == null)
                {
                    continue;
                }
                _chunkMap[row, colume] = new Chunk();
                if (ShipMapInfo[row,colume].CorePoint)
                {
                    _coreChunk = _chunkMap[row, colume];
                }

                _chunkMap[row, colume].shipCoord = ShipMapInfo[row, colume].shipCoord;
                _chunkMap[row, colume].state = ShipMapInfo[row, colume].state;
                _chunkMap[row, colume].isBuildingPiovt = ShipMapInfo[row, colume].isBuildingPiovt;
                _chunkMap[row, colume].isOccupied = ShipMapInfo[row, colume].isOccupied;

            }
        }
        //��ʼ��Building
        for (int i = 0; i < UnitInfoList.Count; i++)
        {
            RestoreUnitFromUnitInfo(UnitInfoList[i]);
        }




        //��Ӳ��ԵĴ��롣 ������ΪBuilding�Ĳ��ԡ� ��ɺ�Ӧ��ɾ��

        //GameObject building;
        //BaseUnitConfig unitconfig;
        //DataManager.Instance.UnitConfigDataDic.TryGetValue(1003, out unitconfig);

        //building = Instantiate(unitconfig.Prefab);






        //��ʼ��������
        if ( mainWeapon == null)
        {
            if(RogueManager.Instance.currentWeaponSelection != null)
            {
                var weaponCfg = RogueManager.Instance.currentWeaponSelection.itemconfig as WeaponConfig;
                Vector2Int[] _reletivemap = weaponCfg.GetReletiveCoord().AddToAll(_coreChunk.shipCoord);
                mainWeapon = AddUnit(weaponCfg, _reletivemap, _coreChunk.shipCoord, 0) as ShipMainWeapon;
                mainWeapon.InitCoreData();
                CoreUnits.Add(mainWeapon);
            }
            else
            {
                Debug.LogError("Weapon Null!");
            }
        }

    
        InitPickUpRange();
        movementState.ChangeState(ShipMovementState.Idle);
        conditionState.ChangeState(ShipConditionState.Normal);
        RefreshShipEnergy();
    }



    /// <summary>
    /// ˢ����Դ
    /// </summary>
    public void RefreshShipEnergy()
    {
        TotalEnergy = 0;
        CurrentUsedEnergy = 0;
        for(int i = 0; i < _unitList.Count; i++)
        {
            var unit = _unitList[i];
            TotalEnergy += unit.baseAttribute.EnergyGenerate;
            CurrentUsedEnergy += unit.baseAttribute.EnergyCost;
        }
        var energyCostAdd = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.EnergyCostAdd);
        CurrentUsedEnergy += Mathf.RoundToInt(energyCostAdd);

        TotalEnergy = (int)Mathf.Clamp(TotalEnergy, 0, int.MaxValue);
        RogueManager.Instance.OnEnergyPercentChange?.Invoke(EnergyPercent);
        ShipPropertyEvent.Trigger(ShipPropertyEventType.EnergyChange);
    }

    public virtual void ActiveShipUnit()
    {
        for (int i = 0; i < _unitList.Count; i++)
        {
            _unitList[i].SetUnitProcess(true);
        }
    }



    protected override void Death(UnitDeathInfo info)
    {
        base.Death(info);

        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + deathVFXName, true, (vfx) =>
        {
            vfx.transform.position = this.transform.position;
            vfx.GetComponent<ParticleController>().PoolableSetActive();
            vfx.GetComponent<ParticleController>().PlayVFX(); 
            GameEvent.Trigger(EGameState.EGameState_GameOver);
        });

        //loop all unit to disable it 

        for (int i = 0; i < _unitList.Count; i++)
        {
            _unitList[i].SetUnitProcess(false);
        }
        //disable controller and inputhandle
        controller.SetControllerUpdate(false);
        //

    }

    public override void ResetShip()
    {
        base.ResetShip();
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
        tempunit.UnitID = m_unitconfig.ID;
        tempunit.Initialization(this, m_unitconfig as BaseUnitConfig);
        RogueManager.Instance.AddNewShipUnit(tempunit);
        obj.transform.rotation = Quaternion.Euler(0, 0, -90 * tempunit.direction);
        //���ö�Ӧ�� ChunMap��Ϣ�������Ƿ�ΪPiovt�� �Ƿ�ռ�õȡ�
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
        if(tempunit is ShipAdditionalWeapon)
        {
            _unitList.Add(tempunit);
        }

        RefreshShipEnergy();
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
                tempunit.UnitID = m_unitInfo.UnitID;
                tempunit.UID = m_unitInfo.UID;
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
                    mainWeapon = tempunit as ShipMainWeapon;
                    mainWeapon.Initialization(this, unitconfig);
                    mainWeapon.InitCoreData();
                    CoreUnits.Add(mainWeapon);
                }
                else
                {
                    tempunit.Initialization(this, unitconfig);
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
            temparrycoord = GameHelper.CoordinateMapToArray(m_unit.occupiedCoords[i], GameGlobalConfig.ShipMapSize);
            _chunkMap[temparrycoord.x, temparrycoord.y].isOccupied = false;
            _chunkMap[temparrycoord.x, temparrycoord.y].unit = null;
            _chunkMap[temparrycoord.x, temparrycoord.y].isBuildingPiovt = false;
        }
        RogueManager.Instance.RemoveShipUnit(m_unit);
        UnitList.Remove(m_unit);
        RefreshShipEnergy();
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
                    if(ShipMapInfo[row,colume].CorePoint)
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

    private void OnTriggerStay2D(Collider2D collision)
    {

    }

    private void InitPickUpRange()
    {
        //����PickUp�õ�Collider
        GameObject obj = new GameObject("PickUpCollider");
        obj.transform.SetParent(this.transform);
        obj.layer = LayerMask.NameToLayer("Trigger");
        obj.transform.localPosition = Vector3.zero;
        pickupCollider = obj.AddComponent<CircleCollider2D>();
        RogueManager.Instance.MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.SuckerRange, CalculateNewSuckerRange);
        CalculateNewSuckerRange();
    }

    /// <summary>
    /// ����ʰȡ��Χ
    /// </summary>
    private void CalculateNewSuckerRange()
    {
        var defaultSuckerRange = DataManager.Instance.battleCfg.ShipBaseSuckerRange;
        var minRange = DataManager.Instance.battleCfg.ShipMinSuckerRange;
        var rangeAdd = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.SuckerRange);
        var newSuckerRange = Mathf.Clamp(defaultSuckerRange * (1 + rangeAdd / 100f), minRange, float.MaxValue);
        pickupCollider.radius = newSuckerRange;
    }

    /// <summary>
    /// ��ʼ�������ȼ�����
    /// </summary>
    private void InitShipClass()
    {
        var classCfg = DataManager.Instance.gameMiscCfg.GetShipClassConfig(playerShipCfg.ShipClass);
        if(classCfg != null)
        {
            var mainProperty = RogueManager.Instance.MainPropertyData;
            mainProperty.SetPropertyModifyValue(PropertyModifyKey.ShipParry, PropertyModifyType.Modify, GameGlobalConfig.PropertyModifyUID_ShipClass, classCfg.BaseParry);
            mainProperty.SetPropertyMaxValue(PropertyModifyKey.ShipParry, classCfg.MaxParry);
        }
    }

    public override void PauseGame()
    {
        
    }

    public override void UnPauseGame()
    {
        
    }
}
