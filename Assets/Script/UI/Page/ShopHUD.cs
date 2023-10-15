using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ShopHUD : GUIBasePanel, EventListener<RogueEvent>, EventListener<ShipPropertyEvent>
{
    private Transform _shopContentRoot;
    private TextMeshProUGUI _currencyText;
    private RectTransform _currencyContent;
    private TextMeshProUGUI _rerollCostText;
    private EnhancedScroller _plugGridScroller;
    private BuildSelectHoverCmpt _hoverCmpt;
    private ShipPropertyGroupPanel _propertyGroup;
    private Text _propertyBtnText;

    private ShipPropertySliderCmpt _energySlider;
    private ShipPropertySliderCmpt _loadSlider;
    private CanvasGroup loadWarningCanvas;
    private CanvasGroup energyWarningCanvas;

    private const string PropertyBtnSwitch_Main = "ShipMainProperty_Btn_Text";
    private const string PropertyBtnSwitch_Sub = "ShipSubProperty_Btn_Text";
    private const string ShipGoodsItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShopSlotItem";
    private const string ShipPlugGridItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShipPlugGroupItem";
    private List<ShopSlotItem> allShopSlotItems = new List<ShopSlotItem>();

    private GeneralScrollerGirdItemController _plugGridController;

    protected override void Awake()
    {
        base.Awake();
        _shopContentRoot = transform.Find("ShopPanel/Content");
        _currencyContent = transform.Find("ShopPanel/Top/ResCount").SafeGetComponent<RectTransform>();
        _currencyText = _currencyContent.Find("CurrencyText").SafeGetComponent<TextMeshProUGUI>();
        _rerollCostText = transform.Find("ShopPanel/Top/Reroll/Reroll/RerollCost").SafeGetComponent<TextMeshProUGUI>();
        _plugGridScroller = transform.Find("ShipPlugSlots/Scroll View").SafeGetComponent<EnhancedScroller>();
        _propertyBtnText = transform.Find("PropertyPanel/PropertyTitle/PropertyBtn/Text").SafeGetComponent<Text>();
        _propertyGroup = transform.Find("PropertyPanel/PropertyGroup").SafeGetComponent<ShipPropertyGroupPanel>();

        _energySlider = transform.Find("SliderContent/EnergySlider").SafeGetComponent<ShipPropertySliderCmpt>();
        _loadSlider = transform.Find("SliderContent/LoadSlider").SafeGetComponent<ShipPropertySliderCmpt>();
        loadWarningCanvas = _loadSlider.transform.Find("Content/Info/Warning").SafeGetComponent<CanvasGroup>();
        energyWarningCanvas = _energySlider.transform.Find("Content/Info/Warning").SafeGetComponent<CanvasGroup>();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<RogueEvent>();
        this.EventStartListening<ShipPropertyEvent>();
        GetGUIComponent<Button>("Launch").onClick.AddListener(OnLaunchBtnPressed);
        GetGUIComponent<Button>("Reroll").onClick.AddListener(OnRerollBtnClick);
        GetGUIComponent<Button>("PropertyBtn").onClick.AddListener(OnShipPropertySwitchClick);
        InitShopContent();
        RefreshGeneral();
    }

    public override void Hidden( )
    {
        _plugGridController.Clear();
        ClearAllShopSlotItems();
        _hoverCmpt?.PoolableDestroy();

        this.EventStopListening<RogueEvent>();
        this.EventStopListening<ShipPropertyEvent>();
        base.Hidden();
    }

    public void OnLaunchBtnPressed()
    {
        RogueManager.Instance.ExitShop();
    }

    public void OnEvent(RogueEvent evt)
    {
        switch (evt.type)
        {
            case RogueEventType.CurrencyChange:
                RefreshCurrency();
                break;

            case RogueEventType.ShopReroll:
                RefreshReroll();
                InitShopContent();
                break;

            case RogueEventType.ShopCostChange:
                RefreshAllShopItemCost();
                break;

            case RogueEventType.ShipPlugChange:
                RefreshShipPlugsContent();
                break;

            case RogueEventType.HoverUnitDisplay:
                OnHoverUnitDisplay((Unit)evt.param[0]);
                break;

            case RogueEventType.HideHoverUnitDisplay:
                OnHideHoverUnitDisplay((Unit)evt.param[0]);
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

            case ShipPropertyEventType.LuckChange:
                RefreshShopLuckContent();
                break;

            case ShipPropertyEventType.MainPropertyValueChange:
                var modifyKey = (PropertyModifyKey)evt.param[0];
                _propertyGroup.RefreshPropertyByKey(modifyKey);
                break;
        }
    }

    private void RefreshGeneral()
    {
        RefreshCurrency();
        RefreshReroll();
        InitPlugController();
        OnShipPropertySwitchClick();
        RefreshEnergySlider();
        RefreshLoadSlider();
    }

    private void InitPlugController()
    {
        _plugGridController = new GeneralScrollerGirdItemController();
        _plugGridController.numberOfCellsPerRow = 15;
        _plugGridController.InitPrefab(ShipPlugGridItem_PrefabPath, true);
        _plugGridController.OnItemSelected += OnPlugItemSelect;
        _plugGridScroller.Delegate = _plugGridController;
        RefreshShipPlugsContent();
    }

    private void InitShopContent()
    {
        var allShopItems = RogueManager.Instance.CurrentRogueShopItems;
       
        if (allShopItems == null || allShopItems.Count <= 0)
        {
            ClearAllShopSlotItems();
            return;
        }

        if(allShopItems.Count != allShopSlotItems.Count)
        {
            ClearAllShopSlotItems();
            var index = 0;
            for (int i = 0; i < allShopItems.Count; i++)
            {
                PoolManager.Instance.GetObjectAsync(ShipGoodsItem_PrefabPath, true, (obj) =>
                {
                    var cmpt = obj.GetComponent<ShopSlotItem>();
                    cmpt.SetUp(allShopItems[index]);
                    allShopSlotItems.Add(cmpt);
                    index++;
                }, _shopContentRoot);
            }
        }
        else
        {
            for (int i = 0; i < allShopItems.Count; i++)
            {
                var cmpt = allShopSlotItems[i];
                cmpt.SetUp(allShopItems[i]);
            }
        }
    }

    private void ClearAllShopSlotItems()
    {
        _shopContentRoot.Pool_BackAllChilds(ShipGoodsItem_PrefabPath);
        allShopSlotItems.Clear();
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

    /// <summary>
    /// 刷新所有物品花费
    /// </summary>
    private void RefreshAllShopItemCost()
    {
        for (int i = 0; i < allShopSlotItems.Count; i++) 
        {
            allShopSlotItems[i].RefreshCost();
        }
    }

    private void RefreshReroll()
    {
        var rerollCost = RogueManager.Instance.CurrentRerollCost;
        _rerollCostText.text = rerollCost.ToString();
    }

    private void OnRerollBtnClick()
    {
        RogueManager.Instance.RefreshShop(true);
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

    /// <summary>
    /// 显示Unit选中
    /// </summary>
    /// <param name="unit"></param>
    private void OnHoverUnitDisplay(Unit unit)
    {
        var pos = UIManager.GetUIposBWorldPosition(unit.transform.position);
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
        if(unit != null)
        {
            unit.OutLineHighlight(false);
        }
    }

    private void OnShipPropertySwitchClick()
    {
        _propertyGroup.SwitchGroupType();
        if (_propertyGroup.CurrentGroupType == ShipPropertyGroupPanel.GroupType.Main)
        {
            _propertyBtnText.text = LocalizationManager.Instance.GetTextValue(PropertyBtnSwitch_Main);
        }
        else
        {
            _propertyBtnText.text = LocalizationManager.Instance.GetTextValue(PropertyBtnSwitch_Sub);
        }
    }

    private void RefreshShopLuckContent()
    {
        for (int i = 0; i < allShopSlotItems.Count; i++) 
        {
            allShopSlotItems[i].RefreshLuckPercentProperty();
        }
    }
}
