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
        InputDispatcher.Instance.Action_GamePlay_Point += HandleBuildMouseMove;
        InputDispatcher.Instance.Action_GamePlay_LeftClick += HandleBuildOperation;
        InputDispatcher.Instance.Action_GamePlay_RightClick += HandleRemoveOperation;
        InputDispatcher.Instance.Action_GamePlay_MidClick += HandleBuildRotation;
        LoadingShip(RogueManager.Instance.ShipMapData);

        CameraManager.Instance.ChangeVCameraLookAtTarget(transform);
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
                }

                //�ж�����Ƿ���Chunk��
                currentChunk = editorShip.GetChunkFromShipCoordinate(_pointedShipCoord);
                if (currentChunk == null)
                {
                    _isValidPos = false;
                }


                _tempmap = _reletiveCoord.AddToAll(_pointedShipCoord);

                //�жϵ�ǰ��Building�Ƿ���Chunk��Χ��,���ҵ�ǰ������û��Buildingռ��
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


                //ʹ��selectedChunk �� ��ǰѡ���currentInventoryBuilding ��map��Ϣ �������Ƿ�������λ���غϣ�
                //��Ҫ����Ѿ��е�Building


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
                GameObject obj;
                Vector2Int chunkpartcoord;
                Vector2Int buildarray;

                if (_isValidPos)
                {

                    editorShip.AddUnit(currentInventoryItem.itemconfig, _tempmap, _pointedShipCoord, _itemDirection);

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
                editorShip.RemoveUnit(currentChunk);
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
