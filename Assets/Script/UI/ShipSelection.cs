using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipSelection : GUIBasePanel, EventListener<GeneralUIEvent>
{
    public RectTransform slotGroup;
    private Image _shipIcon;

    private ShipSelectionInfoPanel _infoPanel;
    private EnhancedScroller _selectionScroller;
    private GeneralScrollerGirdItemController _selectionController;

    private EnhancedScroller _hardLevelScroller;
    private GeneralScrollerItemController _hardLevelController;

    private CanvasGroup _mainGroup;
    private CanvasGroup _hardLevelGroup;

    private const string ShipSelectionGridItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShipSelectionGroupItem";
    private const string HardLevelItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/HardLevelModeItem";

    private int currentSelectIndex = 0;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<GeneralUIEvent>();
        _mainGroup = transform.Find("uiGroup").SafeGetComponent<CanvasGroup>();
        _hardLevelGroup = transform.Find("HardLevelGroup").SafeGetComponent<CanvasGroup>();
        _selectionScroller = transform.Find("uiGroup/SlotPanel/Scroll View").SafeGetComponent<EnhancedScroller>();
        _hardLevelScroller = transform.Find("HardLevelGroup/Content/Scroll View").SafeGetComponent<EnhancedScroller>();
        _shipIcon = transform.Find("uiGroup/ShipIconPanel/Image").SafeGetComponent<Image>();
        _infoPanel = transform.Find("uiGroup/InfoPanel").SafeGetComponent<ShipSelectionInfoPanel>();
        GetGUIComponent<Button>("Back").onClick.AddListener(BackBtnPressed);
        InitSelectionController();
        InitHardLevelController();
        SwitchToMainGroup();
        SetFirstChooseShip();
    }

    public void BackBtnPressed()
    {
        GameStateTransitionEvent.Trigger(EGameState.EGameState_MainMenu);
    }

    public override void Hidden( )
    {
        base.Hidden();
        this.EventStopListening<GeneralUIEvent>();
    }

    private void InitSelectionController()
    {
        _selectionController = new GeneralScrollerGirdItemController();
        _selectionController.numberOfCellsPerRow = 11;
        _selectionController.InitPrefab(ShipSelectionGridItem_PrefabPath, true);
        _selectionController.OnItemSelected = OnShipSelect;
        _selectionScroller.Delegate = _selectionController;
    }

    private void InitHardLevelController()
    {
        _hardLevelController = new GeneralScrollerItemController();
        _hardLevelController.InitPrefab(HardLevelItem_PrefabPath, false);
        _hardLevelController.OnItemSelected = OnHardLevelSelect;
        _hardLevelScroller.Delegate = _hardLevelController;
    }

    /// <summary>
    /// 刷新插件物品栏
    /// </summary>
    private void RefreshShipSelectionContent()
    {
        var items = GameHelper.GetAllShipIDs();
        _selectionController.RefreshData(items);
        _selectionScroller.ReloadData();
    }

    /// <summary>
    /// 刷新hardLevel选择
    /// </summary>
    private void RefreshHardLevelSelection()
    {
        var currentSelectShip = RogueManager.Instance.currentShipSelection;
        if (currentSelectShip == null)
            return;

        GameManager.Instance.RefreshHardLevelByShip(currentSelectShip.itemconfig.ID);

        var hardLevels = GameHelper.GetAllHardLevels();
        _hardLevelController.RefreshData(hardLevels);
        _hardLevelScroller.ReloadData();
    }

    /// <summary>
    /// 刷新舰船显示
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="dataIndex"></param>
    private void OnShipSelect(uint uid, int dataIndex)
    {
        ///RefreshInfo
        var shipCfg = DataManager.Instance.GetShipConfig((int)uid);
        if (shipCfg == null)
            return;

        _shipIcon.sprite = shipCfg.GeneralConfig.IconSprite;
        _infoPanel.SetUpShipInfo(shipCfg);
    }

    private void OnHardLevelSelect(uint uid)
    {

    }


    public void OnEvent(GeneralUIEvent evt)
    {
        switch (evt.type)
        {
            case UIEventType.ShipSelectionChange:
                uint uid = (uint)evt.param[0] ;
                int dataIndex = (int)evt.param[1];
                OnShipSelect(uid, dataIndex);
                break;
            case UIEventType.ShipSelectionConfirm:
                SwitchToHardLevelGroup();
                break;
        }
    }

    private void SwitchToMainGroup()
    {
        currentSelectIndex = 0;
        RefreshShipSelectionContent();
        _mainGroup.ActiveCanvasGroup(true);
        _hardLevelGroup.ActiveCanvasGroup(false);
    }

    private void SwitchToHardLevelGroup()
    {
        currentSelectIndex = 0;
        RefreshHardLevelSelection();
        _mainGroup.ActiveCanvasGroup(false);
        _hardLevelGroup.ActiveCanvasGroup(true);
    }

    /// <summary>
    /// 选择舰船
    /// </summary>
    /// <param name="dataIndex"></param>
    private void SetFirstChooseShip()
    {
        var item = _selectionScroller.GetCellViewAtDataIndex(0);
        if (item != null && item is ShipSelectionGroupItemCmpt)
        {
            var group = item as ShipSelectionGroupItemCmpt;
            group.items[0].OnHoverEnter();
        }
    }
}