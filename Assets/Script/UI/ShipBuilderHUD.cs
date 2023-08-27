using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ShipBuilderHUD : GUIBasePanel, EventListener<RogueEvent>
{
    private Transform _shopContentRoot;
    private TextMeshProUGUI _currencyText;
    private RectTransform _currencyContent;
    private Text _rerollCostText;
    private EnhancedScroller _plugGridScroller;
    private UnitDetailInfoPanel _detailPanel;
    private BuildSelectHoverCmpt _hoverCmpt;


    private const string ShipGoodsItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShopSlotItem";
    private const string ShipPlugGridItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShipPlugGroupItem";
    private List<ShopSlotItem> allShopSlotItems = new List<ShopSlotItem>();
    /// <summary>
    /// PropertyCmpt
    /// </summary>
    private List<ShipPropertyItemCmpt> propertyCmpts = new List<ShipPropertyItemCmpt>();
    private List<ShipPropertyItemCmpt> propertySubCmpts = new List<ShipPropertyItemCmpt>();
    private CanvasGroup _mainPropertyCanvas;
    private CanvasGroup _subPropertyCanvas;

    private GeneralScrollerGirdItemController _plugGridController;

    protected override void Awake()
    {
        base.Awake();
        _shopContentRoot = transform.Find("ShopPanel/ShopContent");
        _hoverCmpt = transform.Find("BuildSelectHover").SafeGetComponent<BuildSelectHoverCmpt>();
        _currencyContent = transform.Find("ShopPanel/Top/ResCount").SafeGetComponent<RectTransform>();
        _mainPropertyCanvas = transform.Find("ShopPanel/PropertyGroup/PropertyPanel").SafeGetComponent<CanvasGroup>();
        _subPropertyCanvas = transform.Find("ShopPanel/PropertyGroup/PropertySubPanel").SafeGetComponent<CanvasGroup>();
        propertyCmpts = _mainPropertyCanvas.GetComponentsInChildren<ShipPropertyItemCmpt>().ToList();
        propertySubCmpts = _subPropertyCanvas.GetComponentsInChildren<ShipPropertyItemCmpt>().ToList();
        _currencyText = _currencyContent.Find("CurrencyText").SafeGetComponent<TextMeshProUGUI>();
        _rerollCostText = GetGUIComponent<Text>("RerollCost");
        _plugGridScroller = transform.Find("ShipPlugSlots/Scroll View").SafeGetComponent<EnhancedScroller>();
        _detailPanel = transform.Find("UnitDetailPanel/Content").SafeGetComponent<UnitDetailInfoPanel>();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<RogueEvent>();
        GetGUIComponent<Button>("Launch").onClick.AddListener(OnLaunchBtnPressed);
        GetGUIComponent<Button>("Reroll").onClick.AddListener(OnRerollBtnClick);
        GetGUIComponent<Button>("MainPropertyBtn").onClick.AddListener(OnMainPropertyBtnClick);
        GetGUIComponent<Button>("SubPropertyBtn").onClick.AddListener(OnSubPropertyBtnClick);
        _detailPanel.Hide();
        _hoverCmpt.SetActive(false);
        InitShopContent();
        RefreshGeneral();
        OnMainPropertyBtnClick();
    }

    public override void Hidden( )
    {
        this.EventStopListening<RogueEvent>();
        base.Hidden();
    }

    public void OnLaunchBtnPressed()
    {
        GameStateTransitionEvent.Trigger(EGameState.EGameState_GameCompleted);
    }

    public void RemoveSlot(int m_index)
    {

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

            case RogueEventType.RefreshWreckage:

                break;

            case RogueEventType.RefreshShopWeaponInfo:
                UI_WeaponUnitPropertyType type = (UI_WeaponUnitPropertyType)evt.param[0];
                RefreshShopWeaponItemProperty(type);
                break;

            case RogueEventType.ShowUnitDetailPage:
                BaseUnitConfig cfg = (BaseUnitConfig)evt.param[0];
                ShowUnitDetailPanel(cfg);
                break;

            case RogueEventType.HideUnitDetailPage:
                HideUnitDetailPage();
                break;

            case RogueEventType.HoverUnitDisplay:
                OnHoverUnitDisplay((Unit)evt.param[0]);
                break;

            case RogueEventType.HideHoverUnitDisplay:
                OnHideHoverUnitDisplay((Unit)evt.param[0]);
                break;

            case RogueEventType.HoverUnitUpgradeDisplay:
                Unit basedUnit = (Unit)evt.param[0];
                InventoryItem item = (InventoryItem)evt.param[1];
                OnShowUpgradeUnitDisplayInfo(basedUnit, item);
                break;
        }
    }

    private void RefreshGeneral()
    {
        RefreshCurrency();
        RefreshReroll();
        InitPlugController();
    }

    private void InitPlugController()
    {
        _plugGridController = new GeneralScrollerGirdItemController();
        _plugGridController.numberOfCellsPerRow = 9;
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
        RogueManager.Instance.RefreshShop();
    }

    private void OnMainPropertyBtnClick()
    {
        _mainPropertyCanvas.ActiveCanvasGroup(true);
        _subPropertyCanvas.ActiveCanvasGroup(false);
        propertyCmpts.ForEach(x => x.SetUp());
    }

    private void OnSubPropertyBtnClick()
    {
        _mainPropertyCanvas.ActiveCanvasGroup(false);
        _subPropertyCanvas.ActiveCanvasGroup(true);
        propertySubCmpts.ForEach(x => x.SetUp());
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

    private void RefreshShopWeaponItemProperty(UI_WeaponUnitPropertyType type)
    {
        for(int i = 0; i < allShopSlotItems.Count; i++)
        {
            var item = allShopSlotItems[i];
            if(item.ItemType == GoodsItemType.ShipUnit)
            {
                item.RefreshWeaponProperty(type);
            }
        }
    }

    private void ShowUnitDetailPanel(BaseUnitConfig cfg)
    {
        _detailPanel.Show();
        _detailPanel.SetUpInfo(cfg, 2);
    }

    private void HideUnitDetailPage()
    {
        _detailPanel.Hide();
    }

    /// <summary>
    /// 显示Unit选中
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
        if(unit != null)
        {
            unit.OutLineHighlight(false);
        }
    }

    /// <summary>
    /// 展示升级预览
    /// </summary>
    private void OnShowUpgradeUnitDisplayInfo(Unit unit, InventoryItem item)
    {
        OnHoverUnitDisplay(unit);
        _hoverCmpt.SetUpUpgradePreivew(item.itemconfig);
    }

}
