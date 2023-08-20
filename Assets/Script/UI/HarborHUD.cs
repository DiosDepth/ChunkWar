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
    private BuildSelectHoverCmpt _hoverCmpt;
    private ShipPropertyGroupPanel _propertyGroup;
    private UnitDetailInfoPanel _detailPanel;


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
        _currencyText = _currencyContent.Find("CurrencyText").SafeGetComponent<TextMeshProUGUI>();
        _hoverCmpt = transform.Find("BuildSelectHover").SafeGetComponent<BuildSelectHoverCmpt>();
        _propertyGroup = transform.Find("ItemPanel/Content/Property/PropertyGroup").SafeGetComponent<ShipPropertyGroupPanel>();
        _propertyBtnText = transform.Find("ItemPanel/Content/Property/PropertyTitle/PropertyBtn/Text").SafeGetComponent<Text>();
        _detailPanel = transform.Find("UnitDetailPanel/Content").SafeGetComponent<UnitDetailInfoPanel>();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<RogueEvent>();
        this.EventStartListening<ShipPropertyEvent>();
        GetGUIComponent<Button>("Launch").onClick.AddListener(OnLaunchBtnPressed);
        GetGUIComponent<Button>("PropertyBtn").onClick.AddListener(OnShipPropertySwitchClick);
        _detailPanel.Hide();
        _hoverCmpt.SetActive(false);
        RefreshGeneral();
    }

    public override void Hidden()
    {
        this.EventStopListening<RogueEvent>();
        this.EventStopListening<ShipPropertyEvent>();
        base.Hidden();
    }

    public void OnLaunchBtnPressed()
    {
        GameStateTransitionEvent.Trigger(EGameState.EGameState_GameCompleted);
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

            case RogueEventType.WreckageAddToShip:
                RefreshWreckageContent();
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
        }
    }

    private void RefreshGeneral()
    {
        InitPlugController();
        InitWreckageController();
        RefreshEnergySlider();
        OnShipPropertySwitchClick();
    }

    private void RefreshEnergySlider()
    {
        _energySlider.RefreshEnergy();
    }

    /// <summary>
    /// ˢ�»���
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
    /// ˢ�²����Ʒ��
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
        _plugGridController.numberOfCellsPerRow = 9;
        _plugGridController.InitPrefab(ShipPlugGridItem_PrefabPath, true);
        _plugGridController.OnItemSelected += OnPlugItemSelect;
        _plugGridScroller.Delegate = _plugGridController;
        RefreshShipPlugsContent();
    }

    /// <summary>
    /// ˢ�²����Ʒ��
    /// </summary>
    private void RefreshShipPlugsContent()
    {
        var items = GameHelper.GetRogueShipPlugItems();
        _plugGridController.RefreshData(items);
        _plugGridScroller.ReloadData();
    }

    /// <summary>
    /// ѡ������ʾ����
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="dataIndex"></param>
    private void OnPlugItemSelect(uint uid, int dataIndex)
    {
        ///RefreshInfo
    }

    #endregion

    /// <summary>
    /// ��ʾUnitѡ��
    /// </summary>
    /// <param name="unit"></param>
    private void OnHoverUnitDisplay(Unit unit)
    {
        var pos = UIManager.GetUIposBWorldPosition(unit.transform.position);
        _hoverCmpt.SetUp(pos, unit._baseUnitConfig.GetMapSize(), unit);
        unit.OutLineHighlight(true);
        _hoverCmpt.SetActive(true);
    }

    private void OnHideHoverUnitDisplay(Unit unit)
    {
        _hoverCmpt.SetActive(false);
        if (unit != null)
        {
            unit.OutLineHighlight(false);
        }
    }

    private void CancelWreckageSelect()
    {
        _wreckageGridController.CalcelSelectAll();
    }
}