using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public struct InventoryEvent
{
    public InventoryEventType type;
    public string name;
    public UnitType storeunittype;
    public InventoryEvent(InventoryEventType m_type, UnitType m_storeunittype, string m_name)
    {
        type = m_type;
        storeunittype = m_storeunittype;
        name = m_name;
    }
    public static InventoryEvent e;
    public static void Trigger(InventoryEventType m_type, UnitType m_storeunittype, string m_name)
    {
        e.type = m_type;
        e.storeunittype = m_storeunittype;
        e.name = m_name;
        EventCenter.Instance.TriggerEvent<InventoryEvent>(e);
    }
}

[System.Serializable]
public class InventoryItem
{
    public bool iscountless;
    public int count;
    public BaseUnitConfig itemconfig;

    public InventoryItem(BaseUnitConfig m_item, int m_count = 1, bool m_iscountless = false)
    {
        iscountless = m_iscountless;
        count = m_count;
        itemconfig = m_item;
    }
}

[System.Serializable]
public class Inventory
{

    public Dictionary<string, InventoryItem> storeDic = new Dictionary<string, InventoryItem>();


    public void CheckIn(InventoryItem m_item)
    {
        if(storeDic.ContainsValue(m_item))
        {
            return;
        }
        storeDic.Add(m_item.itemconfig.UnitName, m_item);
        //发送入库事件
        InventoryEvent.Trigger(InventoryEventType.Checkin,m_item.itemconfig.Type,m_item.itemconfig.UnitName);
    }

    public InventoryItem CheckOut(string m_name)
    {
        InventoryItem item;
        storeDic.TryGetValue(m_name, out item);
      

        //发出库事件
        InventoryEvent.Trigger(InventoryEventType.CheckOut, item.itemconfig.Type, item.itemconfig.UnitName);

        storeDic.Remove(m_name);
        return item;
    }

    public InventoryItem Peek(string m_name)
    {
        InventoryItem item;
        storeDic.TryGetValue(m_name, out item);
       
        return item;
    }
}

public enum ShipBuilderInventoryOperationType
{
    BuildingSelect,
    BuildingUnSelect,
    BuildingRemove,
   
}

public struct ShipBuilderInventoryOperationEvent
{
    public ShipBuilderInventoryOperationType type;
    public UnitType unitytype;
    public int index;
    public string name;
    public ShipBuilderInventoryOperationEvent(ShipBuilderInventoryOperationType m_type, UnitType m_unitytype, int m_index, string m_name)
    {
        type = m_type;
        unitytype = m_unitytype;
        index = m_index;
        name = m_name;
    }
    public static ShipBuilderInventoryOperationEvent e;
    public static void Trigger(ShipBuilderInventoryOperationType m_type, UnitType m_unitytype, int m_index, string m_name)
    {
        e.type = m_type;
        e.unitytype = m_unitytype;
        e.index = m_index;
        e.name = m_name;
        EventCenter.Instance.TriggerEvent<ShipBuilderInventoryOperationEvent>(e);
    }
}

public class ShipBuilder : MonoBehaviour,EventListener<ShipBuilderInventoryOperationEvent>
{
    public static ShipBuilder instance;
    public enum EditorMode
    {
      
        ShipEditorMode,
        BuildingEditorMode,
    }

    public int brickCount = 100;
    public int brickConsumePerChunk = 5;

    public EditorMode editorMode = EditorMode.ShipEditorMode;

    public GameObject CorePrefab;
    public GameObject BasePrefab;

    public Ship editorShip;
    public ShipBuilderBrush editorBrush;
    public Color brushValidColor;
    public Color brushInvalidColor;





    [Header("Inventory")]

    public Inventory chunkPartInventory;
    public Inventory buildingInventory;


    public Building currentBuilding;
    public Chunk currentChunk;

    //select item from both chunkPartInventory and buildingInventory
    public InventoryItem currentInventoryItem;





    [Header("CameraSettings")]
    public Vector3 cameraOffset = Vector3.zero;
    public float cameraSize = 10;


    private bool _isInitial;
    private Vector2 _mousePos;
    private Vector2 _mouseWorldPos;
    private Ray _mouseRay;
    private int _itemDirection = 0;
    private bool _isValidPos = false;


    private Vector2Int _pointedShipCoord;
    private Vector2Int[] _reletiveCoord;
    private Vector2Int[] _outLineShipCood;


    private Vector2Int[] _tempmap;
    private Vector2Int _temparray;

