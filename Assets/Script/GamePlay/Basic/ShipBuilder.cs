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

    public int brickCount = 100;
    public int brickConsumePerChunk = 5;

    public GameObject CorePrefab;
    public GameObject BasePrefab;

    public PlayerShip editorShip;
    public ShipBuilderBrush editorBrush;
    public Color brushValidColor;
    public Color brushInvalidColor;


    [Header("Inventory")]


    public Building currentBuilding;
   

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

    private Chunk currentChunk;
    private bool _isPointOverGameObject;

    private void Awake()
    {
        instance = this;
    }

    public void Initialization()
    {
        RogueManager.Instance.InitTempUnitSlots();
        InputDispatcher.Instance.Action_GamePlay_Point += HandleBuildMouseMove;
        InputDispatcher.Instance.Action_GamePlay_LeftClick += HandleBuildOperation;
        InputDispatcher.Instance.Action_GamePlay_RightClick += HandleRemoveOperation;
        InputDispatcher.Instance.Action_GamePlay_MidClick += HandleBuildRotation;
        LoadingShip(RogueManager.Instance.ShipMapData);

        CameraManager.Instance.ChangeVCameraLookAtTarget(transform);
        CameraManager.Instance.SetVCameraPos(transform.position);
        CameraManager.Instance.ChangeVCameraFollowTarget(transform);
        CameraManager.Instance.vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.x = cameraOffset.x;
        CameraManager.Instance.vcam.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.x = cameraOffset.x;
        CameraManager.Instance.vcam.m_Lens.OrthographicSize = cameraSize;

        _isInitial = true;
        editorBrush.Initialization();
        _reletiveCoord = new Vector2Int[2];
    }

    private void OnDestroy()
    {
        InputDispatcher.Instance.Action_GamePlay_Point -= HandleBuildMouseMove;
        InputDispatcher.Instance.Action_GamePlay_LeftClick -= HandleBuildOperation;
        InputDispatcher.Instance.Action_GamePlay_RightClick -= HandleRemoveOperation;
        InputDispatcher.Instance.Action_GamePlay_MidClick -= HandleBuildRotation;
    }

    public void LoadingShip(ShipMapData runtimedata)
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
        editorShip.CreateShip(true);
    }

    /// <summary>
    /// 设置笔刷图片
    /// </summary>
    public void SetBrushSprite()
    {
        if(currentInventoryItem != null)
        {
            var cfg = currentInventoryItem.itemconfig as BaseUnitConfig;
            if(cfg != null)
            {
                editorBrush.ChangeBurshSprite(cfg.EditBrushSprite, cfg.EditorBrushDefaultRotation);
            }
           
        }
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
                _pointedShipCoord = GameHelper.GetReletiveCoordFromWorldPos(editorShip.shipMapCenter, _mouseWorldPos);
                if (currentInventoryItem == null || currentInventoryItem.itemconfig == null)
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
                }

                //判断鼠标是否在Chunk内
                currentChunk = editorShip.GetChunkFromShipCoordinate(_pointedShipCoord);
                if (currentChunk == null)
                {
                    _isValidPos = false;
                }


                _tempmap = _reletiveCoord.AddToAll(_pointedShipCoord);

                //判断当前的Building是否在Chunk范围内,并且当前区块内没有Building占据
                for (int i = 0; i < _tempmap.Length; i++)
                {
                    //Debug.Log("[" + i + "] " + "ReletiveCoord : " + _reletiveCoord[i] + "  Tempmap : " + _tempmap[i]);
                    _temparray = GameHelper.CoordinateMapToArray(_tempmap[i], GameGlobalConfig.ShipMapSize);
                    if (editorShip.ChunkMap[_temparray.x, _temparray.y] == null)
                    {
                        _isValidPos = false;
                        break;
                    }
                    if (editorShip.ChunkMap[_temparray.x, _temparray.y].unit != null)
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
                OnAddUnit();
                break;
            case InputActionPhase.Canceled:
                break;
        }
    }

    /// <summary>
    /// 右键点击
    /// </summary>
    /// <param name="context"></param>
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
                CancelBuildingSelect();
                break;
            case InputActionPhase.Canceled:
                break;
        }
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

    /// <summary>
    /// 建造
    /// </summary>
    private void OnAddUnit()
    {
        if (!_isValidPos || currentInventoryItem == null)
            return;

        editorShip.AddUnit(currentInventoryItem.itemconfig, _tempmap, _pointedShipCoord, _itemDirection, true);
        _itemDirection = 0;
        editorBrush.ResetBrush();
        var slotIndex = RogueManager.Instance.CurrentSelectedHarborSlotIndex;
        RogueEvent.Trigger(RogueEventType.ShipUnitTempSlotChange, false, currentInventoryItem.itemconfig.ID, slotIndex);

        ///Reset
        RogueManager.Instance.CurrentSelectedHarborSlotIndex = 0;
        RogueManager.Instance.RemoveTempUnitInHarbor(slotIndex); 
        currentInventoryItem = null;
        editorBrush.gameObject.SetActive(false);
        RogueEvent.Trigger(RogueEventType.HideUnitDetailPage);
    }

    /// <summary>
    /// 撤销选择
    /// </summary>
    private void CancelBuildingSelect()
    {
        if (currentInventoryItem == null)
            return;

        RogueManager.Instance.CurrentSelectedHarborSlotIndex = 0;
        _itemDirection = 0;
        editorBrush.ResetBrush();
        currentInventoryItem = null;
        editorBrush.gameObject.SetActive(false);
        RogueEvent.Trigger(RogueEventType.HideUnitDetailPage);
    }
}
