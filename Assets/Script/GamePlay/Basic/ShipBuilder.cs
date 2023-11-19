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
    /// Unit能否放置
    /// </summary>
    private bool _canUnitPlace = false;


    private Vector2Int _pointedShipCoord;
    private Vector2Int[] _reletiveCoord;

    private Vector2Int[] _tempUnitMap;
    private Vector2Int _temparray;

    /// <summary>
    /// 当前选中的chunk
    /// </summary>
    private Chunk currentChunk;
    private Unit _currentHoverUnit;

    private bool _isPointOverGameObject;
    private bool _isDisplayingHoverUnit = false;
    private bool _isShowUnitSelectOptionPanel = false;
    private Vector2Int _tempBuildCenter;
    /// <summary>
    /// 移动模式
    /// </summary>
    private bool _inMoveMode = false;

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

        CameraManager.Instance.ChangeVCameraFollowTarget(transform, true);
        CameraManager.Instance.SetHarborZoom();

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

                //判断鼠标是否在Chunk内
                var chunk = editorShip.GetChunkFromShipCoordinate(_pointedShipCoord);
                ///如果与老chunk相同，则不更新
                if (currentChunk == chunk)
                    return;

                currentChunk = chunk;
                RefreshGridOccupied();
                _isValidPos &= currentChunk != null;

                ///空选中，显示信息
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
                    {
                        _canUnitPlace = false;
                        return;
                    }
                    //使用selectedChunk 和 当前选择的currentInventoryBuilding 中map信息 来计算是否有其他位置重合，
                    //需要检测已经有的Building
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

                if (_inMoveMode)
                {
                    ///退出移动模式
                    _inMoveMode = false;
                }

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
    /// 右键点击
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

                if (_inMoveMode)
                {
                    CancelMoveMode();
                    return;
                }

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
    /// 进入移动模式，重新更换位置
    /// </summary>
    public void EnterMoveMode()
    {
        if (_inMoveMode || _currentHoverUnit == null)
            return;

        _inMoveMode = true;

        if (_isShowUnitSelectOptionPanel)
        {
            ClearCurrentUnitOptionPanel();
        }
        _tempBuildCenter = _currentHoverUnit.pivot;
        editorShip.RemoveEdtiorUnit(_currentHoverUnit);
        SetSelectedInventoryItem(new InventoryItem(_currentHoverUnit._baseUnitConfig));
        SetBrushSprite();
        _currentHoverUnit = null;
    }

    /// <summary>
    /// 出售Unit
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
            editorShip.RemoveEdtiorUnit(_currentHoverUnit, true);
            _currentHoverUnit = null;
            SoundManager.Instance.PlayUISound("Unit_UnBuild");
        }
    }

    /// <summary>
    /// Unit放入仓库
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
        SoundManager.Instance.PlayUISound("Unit_UnBuild");
    }

    #endregion

    #region Function

    /// <summary>
    /// 建造
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
        SoundManager.Instance.PlayUISound("Unit_Build");
    }

    /// <summary>
    /// 撤销选择
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
    /// 是否在建造范围内
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
    /// 处理建造条件
    /// </summary>
    /// <returns></returns>
    private bool HandleUnitBuildProcess()
    {
        _canUnitPlace = true;
        var unitCfg = currentInventoryItem.itemconfig as BaseUnitConfig;

        var offset = new Vector2(unitCfg.CorsorOffsetX, unitCfg.CorsorOffsetY);
        editorBrush.SetPosition(GameHelper.GetWorldPosFromReletiveCoord(editorShip.shipMapCenter, _pointedShipCoord), offset);

        _tempUnitMap = _reletiveCoord.AddToAll(_pointedShipCoord);
        //判断当前的Building是否在Chunk范围内,并且当前区块内没有Building占据
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
        SoundManager.Instance.PlayUISound(SoundEventStr.Mouse_PointOver_2);
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
    /// 选中当前悬停的Unit
    /// </summary>
    private void HandleSelectCurrentUnitClick()
    {
        ///主武器没有选项
        var unitType = _currentHoverUnit._baseUnitConfig.unitType;
        if (unitType == UnitType.MainWeapons || unitType == UnitType.MainBuilding)
            return;

        if (_isDisplayingHoverUnit)
        {
            RogueEvent.Trigger(RogueEventType.HideHoverUnitDisplay, _currentHoverUnit);
            _isDisplayingHoverUnit = false;
        }

        if (_isShowUnitSelectOptionPanel)
        {
            ClearCurrentUnitOptionPanel();
            return;
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
    /// 刷新当前选择的Unit单位
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

    /// <summary>
    /// 取消移动模式
    /// </summary>
    private void CancelMoveMode()
    {
        if(currentInventoryItem != null)
        {
            _tempUnitMap = _reletiveCoord.AddToAll(_tempBuildCenter);
            ///恢复
            var unit = editorShip.AddEditUnit(currentInventoryItem.itemconfig, _tempUnitMap, _tempBuildCenter, _itemDirection, false);
            var chunks = unit.occupiedCoords;
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var grid = GetChunkGridByPos(chunk.x, chunk.y);
                if (grid != null)
                {
                    grid.SetOccupied(true);
                }
            }

            _canUnitPlace = false;
            _itemDirection = 0;
            editorBrush.ResetBrush();
            editorBrush.gameObject.SetActive(false);
        }
        
        currentInventoryItem = null;
        _inMoveMode = false;
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
    /// 刷新格子占位
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