    // Start is called before the first frame update

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        StartCoroutine(DataManager.Instance.LoadAllData(() =>
        {
            //Test
            BaseUnitConfig tempdata;

            
            DataManager.Instance.UnitConfigDataDic.TryGetValue("BaseBuilding", out tempdata);

            buildingInventory.CheckIn(new InventoryItem(tempdata, 1, true));
            DataManager.Instance.UnitConfigDataDic.TryGetValue("LightAutoCannon", out tempdata);
            buildingInventory.CheckIn(new InventoryItem(tempdata, 1, true));
            DataManager.Instance.UnitConfigDataDic.TryGetValue("DirectionalSmoothboreCannon", out tempdata);
            buildingInventory.CheckIn(new InventoryItem(tempdata, 1, true));

            DataManager.Instance.UnitConfigDataDic.TryGetValue("ChunkPart_001", out tempdata);
            chunkPartInventory.CheckIn(new InventoryItem(tempdata, 1, false));
            DataManager.Instance.UnitConfigDataDic.TryGetValue("ChunkPart_002", out tempdata);
            chunkPartInventory.CheckIn(new InventoryItem(tempdata, 1, false));


            UIManager.Instance.ShowUI<ShipBuilderHUD>("ShipBuilderHUD", E_UI_Layer.Mid,this ,(panel) =>
            {
                panel.Initialization();

                EventRegister.EventStartListening<ShipBuilderInventoryOperationEvent>(this);
                InputDispatcher.Instance.Action_GamePlay_Point += HandleBuildMouseMove;
                InputDispatcher.Instance.Action_GamePlay_LeftClick += HandleBuildOperation;
                InputDispatcher.Instance.Action_GamePlay_RightClick += HandleRemoveOperation;
                InputDispatcher.Instance.Action_GamePlay_MidClick += HandleBuildRotation;
                LoadingShip(GameManager.Instance.gameEntity.runtimeData);

                CameraManager.Instance.ChangeVCameraLookAtTarget(transform);
                CameraManager.Instance.ChangeVCameraFollowTarget(transform);
                CameraManager.Instance.vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.x = cameraOffset.x;
                CameraManager.Instance.vcam.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.x = cameraOffset.x;
                CameraManager.Instance.vcam.m_Lens.OrthographicSize = cameraSize;

                _isInitial = true;
            });

        }));

        editorBrush.Initialization();
        Debug.Log("Start ShipBuilder");

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        EventRegister.EventStopListening<ShipBuilderInventoryOperationEvent>(this);
        InputDispatcher.Instance.Action_GamePlay_Point -= HandleBuildMouseMove;
        InputDispatcher.Instance.Action_GamePlay_LeftClick -= HandleBuildOperation;
        InputDispatcher.Instance.Action_GamePlay_RightClick -= HandleRemoveOperation;
        InputDispatcher.Instance.Action_GamePlay_MidClick -= HandleBuildRotation;

