using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ShipBuilder : MonoBehaviour
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

    public PlayerShip editorShip;
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
    }

    // Update is called once per frame
    void Update()
    {

   

    }

    private void OnDestroy()
    {
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
            editorShip = ship.GetComponentInChildren<PlayerShip>();
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

                    //    //�ж�����Ƿ��ڽ��췶Χ��

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
                    //        //�жϵ�ǰ��Building�Ƿ���Chunk��Χ��,���ҵ�ǰ������û��Buildingռ��
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
                        

                    //    //�жϵ�ǰ���ڽ� �Ƿ����κ�һ��Chunk
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

              

                    //    //������ɫ
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

                        //�ж�����Ƿ��ڽ��췶Χ��
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

                        //�ж�����Ƿ���Chunk��

                        if(currentChunk == null)
                        {
                            _isValidPos = false;
                        }


                        _tempmap = _reletiveCoord.AddToAll(_pointedShipCoord);
                       


                    
                        

                        //�жϵ�ǰ��Building�Ƿ���Chunk��Χ��,���ҵ�ǰ������û��Buildingռ��
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


                        //ʹ��selectedChunk �� ��ǰѡ���currentInventoryBuilding ��map��Ϣ �������Ƿ�������λ���غϣ�
                        //��Ҫ����Ѿ��е�Building


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

                        
                            ////����Building��Prefab���ҹ���ź�

                            
                            ////���ö�Ӧ��ChunMap��Ϣ�������Ƿ�ΪPiovt�� �Ƿ�ռ�õȡ�
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
                        //ֻ��Ҫ����editorShip.BuildingList�Ϳ���
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

    //public void OnEvent(InventoryOperationEvent evt)
    //{
    //    switch (evt.type)
    //    {
    //        case InventoryOperationType.ItemSelect:

    //            //if(evt.unitytype == UnitType.ChunkParts)
    //            //{
    //            //    editorMode = EditorMode.ShipEditorMode;

    //            //}

    //            currentInventoryItem = GameManager.Instance.gameEntity.buildingInventory.Peek(evt.name);
                
                
    //            editorBrush.brushSprite.sprite = currentInventoryItem.itemconfig.Icon;
        
    //            _reletiveCoord = (currentInventoryItem.itemconfig as BaseUnitConfig) .GetReletiveCoord();
    //            editorBrush.UpdateShadows(_reletiveCoord);
    //            _itemDirection = 0;
    //            editorBrush.brushSprite.transform.rotation = Quaternion.Euler(0, 0, 0);

    //            break;
    //        case InventoryOperationType.ItemUnselect:
    //            break;
    //        case InventoryOperationType.ItemRemove:
    //            break;
    //    }
    //}
}