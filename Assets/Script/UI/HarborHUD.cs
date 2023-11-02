using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HarborHUD : GUIBasePanel, EventListener<RogueEvent>, EventListener<ShipPropertyEvent>
{
    private EnhancedScroller _plugGridScroller;
    private EnhancedScroller _wreckageSlotScroller;
    private GeneralScrollerGirdItemController _plugGridController;
    private GeneralScrollerGirdItemController _wreckageGridController;
    private TextMeshProUGUI _currencyText;
    private RectTransform _currencyContent;
    private Text _propertyBtnText;

    private ShipPropertySliderCmpt _energySlider;
    private ShipPropertySliderCmpt _loadSlider;
    private BuildSelectHoverCmpt _hoverCmpt;
    private ShipPropertyGroupPanel _propertyGroup;
    private UnitSelectOption _unitSelectOption;

    private CanvasGroup loadWarningCanvas;
    private CanvasGroup energyWarningCanvas;

    private const string ShipPlugGridItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShipPlugGroupItem";
    private const string WreckageGridItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/WreckageSlotGroup";
    private const string PropertyBtnSwitch_Main = "ShipMainProperty_Btn_Text";
    private const string PropertyBtnSwitch_Sub = "ShipSubProperty_Btn_Text";

    protected override void Awake()
    {
        base.Awake();
        _currencyContent = transform.Find("ItemPanel/Top/ResCount").SafeGetComponent<RectTransform>();
        _plugGridScroller = transform.Find("ShipPlugSlots/Scroll View").SafeGetComponent<EnhancedScroller>();
        _wreckageSlotScroller = transform.Find("ItemPanel/Content/ItemContent/Scroll View").SafeGetComponent<EnhancedScroller>();
        _energySlider = transform.Find("EnergySlider").SafeGetComponent<ShipPropertySliderCmpt>();
        _loadSlider = transform.Find("ItemPanel/Top/LoadSlider").SafeGetComponent<ShipPropertySliderCmpt>();
        _currencyText = _currencyContent.Find("CurrencyText").SafeGetComponent<TextMeshProUGUI>();
        _propertyGroup = transform.Find("PropertyPanel/PropertyGroup").SafeGetComponent<ShipPropertyGroupPanel>();
        _propertyBtnText = transform.Find("PropertyPanel/PropertyTitle/PropertyBtn/Text").SafeGetComponent<Text>();
        _unitSelectOption = transform.Find("UnitSelectOption").SafeGetComponent<UnitSelectOption>();

        loadWarningCanvas = transform.Find("ItemPanel/Top/LoadSlider/Content/Info/Warning").SafeGetComponent<CanvasGroup>();
        energyWarningCanvas = transform.Find("EnergySlider/Content/Info/Warning").SafeGetComponent<CanvasGroup>();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<RogueEvent>();
        this.EventStartListening<ShipPropertyEvent>();

        GetGUIComponent<Button>("Launch").onClick.AddListener(OnLaunchBtnPressed);
        GetGUIComponent<Button>("PropertyBtn").onClick.AddListener(OnShipPropertySwitchClick);
        GetGUIComponent<Button>("Storage").onClick.AddListener(OnUnitSelect_StorageClick);
        GetGUIComponent<Button>("ChangePos").onClick.AddListener(OnUnitSelect_ChangePostionClick);
        _unitSelectOption.Active(null, false);
        RefreshGeneral();
        SoundManager.Instance.PlayUISound("Harbor_Enter");
    }

    public override void Hidden()
    {
        _wreckageGridController.Clear();
        _plugGridController.Clear();
        _hoverCmpt?.PoolableDestroy();

        this.EventStopListening<RogueEvent>();
        this.EventStopListening<ShipPropertyEvent>();
        base.Hidden();
    }

    public void OnLaunchBtnPressed()
    {
        GameStateTransitionEvent.Trigger(EGameState.EGameState_GameHarbor);
        SoundManager.Instance.PlayUISound(SoundEventStr.UI_Launch_ButtonClick);
    }

    public void OnEvent(RogueEvent evt)
    {
        switch (evt.type)
        {
            case RogueEventType.CurrencyChange:
                RefreshCurrency();
                break;

            case RogueEventType.HoverUnitDisplay:
                OnHoverUnitDisplay((Unit)evt.param[0]);
                break;

            case RogueEventType.HideHoverUnitDisplay:
                OnHideHoverUnitDisplay((Unit)evt.param[0]);
                break;

            case RogueEventType.CancelWreckageSelect:
                CancelWreckageSelect();
                break;

            case RogueEventType.RefreshWreckage:
            case RogueEventType.WreckageAddToShip:
            case RogueEventType.WasteCountChange:
                RefreshWreckageContent();
                break;

            case RogueEventType.ShowUnitSelectOptionPanel:
                var unit = (Unit)evt.param[0];
                _unitSelectOption.Active(unit, true);
                break;

            case RogueEventType.HideUnitSelectOptionPanel:
                _unitSelectOption.Active(null, false);
                break;
        }
    }

    public void OnEvent(ShipPropertyEvent evt)
    {
        switch (evt.type)
        {
            case ShipPropertyEventType.EnergyChange:
                RefreshEnergySlider();
                break;

            case ShipPropertyEventType.WreckageLoadChange:
                RefreshLoadSlider();
                break;

            case ShipPropertyEventType.MainPropertyValueChange:
                var modifyKey = (PropertyModifyKey)evt.param[0];
                _propertyGroup.RefreshPropertyByKey(modifyKey);
                break;
        }
    }

    private void RefreshGeneral()
    {
        InitPlugController();
        InitWreckageController();
        RefreshEnergySlider();
        RefreshLoadSlider();
        RefreshCurrency();
        OnShipPropertySwitchClick();
    }

    private void RefreshEnergySlider()
    {
        _energySlider.RefreshEnergy();
        energyWarningCanvas.ActiveCanvasGroup(_energySlider.overlordPercent > 1);
    }
    
    private void RefreshLoadSlider()
    {
        _loadSlider.RefreshLoad();
        loadWarningCanvas.ActiveCanvasGroup(_loadSlider.overlordPercent > 1);
    }

    /// <summary>
    /// 刷新货币
    /// </summary>
    private void RefreshCurrency()
    {
        _currencyText.text = RogueManager.Instance.CurrentCurrency.ToString();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_currencyContent);
    }

    private void OnShipPropertySwitchClick()
    {
        _propertyGroup.SwitchGroupType();
        if(_propertyGroup.CurrentGroupType == ShipPropertyGroupPanel.GroupType.Main)
        {
            _propertyBtnText.text = LocalizationManager.Instance.GetTextValue(PropertyBtnSwitch_Main);
        }
        else
        {
            _propertyBtnText.text = LocalizationManager.Instance.GetTextValue(PropertyBtnSwitch_Sub);
        }
    }

    #region Wreckage

    private void InitWreckageController()
    {
        _wreckageGridController = new GeneralScrollerGirdItemController();
        _wreckageGridController.numberOfCellsPerRow = 3;
        _wreckageGridController.InitPrefab(WreckageGridItem_PrefabPath, true);
        _wreckageGridController.OnItemSelected += OnWreckageItemSelect;
        _wreckageSlotScroller.Delegate = _wreckageGridController;
        RefreshWreckageContent();
    }

    private void OnWreckageItemSelect(uint uid, int dataIndex)
    {

    }

    /// <summary>
    /// 刷新插件物品栏
    /// </summary>
    private void RefreshWreckageContent()
    {
        var items = GameHelper.GetCurrentWreckageItems();
        _wreckageGridController.RefreshData(items);
        _wreckageSlotScroller.ReloadData();
    }

    #endregion

    #region Plug

    private void InitPlugController()
    {
        _plugGridController = new GeneralScrollerGirdItemController();
        _plugGridController.numberOfCellsPerRow = 15;
        _plugGridController.InitPrefab(ShipPlugGridItem_PrefabPath, true);
        _plugGridController.OnItemSelected += OnPlugItemSelect;
        _plugGridScroller.Delegate = _plugGridController;
        RefreshShipPlugsContent();
    }

    /// <summary>
    /// 刷新插件物品栏
    /// </summary>
    private void RefreshShipPlugsContent()
    {
        var items = GameHelper.GetRogueShipPlugItems();
        _plugGridController.RefreshData(items);
        _plugGridScroller.ReloadData();
    }

    /// <summary>
    /// 选择插件显示详情
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="dataIndex"></param>
    private void OnPlugItemSelect(uint uid, int dataIndex)
    {
        ///RefreshInfo
    }

    #endregion

    /// <summary>
    /// 显示Unit选中
    /// </summary>
    /// <param name="unit"></param>
    private void OnHoverUnitDisplay(Unit unit)
    {
        UIManager.Instance.ClearAllHoverUI();
        var pos = UIManager.Instance.GetUIposBWorldPosition(unit.transform.position);
        var offset = new Vector2(unit._baseUnitConfig.CorsorOffsetX, unit._baseUnitConfig.CorsorOffsetY);
        UIManager.Instance.CreatePoolerUI<BuildSelectHoverCmpt>("BuildSelectHover", true, E_UI_Layer.Top, null, (panel) =>
        {
            panel.SetUp(pos, unit._baseUnitConfig.GetMapSize(), offset, unit.UnitID);
            _hoverCmpt = panel;
        });
        unit.OutLineHighlight(true);
    }

    private void OnHideHoverUnitDisplay(Unit unit)
    {
        if(_hoverCmpt != null)
        {
            _hoverCmpt.PoolableDestroy();
        }
        if (unit != null)
        {
            unit.OutLineHighlight(false);
        }
    }

    private void CancelWreckageSelect()
    {
        _wreckageGridController.CalcelSelectAll();
    }

    private void OnUnitSelect_StorageClick()
    {
        ShipBuilder.instance.StorageCurrentHoverUnit();
    }

    private void OnUnitSelect_ChangePostionClick()
    {
        ShipBuilder.instance.EnterMoveMode();
    }

}
