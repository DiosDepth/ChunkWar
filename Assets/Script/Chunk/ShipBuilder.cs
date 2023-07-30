using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;




[System.Serializable]
public class Inventory : IEnumerable, IEnumerator
{

    private Dictionary<string, InventoryItem> storeDic = new Dictionary<string, InventoryItem>();
    private IEnumerator<KeyValuePair<string, InventoryItem>> dicEnumerator;

    public object Current => dicEnumerator.Current;

    public Inventory()
    {
        dicEnumerator = storeDic.GetEnumerator();
    }

    public Inventory(Dictionary<string,InventoryItem> dic)
    {
        storeDic = dic;
        dicEnumerator = storeDic.GetEnumerator();
    }



    public void CheckIn(InventoryItem m_item)
    {
        if(storeDic.ContainsKey(m_item.itemconfig.UnitName))
        {
            return;
        }
        storeDic.Add(m_item.itemconfig.UnitName, m_item);
        //发送入库事件
        InventoryEvent.Trigger(InventoryEventType.Checkin,m_item.itemconfig.UnitName);
    }

    public InventoryItem CheckOut(string m_name)
    {
        InventoryItem item;
        storeDic.TryGetValue(m_name, out item);
      

        //发出库事件
        InventoryEvent.Trigger(InventoryEventType.CheckOut, item.itemconfig.UnitName);

        storeDic.Remove(m_name);
        return item;
    }

    public InventoryItem Peek(string m_name)
    {
        InventoryItem item;
        storeDic.TryGetValue(m_name, out item);

        return item;
    }

    public bool IsEmpty()
    {
        if(storeDic.Count == 0)
        {
            return true;
        }
        return false;
    }

    public IEnumerator GetEnumerator()
    {
        dicEnumerator = storeDic.GetEnumerator();
        return dicEnumerator;
    }

    public bool MoveNext()
    {
        return dicEnumerator.MoveNext();
    }

    public void Reset()
    {
       
        dicEnumerator.Reset();
    }

}

public enum InventoryOperationType
{
    ItemSelect,
    ItemUnselect,
    ItemRemove,
   
}

public struct InventoryOperationEvent
{
    public InventoryOperationType type;

    public int index;
    public string name;
    public InventoryOperationEvent(InventoryOperationType m_type, int m_index, string m_name)
    {
        type = m_type;
   
        index = m_index;
        name = m_name;
    }
    public static InventoryOperationEvent e;
    public static void Trigger(InventoryOperationType m_type, int m_index, string m_name)
    {
        e.type = m_type;

        e.index = m_index;
        e.name = m_name;
        EventCenter.Instance.TriggerEvent<InventoryOperationEvent>(e);
    }
}

public class ShipBuilder : MonoBehaviour,EventListener<InventoryOperationEvent>
{
    public static ShipBuilder instance;
    public enum EditorMode
    {
      
        ShipEditorMode,
        BuildingEditorMode,
    }

    public int brickCount = 100;
    public int brickConsumePerChunk = 5;

    public EditorMode editorMode = EditorMode.BuildingEditorMode;

    public GameObject CorePrefab;
    public GameObject BasePrefab;

    public Ship editorShip;
    public ShipBuilderBrush editorBrush;
    public Color brushValidColor;
    public Color brushInvalidColor;





    [Header("Inventory")]


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


    private bool _isPointOverGameObject;
    // Start is called before the first frame update



   


    void Awake()
    {
        instance = this;
    }
    void Start()
    {

    }
    public void Initialization()
    {
        EventRegister.EventStartListening<InventoryOperationEvent>(this);
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
        editorBrush.Initialization();

        BaseUnitConfig baseConfig;
        DataManager.Instance.UnitConfigDataDic.TryGetValue("DirectionalSmoothboreCannon", out baseConfig);
        GameManager.Instance.gameEntity.buildingInventory.CheckIn(new InventoryItem(baseConfig));
    }

    // Update is called once per frame
    void Update()
    {

   

    }

    private void OnDestroy()
    {
        EventRegister.EventStopListening<InventoryOperationEvent>(this);
        InputDispatcher.Instance.Action_GamePlay_Point -= HandleBuildMouseMove;
        InputDispatcher.Instance.Action_GamePlay_LeftClick -= HandleBuildOperation;
        InputDispatcher.Instance.Action_GamePlay_RightClick -= HandleRemoveOperation;
        InputDispatcher.Instance.Action_GamePlay_MidClick -= HandleBuildRotation;

     
    }