        UIManager.Instance.HiddenUI("ShipBuilderHUD");
    }

    public void LoadingShip(RuntimeData runtimedata)
    {

        if(editorShip == null)
        {
            var obj = ResManager.Instance.Load<GameObject>(GameManager.Instance.playerPrefabPath);
            editorShip = obj.GetComponentInChildren<Ship>();
        }        

        if (runtimedata == null)
        {
            editorShip.ResetShip();
        }
        else
        {
            editorShip.LoadRuntimeData(runtimedata);
        }
        editorShip.InitialShip();
    }


    public void SaveShip()
    {
        editorShip.SaveRuntimeData();
    }

    public void HandleBuildMouseMove(InputAction.CallbackContext context)
    {
        if(!_isInitial)
        {
            return;
        }
        switch (context.phase)
        {
            case InputActionPhase.Disabled:
                break;
            case InputActionPhase.Waiting:
                break;
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Performed:
                _mousePos = context.ReadValue<Vector2>();
                _mouseWorldPos = CameraManager.Instance.mainCamera.ScreenToWorldPoint(new Vector3(_mousePos.x, _mousePos.y, 0));
                _pointedShipCoord = editorShip.GetShipCoordinateFromWorldPos(_mouseWorldPos);

                currentChunk = editorShip.GetChunkFromShipCoordinate(_pointedShipCoord);

                switch (editorMode)
                {
                    case EditorMode.ShipEditorMode:


                        if (currentInventoryItem.itemconfig == null)
                        {
                            return;
                        }

                        _isValidPos = false;

                        _tempmap = _reletiveCoord.AddToAll(_pointedShipCoord);

                        //判断鼠标是否在建造范围内

                        for (int i = 0; i < _tempmap.Length; i++)
                        {

                            if (Mathf.Abs(_pointedShipCoord.x) > GameGlobalConfig.ShipMapSize || Mathf.Abs(_pointedShipCoord.y) > GameGlobalConfig.ShipMapSize)
                            {
                                editorBrush.gameObject.SetActive(false);
                                return;
                            }
                            else
                            {
                                editorBrush.gameObject.SetActive(true);
                            }

                            if (Mathf.Abs(_tempmap[i].x) > GameGlobalConfig.ShipMapSize || Mathf.Abs(_tempmap[i].y) > GameGlobalConfig.ShipMapSize)
                            {
                                _isValidPos = false;
                                break;
                            }
                            else
                            {
                                _isValidPos = true;
                                    
                            }
                        }

                        
                        if(_isValidPos)
                        {
                            //判断当前的Building是否在Chunk范围内,并且当前区块内没有Building占据
                            for (int i = 0; i < _tempmap.Length; i++)
                            {
                                _temparray = GameHelper.CoordinateMapToArray(_tempmap[i], GameGlobalConfig.ShipMapSize);


                                if (Mathf.Abs(_tempmap[i].x) > GameGlobalConfig.ShipMapSize || Mathf.Abs(_tempmap[i].y) > GameGlobalConfig.ShipMapSize)
                                {
                                    _isValidPos = false;
                                    break;
                                }

                                if (editorShip.ChunkMap[_temparray.x, _temparray.y] != null)
                                {
                                    _isValidPos = false;
                                    break;
                                }
                            }
                        }
                        

                        //判断当前的邻接 是否有任何一个Chunk
                        if(_isValidPos)
                        {
                            _tempmap = _outLineShipCood.AddToAll(_pointedShipCoord);
                            _isValidPos = false;
                            for (int i = 0; i < _tempmap.Length; i++)
                            {
                                if (Mathf.Abs(_tempmap[i].x) > GameGlobalConfig.ShipMapSize || Mathf.Abs(_tempmap[i].y) > GameGlobalConfig.ShipMapSize)
                                {
                                    
                                    continue;
                                }
                                _temparray = GameHelper.CoordinateMapToArray(_tempmap[i], GameGlobalConfig.ShipMapSize);
                                if (editorShip.ChunkMap[_temparray.x, _temparray.y] != null)
                                {
                                    _isValidPos = true;
                                    break;
                                }
                            }
                        }

              

                        //决定颜色
                        if (currentChunk == null && _isValidPos)
                        {
                            editorBrush.brushSprite.color = editorBrush.validColor;
                            editorBrush.ChangeShadowColor(editorBrush.validColor);
                        }
                        else
                        {
                            editorBrush.brushSprite.color = editorBrush.invalidColor;
                            editorBrush.ChangeShadowColor(editorBrush.invalidColor);
                        }
                        editorBrush.transform.position = editorShip.GetWorldPosFromShipCoordinate(_pointedShipCoord);

                        break;
                    case EditorMode.BuildingEditorMode:

                        if (currentInventoryItem.itemconfig == null)
                        {
                            return;
                        }

                        _isValidPos = false;

                        //判断鼠标是否在建造范围内
                        if (Mathf.Abs(_pointedShipCoord.x) > GameGlobalConfig.ShipMapSize || Mathf.Abs(_pointedShipCoord.y) > GameGlobalConfig.ShipMapSize)
                        {
                            editorBrush.gameObject.SetActive(false);
                            return;
                        }
                        else
                        {
                            editorBrush.gameObject.SetActive(true);
                            _isValidPos = true;
                            //editorBrush.UpdateShadows(currentInventoryBuilding.config.GetReletiveBuildingCoord());
                        }

                        //判断鼠标是否在Chunk内

                        if(currentChunk == null)
                        {
                            _isValidPos = false;
                        }


                        _tempmap = _reletiveCoord.AddToAll(_pointedShipCoord);
                       
                        

                        //判断当前的Building是否在Chunk范围内,并且当前区块内没有Building占据
                        for (int i = 0; i < _tempmap.Length; i++)
                        {
                            _temparray = GameHelper.CoordinateMapToArray(_tempmap[i], GameGlobalConfig.ShipMapSize);
                            if (editorShip.ChunkMap[_temparray.x, _temparray.y] == null)
                            {
                                _isValidPos = false;
                                break;
                            }
                            if(editorShip.ChunkMap[_temparray.x, _temparray.y].building != null)
                            {
                                _isValidPos = false;
                                break;
                            }
                        }


                        if (currentChunk != null && _isValidPos)
                        {
                            editorBrush.brushSprite.color = editorBrush.validColor;
                            editorBrush.ChangeShadowColor(editorBrush.validColor);
                        }
                        else
                        {
                            editorBrush.brushSprite.color = editorBrush.invalidColor;
                            editorBrush.ChangeShadowColor(editorBrush.invalidColor);
                        }


                        //使用selectedChunk 和 当前选择的currentInventoryBuilding 中map信息 来计算是否有其他位置重合，
                        //需要检测已经有的Building


                        editorBrush.transform.position = editorShip.GetWorldPosFromShipCoordinate(_pointedShipCoord);
                        break;
                }


                break;
            case InputActionPhase.Canceled:
                break;
        }
    }


    public void HandleBuildRotation(InputAction.CallbackContext context)
    {
        if(!_isInitial)
        {
            return;
        }

        switch (context.phase)
        {
            case InputActionPhase.Disabled:
                break;
            case InputActionPhase.Waiting:
                break;
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Performed:
                if(currentInventoryItem.itemconfig == null)
                {
                    return;
                }
                _itemDirection++;
                if(_itemDirection >= 4)
                {
                    _itemDirection = 0;
                }
                _reletiveCoord = RotateCoordClockWise(_reletiveCoord);
                editorBrush.UpdateShadows(_reletiveCoord);
                
                if (currentInventoryItem.itemconfig.Type == UnitType.ChunkParts)
                {
                    _outLineShipCood = editorShip.GetOutLineByShipcoordList(_reletiveCoord);
                }


                editorBrush.brushSprite.transform.rotation = Quaternion.Euler(0, 0, -90 * _itemDirection);
                break;
            case InputActionPhase.Canceled:
                break;
        }
    }

    public void HandleBuildOperation(InputAction.CallbackContext context)
    {
        if (!_isInitial)
        {
            return;
        }

        if ( EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        switch (context.phase)
        {
            case InputActionPhase.Disabled:
                break;
            case InputActionPhase.Waiting:
                break;
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Performed:
                GameObject obj;
                Vector2Int chunkpartcoord;
                Vector2Int buildarray;
                switch (editorMode)
                {
                    case EditorMode.ShipEditorMode:
                        if (currentChunk == null && _isValidPos)
                        {
                            for (int i = 0; i < _reletiveCoord.Length; i++)
                                {
                                chunkpartcoord = _reletiveCoord[i] + _pointedShipCoord;

                                obj = Instantiate(currentInventoryItem.itemconfig.Prefab);
                                obj.transform.parent = editorShip.chunksParent.transform;
                                obj.transform.localPosition = new Vector3(chunkpartcoord.x, chunkpartcoord.y);


                                var coordarray = editorShip.CoordinateShipToArray(chunkpartcoord);
                                //chunke map set
                                editorShip.ChunkMap[coordarray.x, coordarray.y] = obj.GetComponent<Chunk>();
                                editorShip.ChunkMap[coordarray.x, coordarray.y].shipCoord = chunkpartcoord;
                                editorShip.ChunkMap[coordarray.x, coordarray.y].state = UnitState.Normal;
                                //ship map set

                            }
                            Debug.Log(editorShip.ToString());
                        }
                        break;
                    case EditorMode.BuildingEditorMode:

                        if(_isValidPos)
                        {
                            obj = Instantiate(currentInventoryItem.itemconfig.Prefab);
                            obj.transform.parent = editorShip.buildingsParent.transform;
                            obj.transform.localPosition = new Vector3(_pointedShipCoord.x, _pointedShipCoord.y);
                            var tempbuilding = obj.GetComponent<Building>();
                            tempbuilding.direction = _itemDirection;
                            obj.transform.rotation = Quaternion.Euler(0, 0, -90 * tempbuilding.direction);
                            tempbuilding.unitName = currentInventoryItem.itemconfig.UnitName;

                            BuildingMapInfo buildinginfo = new BuildingMapInfo();
                            //创建Building的Prefab并且归类放好

                            
                            //设置对应的ChunMap信息，比如是否为Piovt， 是否被占用等。
                            if (_tempmap.Length > 0)
                            {
                                for (int i = 0; i < _tempmap.Length; i++)
                                {
                                    buildarray = GameHelper.CoordinateMapToArray(_tempmap[i], GameGlobalConfig.ShipMapSize);
                                    

                                    editorShip.ChunkMap[buildarray.x, buildarray.y].building = tempbuilding;
                                
                                    if (_tempmap[i] == _pointedShipCoord)
                                    {
                                        editorShip.ChunkMap[buildarray.x, buildarray.y].isBuildingPiovt = true;
                
                                        tempbuilding.pivot = _tempmap[i];
                                    }
                                    editorShip.ChunkMap[buildarray.x, buildarray.y].isOccupied = true;

                                    tempbuilding.occupiedCoords.Add(_tempmap[i]);
                                }
                            }

                            editorShip.BuildingList.Add(tempbuilding);

                            //Update buildInfoList
                            buildinginfo.direction = tempbuilding.direction;
                            buildinginfo.unitName = tempbuilding.unitName;
                            buildinginfo.pivot = tempbuilding.pivot;
                            buildinginfo.occupiedCoords = tempbuilding.occupiedCoords;
                        }

                        break;
                }


                break;
            case InputActionPhase.Canceled:
                break;
        }
    }

    public void HandleRemoveOperation(InputAction.CallbackContext context)
    {
        if (!_isInitial)
        {
            return;
        }
        switch (context.phase)
        {
            case InputActionPhase.Disabled:
                break;
            case InputActionPhase.Waiting:
                break;
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Performed:
                switch (editorMode)
                {
                    case EditorMode.ShipEditorMode:

                        break;
                    case EditorMode.BuildingEditorMode:
                        break;
                }

                break;
            case InputActionPhase.Canceled:
                break;
        }
    }

    public void ChangeEditorMode(EditorMode newmode)
    {
        editorMode = newmode;
    }

    public Vector2Int[] RotateCoordClockWise(Vector2Int[] oricoord)
    {
        Vector2Int[] newcoord = new Vector2Int[oricoord.Length];

        for (int i = 0; i < oricoord.Length; i++)
        {
            newcoord[i] = new Vector2Int(oricoord[i].y, -1 * oricoord[i].x);
        }
        return newcoord;
    }


    public Vector2Int[] RotateCoordByDirection(Vector2Int[] oricoord, int direction)
    {
        Vector2Int[] newcoord = new Vector2Int[oricoord.Length];
        for (int i = 0; i < oricoord.Length; i++)
        {
            // 90 degree
            if (direction == 1)
            {
                newcoord[i] = new Vector2Int(oricoord[i].y, -1 * oricoord[i].x);
            }
            // 180 degree
            if (direction == 2)
            {
                newcoord[i] = new Vector2Int( -1 *oricoord[i].x, -1 * oricoord[i].y);
            }
            // 270 degree
            if (direction == 3)
            {
                newcoord[i] = new Vector2Int(-1 * oricoord[i].y, oricoord[i].x);
            }
        }

        return newcoord;


    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_mouseWorldPos, 0.1f);
        Gizmos.DrawLine(_mouseWorldPos.ToVector3(), _mouseWorldPos.ToVector3() + _mouseRay.direction * 10f);

    
    }

    public void OnEvent(ShipBuilderInventoryOperationEvent evt)
    {
        switch (evt.type)
        {
            case ShipBuilderInventoryOperationType.BuildingSelect:

                if(evt.unitytype == UnitType.ChunkParts)
                {
                    editorMode = EditorMode.ShipEditorMode;
                    currentInventoryItem = chunkPartInventory.Peek(evt.name);
                }
                if(evt.unitytype == UnitType.Buildings)
                {
                    editorMode = EditorMode.BuildingEditorMode;
                    currentInventoryItem = buildingInventory.Peek(evt.name);
                }
                
                editorBrush.brushSprite.sprite = currentInventoryItem.itemconfig.Icon;
        
                _reletiveCoord = currentInventoryItem.itemconfig.GetReletiveCoord();
                editorBrush.UpdateShadows(_reletiveCoord);
                _itemDirection = 0;
                editorBrush.brushSprite.transform.rotation = Quaternion.Euler(0, 0, 0);
                if (currentInventoryItem.itemconfig.Type == UnitType.ChunkParts)
                {
                    _outLineShipCood = editorShip.GetOutLineByShipcoordList(_reletiveCoord);
                }
                break;
            case ShipBuilderInventoryOperationType.BuildingUnSelect:
                break;
            case ShipBuilderInventoryOperationType.BuildingRemove:
                break;
        }
    }
}
