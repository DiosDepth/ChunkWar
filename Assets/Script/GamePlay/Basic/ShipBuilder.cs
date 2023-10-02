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

    public PlayerEditShip editorShip;
    public ShipBuilderBrush editorBrush;
   
    public InventoryItem CurrentInventoryItem
    {
        get { return currentInventoryItem; }
        set
        {
            SetSelectedInventoryItem(value);
        }
    }

    //select item from both chunkPartInventory and buildingInventory
    private InventoryItem currentInventoryItem;

    [Header("CameraSettings")]
    public Vector3 cameraOffset = Vector3.zero;
    public float cameraSize = 10;


    private bool _isInitial;
    private Vector2 _mousePos;
    private Vector2 _mouseWorldPos;

    private int _itemDirection = 0;
    private bool _isValidPos = false;
    /// <summary>
    /// Unit�ܷ����
    /// </summary>
    private bool _canUnitPlace = false;


    private Vector2Int _pointedShipCoord;
    private Vector2Int[] _reletiveCoord;

    private Vector2Int[] _tempUnitMap;
    private Vector2Int _temparray;

    /// <summary>
    /// ��ǰѡ�е�chunk
    /// </summary>
    private Chunk currentChunk;
    private Unit _currentHoverUnit;

    private bool _isPointOverGameObject;
    private bool _isDisplayingHoverUnit = false;
    private bool _isShowUnitSelectOptionPanel = false;

    private List<ShipChunkGrid> _shipGrids;
    private List<ShipChunkErrorGrid> _tempErrorGrid;

    private const string ShipChunkGrid_PrefabPath = "Prefab/Chunk/ShipChunkGrid";
    private const string ShipChunkGridError_PrefabPath = "Prefab/Chunk/ShipChunkErrorGrid";

    private void Awake()
    {
        instance = this;
        _shipGrids = new List<ShipChunkGrid>();
        _tempErrorGrid = new List<ShipChunkErrorGrid>();
    }

    public void Initialization()
    {
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
        currentInventoryItem = null;
        editorBrush.Initialization();
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

            var shipCfg = RogueManager.Instance.currentShipSelection.itemconfig as PlayerShipConfig;
            var ship = GameObject.Instantiate(shipCfg.EditorPrefab);
            editorShip = ship.AddComponent<PlayerEditShip>();
            editorShip.container = obj;
            editorShip.gameObject.SetActive(true);
            var shipTrans = editorShip.transform;
            shipTrans.position = Vector3.zero - editorShip.shipMapCenter.localPosition;
            shipTrans.rotation = Quaternion.identity ;
            shipTrans.parent = obj.transform;
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
        editorShip.Initialization();
        editorShip.CreateShip();
        InitShipChunkGrids();
    }

    /// <summary>
    /// ���ñ�ˢͼƬ
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

    #region Perform Action

    private void HandleBuildMouseMove(InputAction.CallbackContext context)
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

                _isValidPos = IsCorsorInBuildRange(context);
                if (!_isValidPos)
                {
                    if (editorBrush.IsBrushActive)
                    {
                        editorBrush.ActiveBrush(false);
                    }
                    return;
                }

                //�ж�����Ƿ���Chunk��
                var chunk = editorShip.GetChunkFromShipCoordinate(_pointedShipCoord);
                ///�������chunk��ͬ���򲻸���
                if (currentChunk == chunk)
                    return;

                currentChunk = chunk;
                RefreshGridOccupied();
                _isValidPos &= currentChunk != null;

                ///��ѡ�У���ʾ��Ϣ
                if (currentInventoryItem == null)
                {
                    ///Hover 
                    if(_isValidPos && currentChunk.unit != null && !_isShowUnitSelectOptionPanel)
                    {
                        OnHoverUnitDisplay(currentChunk.unit);
                    }
                    else if (_isDisplayingHoverUnit)
                    {
                        ResetHoverUnit();
                    }
                }
                else
                {
                    editorBrush.ActiveBrush(_isValidPos);
                    if (!_isValidPos)
                        return;

                    //ʹ��selectedChunk �� ��ǰѡ���currentInventoryBuilding ��map��Ϣ �������Ƿ�������λ���غϣ�
                    //��Ҫ����Ѿ��е�Building
                    if (!HandleUnitBuildProcess())
                        return;

                    if (_canUnitPlace)
                    {
                        editorBrush.SetBrushState(ShipBuilderBrush.BrushState.Vaild);
                    }
                    else
                    {
                        editorBrush.SetBrushState(ShipBuilderBrush.BrushState.Error);
                    }
                }

                break;
            case InputActionPhase.Canceled:
                break;
        }
    }


    private void HandleBuildRotation(InputAction.CallbackContext context)
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

                editorBrush.SetDirection(_itemDirection);
                break;
            case InputActionPhase.Canceled:
                break;
        }
    }

    private void HandleBuildOperation(InputAction.CallbackContext context)
    {
        if (!_isInitial)
        {
            return;
        }

        switch (context.phase)
        {
            case InputActionPhase.Started:
                break;
            case InputActionPhase.Performed:

                if(CurrentInventoryItem != null)
                {
                    OnAddUnit();
                    return;
                }

                ///RefreshHover
                RefreshCurrnetHoverUnit();
                if (_currentHoverUnit != null)
                {
                    HandleSelectCurrentUnitClick();
                }
                else
                {
                    ClearCurrentUnitOptionPanel();
                }
                
                break;
        }
    }

    /// <summary>
    /// �Ҽ����
    /// </summary>
    /// <param name="context"></param>
    private void HandleRemoveOperation(InputAction.CallbackContext context)
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

                if (currentInventoryItem != null)
                {
                    CancelBuildingSelect();
                    return;
                }

                if (_isShowUnitSelectOptionPanel)
                {
                    ClearCurrentUnitOptionPanel();
                }

                break;
            case InputActionPhase.Canceled:
                break;
        }
    }

    #endregion

    #region Public Function

    /// <summary>
    /// ����Unit
    /// </summary>
    public void SellCurrentHoverUnit()
    {
        if (_currentHoverUnit == null)
            return;

        if (_isShowUnitSelectOptionPanel)
        {
            ClearCurrentUnitOptionPanel();
        }

        ///Sell
        
        if(RogueManager.Instance.SellUnit(_currentHoverUnit))
        {
            editorShip.RemoveEdtiorUnit(_currentHoverUnit);
            _currentHoverUnit = null;
        }
    }

    /// <summary>
    /// Unit����ֿ�
    /// </summary>
    public void StorageCurrentHoverUnit()
    {
        if (_currentHoverUnit == null)
            return;

        if (_isShowUnitSelectOptionPanel)
        {
            ClearCurrentUnitOptionPanel();
        }

        editorShip.RemoveEdtiorUnit(_currentHoverUnit);
        ///Create New Wreckage
        RogueManager.Instance.CreateAndAddNewWreckageInfo(_currentHoverUnit.UnitID);

        _currentHoverUnit = null;
    }

    #endregion

    #region Function

    /// <summary>
    /// ����
    /// </summary>
    private void OnAddUnit()
    {
        if (!_canUnitPlace || currentInventoryItem == null)
            return;

        var unit = editorShip.AddEditUnit(currentInventoryItem.itemconfig, _tempUnitMap, _pointedShipCoord, _itemDirection, true);
        var chunks = unit.occupiedCoords;
        for (int i = 0; i < chunks.Count; i++) 
        {
            var chunk = chunks[i];
            var grid = GetChunkGridByPos(chunk.x, chunk.y);
            if(grid != null)
            {
                grid.SetOccupied(true);
            }
        }

        _canUnitPlace = false;
        _itemDirection = 0;
        editorBrush.ResetBrush();
        RogueManager.Instance.RemoveWreckageByUID(currentInventoryItem.RefUID);
        ///Reset
        currentInventoryItem = null;
        editorBrush.gameObject.SetActive(false);
        RogueEvent.Trigger(RogueEventType.WreckageAddToShip);
    }

    /// <summary>
    /// ����ѡ��
    /// </summary>
    private void CancelBuildingSelect()
    {
        currentChunk = null;
        _itemDirection = 0;
        editorBrush.ResetBrush();
        RefreshGridOccupied();
        currentInventoryItem = null;
        editorBrush.gameObject.SetActive(false);
        RogueEvent.Trigger(RogueEventType.CancelWreckageSelect);
    }

    private void OnHoverUnitDisplay(Unit targetUnit)
    {
        if (_isDisplayingHoverUnit && _currentHoverUnit == targetUnit)
            return;

        if(_isDisplayingHoverUnit && _currentHoverUnit != targetUnit)
        {
            RogueEvent.Trigger(RogueEventType.HideHoverUnitDisplay, _currentHoverUnit);
        }

        SetHoverUnit(targetUnit);
    }

    /// <summary>
    /// �Ƿ��ڽ��췶Χ��
    /// </summary>
    /// <returns></returns>
    private bool IsCorsorInBuildRange(InputAction.CallbackContext context)
    {
        _mousePos = context.ReadValue<Vector2>();
        return IsCorsorInBuildRange(_mousePos);
    }

    private bool IsCorsorInBuildRange(Vector2 pos)
    {
        _mouseWorldPos = CameraManager.Instance.mainCamera.ScreenToWorldPoint(new Vector3(pos.x, pos.y, 0));
        _pointedShipCoord = GameHelper.GetReletiveCoordFromWorldPos(editorShip.shipMapCenter, _mouseWorldPos);

        return Mathf.Abs(_pointedShipCoord.x) <= GameGlobalConfig.ShipMapSize && Mathf.Abs(_pointedShipCoord.y) <= GameGlobalConfig.ShipMapSize;
    }

    /// <summary>
    /// ����������
    /// </summary>
    /// <returns></returns>
    private bool HandleUnitBuildProcess()
    {
        _canUnitPlace = true;
        editorBrush.SetPosition(GameHelper.GetWorldPosFromReletiveCoord(editorShip.shipMapCenter, _pointedShipCoord));

        _tempUnitMap = _reletiveCoord.AddToAll(_pointedShipCoord);
        //�жϵ�ǰ��Building�Ƿ���Chunk��Χ��,���ҵ�ǰ������û��Buildingռ��
        for (int i = 0; i < _tempUnitMap.Length; i++)
        {
            _temparray = GameHelper.CoordinateMapToArray(_tempUnitMap[i], GameGlobalConfig.ShipMapSize);
            var chunk = editorShip.ChunkMap[_temparray.x, _temparray.y];

            if (chunk == null)
            {
                var root = transform.Find("Grids");
                _canUnitPlace = false;
                PoolManager.Instance.GetObjectSync(ShipChunkGridError_PrefabPath, true, (obj) =>
                {
                    var gridPos = GameHelper.CoordinateArrayToMap(new Vector2Int(_temparray.x, _temparray.y), GameGlobalConfig.ShipMapSize);
                    var cmpt = obj.transform.SafeGetComponent<ShipChunkErrorGrid>();
                    cmpt.SetUp(gridPos);
                    _tempErrorGrid.Add(cmpt);
                }, root);
            }
            else
            {
                var chunkGrid = GetChunkGridByChunk(chunk);
                if (chunk.unit != null)
                {
                    _canUnitPlace = false;
                    chunkGrid.SetGridError();
                }
                else
                {
                    chunkGrid.SetOccupied(true);
                }
            }
        }

        return _canUnitPlace;
    }

    private void SetHoverUnit(Unit targetUnit)
    {
        _isDisplayingHoverUnit = true;
        _currentHoverUnit = targetUnit;
        RogueEvent.Trigger(RogueEventType.HoverUnitDisplay, targetUnit);
    }

    private void ResetHoverUnit()
    {
        RogueEvent.Trigger(RogueEventType.HideHoverUnitDisplay, _currentHoverUnit);
        _currentHoverUnit = null;
        _isDisplayingHoverUnit = false;
    }

    private void SetSelectedInventoryItem(InventoryItem item)
    {
        if (currentInventoryItem != item) 
        {
            currentInventoryItem = item;
        }

        if(currentInventoryItem != null && currentInventoryItem.itemconfig is BaseUnitConfig)
        {
            var unitCfg = currentInventoryItem.itemconfig as BaseUnitConfig;
            _reletiveCoord = unitCfg.GetReletiveCoord();
        }
    }

    /// <summary>
    /// ѡ�е�ǰ��ͣ��Unit
    /// </summary>
    private void HandleSelectCurrentUnitClick()
    {
        ///������û��ѡ��
        if (_currentHoverUnit._baseUnitConfig.unitType == UnitType.MainWeapons)
            return;

        if (_isDisplayingHoverUnit)
        {
            RogueEvent.Trigger(RogueEventType.HideHoverUnitDisplay, _currentHoverUnit);
            _isDisplayingHoverUnit = false;
        }

        RogueEvent.Trigger(RogueEventType.ShowUnitSelectOptionPanel, _currentHoverUnit);
        _isShowUnitSelectOptionPanel = true;
    }

    private void ClearCurrentUnitOptionPanel()
    {
        RogueEvent.Trigger(RogueEventType.HideUnitSelectOptionPanel);
        _isShowUnitSelectOptionPanel = false;
    }

    /// <summary>
    /// ˢ�µ�ǰѡ���Unit��λ
    /// </summary>
    private void RefreshCurrnetHoverUnit()
    {
        _isValidPos = IsCorsorInBuildRange(_mousePos);
        if (!_isValidPos)
        {
            _currentHoverUnit = null;
            return;
        }

        var chunk = editorShip.GetChunkFromShipCoordinate(_pointedShipCoord);
        if(chunk != null && currentChunk.unit != null)
        {
            _currentHoverUnit = currentChunk.unit;
            return;
        }
        else
        {
            _currentHoverUnit = null;
        }
    }
    #endregion

    #region Grid
    private void InitShipChunkGrids()
    {
        if (editorShip == null)
            return;

        var root = transform.Find("Grids");
        var allChunks = editorShip.ChunkMap;
        var rowCount = allChunks.GetLength(0);
        var columeCount = allChunks.GetLength(1);

        for (int row = 0; row < rowCount; row++)
        {
            for (int colume = 0; colume < columeCount; colume++)
            {
                var chunk = allChunks[row, colume];
                if(chunk != null)
                {
                    PoolManager.Instance.GetObjectSync(ShipChunkGrid_PrefabPath, true, (obj) =>
                    {
                        var cmpt = obj.transform.SafeGetComponent<ShipChunkGrid>();
                        cmpt.SetUp(chunk.shipCoord, chunk.isOccupied);
                        _shipGrids.Add(cmpt);

                    }, root);
                }
            }
        }
    }

    /// <summary>
    /// ˢ�¸���ռλ
    /// </summary>
    private void RefreshGridOccupied()
    {
        for (int i = 0; i < _shipGrids.Count; i++) 
        {
            var grid = _shipGrids[i];
            Vector2Int coordinate = new Vector2Int(grid.PosX, grid.PosY);
            var chunk = editorShip.GetChunkFromShipCoordinate(coordinate);
            if(chunk != null)
            {
                grid.SetOccupied(chunk.unit != null);
            }
        }
        ClearTempErrorGrid();
    }

    private void ClearTempErrorGrid()
    {
        for (int i = _tempErrorGrid.Count - 1; i >= 0; i--)
        {
            _tempErrorGrid[i].PoolableDestroy();
        }
        _tempErrorGrid.Clear();
    }

    private ShipChunkGrid GetChunkGridByPos(int x, int y)
    {
        return _shipGrids.Find(grid => grid.PosX == x && grid.PosY == y);
    }

    private ShipChunkGrid GetChunkGridByChunk(Chunk chunk)
    {
        return GetChunkGridByPos(chunk.shipCoord.x, chunk.shipCoord.y);
    }

    #endregion


    #region Tools
    private Vector2Int[] RotateCoordClockWise(Vector2Int[] oricoord)
    {
        Vector2Int[] newcoord = new Vector2Int[oricoord.Length];

        for (int i = 0; i < oricoord.Length; i++)
        {
            newcoord[i] = new Vector2Int(oricoord[i].y, -1 * oricoord[i].x);
        }
        return newcoord;
    }

    private Vector2Int[] RotateCoordByDirection(Vector2Int[] oricoord, int direction)
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
                newcoord[i] = new Vector2Int(-1 * oricoord[i].x, -1 * oricoord[i].y);
            }
            // 270 degree
            if (direction == 3)
            {
                newcoord[i] = new Vector2Int(-1 * oricoord[i].y, oricoord[i].x);
            }
        }

        return newcoord;
    }

    #endregion
}