    public void LoadingShip(RuntimeData runtimedata)
    {

        if(editorShip == null)
        {

            var obj = new GameObject();
            obj.transform.position = Vector3.zero;
            obj.name = "ShipContainer";

            var ship = GameObject.Instantiate(RogueManager.Instance.currentShipSelection.itemconfig.Prefab);
            editorShip = ship.GetComponentInChildren<Ship>();
            editorShip.container = obj;
            editorShip.gameObject.SetActive(true);
            editorShip.transform.position = Vector3.zero - editorShip.shipMapCenter.localPosition;
            editorShip.transform.rotation = Quaternion.identity ;
            editorShip.transform.parent = obj.transform;

            
            editorShip.container.transform.name += "_Editor";
        }        

        if (runtimedata == null)
        {
            Debug.LogError("runtimeData is null, plealse check it");
            return;
        }
        else
        {
            editorShip.LoadRuntimeData(runtimedata);
        }
        editorShip.CreateShip();
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
        //if(UIManager.Instance.IsMouseOverUI())
        //{
        //    return;
        //}
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
                _pointedShipCoord = GameHelper.GetReletiveCoordFromWorldPos(editorShip.shipMapCenter, _mouseWorldPos);
                //Debug.Log("PointShipCoord = "+ _pointedShipCoord);
                currentChunk = editorShip.GetChunkFromShipCoordinate(_pointedShipCoord);



                switch (editorMode)
                {
                    //case EditorMode.ShipEditorMode:


                    //    if (currentInventoryItem.itemconfig == null)
                    //    {
                    //        return;
                    //    }

                    //    _isValidPos = false;

                    //    _tempmap = _reletiveCoord.AddToAll(_pointedShipCoord);

                    //    //判断鼠标是否在建造范围内

                    //    for (int i = 0; i < _tempmap.Length; i++)
                    //    {

                    //        if (Mathf.Abs(_pointedShipCoord.x) > GameGlobalConfig.ShipMapSize || Mathf.Abs(_pointedShipCoord.y) > GameGlobalConfig.ShipMapSize)
                    //        {
                    //            editorBrush.gameObject.SetActive(false);
                    //            return;
                    //        }
                    //        else
                    //        {
                    //            editorBrush.gameObject.SetActive(true);
                    //        }

                    //        if (Mathf.Abs(_tempmap[i].x) > GameGlobalConfig.ShipMapSize || Mathf.Abs(_tempmap[i].y) > GameGlobalConfig.ShipMapSize)
                    //        {
                    //            _isValidPos = false;
                    //            break;
                    //        }
                    //        else
                    //        {
                    //            _isValidPos = true;
                                    
                    //        }
                    //    }

                        
                    //    if(_isValidPos)
                    //    {
                    //        //判断当前的Building是否在Chunk范围内,并且当前区块内没有Building占据
                    //        for (int i = 0; i < _tempmap.Length; i++)
                    //        {
                    //            _temparray = GameHelper.CoordinateMapToArray(_tempmap[i], GameGlobalConfig.ShipMapSize);


                    //            if (Mathf.Abs(_tempmap[i].x) > GameGlobalConfig.ShipMapSize || Mathf.Abs(_tempmap[i].y) > GameGlobalConfig.ShipMapSize)
                    //            {
                    //                _isValidPos = false;
                    //                break;
                    //            }

                    //            if (editorShip.ChunkMap[_temparray.x, _temparray.y] != null)
                    //            {
                    //                _isValidPos = false;
                    //                break;
                    //            }
                    //        }
                    //    }
                        

                    //    //判断当前的邻接 是否有任何一个Chunk
                    //    if(_isValidPos)
                    //    {
                    //        _tempmap = _outLineShipCood.AddToAll(_pointedShipCoord);
                    //        _isValidPos = false;
                    //        for (int i = 0; i < _tempmap.Length; i++)
                    //        {
                    //            if (Mathf.Abs(_tempmap[i].x) > GameGlobalConfig.ShipMapSize || Mathf.Abs(_tempmap[i].y) > GameGlobalConfig.ShipMapSize)
                    //            {
                                    
                    //                continue;
                    //            }
                    //            _temparray = GameHelper.CoordinateMapToArray(_tempmap[i], GameGlobalConfig.ShipMapSize);
                    //            if (editorShip.ChunkMap[_temparray.x, _temparray.y] != null)
                    //            {
                    //                _isValidPos = true;
                    //                break;
                    //            }
                    //        }
                    //    }

              

                    //    //决定颜色
                    //    if (currentChunk == null && _isValidPos)
                    //    {
                    //        editorBrush.brushSprite.color = editorBrush.validColor;
                    //        editorBrush.ChangeShadowColor(editorBrush.validColor);
                    //    }
                    //    else
                    //    {
                    //        editorBrush.brushSprite.color = editorBrush.invalidColor;
                    //        editorBrush.ChangeShadowColor(editorBrush.invalidColor);
                    //    }
                    //    editorBrush.transform.position = GameHelper.GetWorldPosFromReletiveCoord(editorShip.shipMapCenter, _pointedShipCoord);  

                    //    break;
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
                            Debug.Log("["+i+"] " + "ReletiveCoord : " + _reletiveCoord[i] + "  Tempmap : " + _tempmap[i]);
                            _temparray = GameHelper.CoordinateMapToArray(_tempmap[i], GameGlobalConfig.ShipMapSize);
                            if (editorShip.ChunkMap[_temparray.x, _temparray.y] == null)
                            {
                                _isValidPos = false;
                                break;
                            }
                            if(editorShip.ChunkMap[_temparray.x, _temparray.y].unit != null)
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


                        editorBrush.transform.position = GameHelper.GetWorldPosFromReletiveCoord(editorShip.shipMapCenter, _pointedShipCoord); 
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
                if(currentInventoryItem.itemconfig == null || (currentInventoryItem.itemconfig as BuildingConfig).redirection == false)
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
                    //case EditorMode.ShipEditorMode:
                    //    if (currentChunk == null && _isValidPos)
                    //    {
                    //        for (int i = 0; i < _reletiveCoord.Length; i++)
                    //            {
                    //            chunkpartcoord = _reletiveCoord[i] + _pointedShipCoord;

                    //            obj = Instantiate(currentInventoryItem.itemconfig.Prefab);
                    //            //obj.transform.parent = editorShip.chunksParent.transform;
                    //            obj.transform.localPosition = new Vector3(chunkpartcoord.x, chunkpartcoord.y);


                    //            var coordarray = editorShip.CoordinateShipToArray(chunkpartcoord);
                    //            //chunke map set
                    //            editorShip.ChunkMap[coordarray.x, coordarray.y] = obj.GetComponent<Chunk>();
                    //            editorShip.ChunkMap[coordarray.x, coordarray.y].shipCoord = chunkpartcoord;
                    //            editorShip.ChunkMap[coordarray.x, coordarray.y].state = UnitState.Normal;
                    //            //ship map set

                    //        }
                    //        Debug.Log(editorShip.ToString());
                    //    }
                    //    break;
                    case EditorMode.BuildingEditorMode:

                        if(_isValidPos)
                        {

                            editorShip.AddUnit(currentInventoryItem.itemconfig, _tempmap, _pointedShipCoord, _itemDirection);
                            //obj = Instantiate(currentInventoryItem.itemconfig.Prefab);
                            //obj.transform.parent = editorShip.buildingsParent.transform;
                            //obj.transform.localPosition = new Vector3(_pointedShipCoord.x + editorShip.shipMapCenter.localPosition.x, _pointedShipCoord.y + editorShip.shipMapCenter.localPosition.y);
                            //var tempunit = obj.GetComponent<Unit>();
                            //tempunit.direction = _itemDirection;
                            //obj.transform.rotation = Quaternion.Euler(0, 0, -90 * tempunit.direction);
                            //tempunit.unitName = currentInventoryItem.itemconfig.UnitName;

                        
                            ////创建Building的Prefab并且归类放好

                            
                            ////设置对应的ChunMap信息，比如是否为Piovt， 是否被占用等。
                            //if (_tempmap.Length > 0)
                            //{
                            //    for (int i = 0; i < _tempmap.Length; i++)
                            //    {
                            //        buildarray = GameHelper.CoordinateMapToArray(_tempmap[i], GameGlobalConfig.ShipMapSize);
                                    

                            //        editorShip.ChunkMap[buildarray.x, buildarray.y].unit = tempunit;
                                
                            //        if (_tempmap[i] == _pointedShipCoord)
                            //        {
                            //            editorShip.ChunkMap[buildarray.x, buildarray.y].isBuildingPiovt = true;
                
                            //            tempunit.pivot = _tempmap[i];
                            //        }
                            //        editorShip.ChunkMap[buildarray.x, buildarray.y].isOccupied = true;

                            //        tempunit.occupiedCoords.Add(_tempmap[i]);
                            //    }
                            //}

                            //editorShip.UnitList.Add(tempunit);

                            //UnitMapInfo buildinginfo = new UnitMapInfo(tempunit);
                            //Update buildInfoList
                   
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
                        //只需要处理editorShip.BuildingList就可以
                        editorShip.RemoveUnit(currentChunk);
                        //if(currentChunk.isOccupied)
                        //{
                        //    editorShip.UnitList.Remove(currentChunk.unit);
                        //    GameObject.Destroy(currentChunk.unit.gameObject);
                        //    currentChunk.shipCoord = Vector2Int.zero;
                        //    currentChunk.isOccupied = false;
                        //    currentChunk.isBuildingPiovt = false;
                        //}
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

    public void OnEvent(InventoryOperationEvent evt)
    {
        switch (evt.type)
        {
            case InventoryOperationType.ItemSelect:

                //if(evt.unitytype == UnitType.ChunkParts)
                //{
                //    editorMode = EditorMode.ShipEditorMode;

                //}

                currentInventoryItem = GameManager.Instance.gameEntity.buildingInventory.Peek(evt.name);
                
                
                editorBrush.brushSprite.sprite = currentInventoryItem.itemconfig.Icon;
        
                _reletiveCoord = (currentInventoryItem.itemconfig as BaseUnitConfig) .GetReletiveCoord();
                editorBrush.UpdateShadows(_reletiveCoord);
                _itemDirection = 0;
                editorBrush.brushSprite.transform.rotation = Quaternion.Euler(0, 0, 0);

                break;
            case InventoryOperationType.ItemUnselect:
                break;
            case InventoryOperationType.ItemRemove:
                break;
        }
    }
}
