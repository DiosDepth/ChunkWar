using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ShopHUD : GUIBasePanel, EventListener<RogueEvent>
{
    private Transform _shopContentRoot;
    private TextMeshProUGUI _currencyText;
    private RectTransform _currencyContent;
    private TextMeshProUGUI _rerollCostText;
    private EnhancedScroller _plugGridScroller;
    private BuildSelectHoverCmpt _hoverCmpt;


    private const string ShipGoodsItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShopSlotItem";
    private const string ShipPlugGridItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShipPlugGroupItem";
    private List<ShopSlotItem> allShopSlotItems = new List<ShopSlotItem>();

    private GeneralScrollerGirdItemController _plugGridController;

    protected override void Awake()
    {
        base.Awake();
        _shopContentRoot = transform.Find("ShopPanel/Content/Shop");
        _hoverCmpt = transform.Find("BuildSelectHover").SafeGetComponent<BuildSelectHoverCmpt>();
        _currencyContent = transform.Find("ShopPanel/Top/ResCount").SafeGetComponent<RectTransform>();
        _currencyText = _currencyContent.Find("CurrencyText").SafeGetComponent<TextMeshProUGUI>();
        _rerollCostText = transform.Find("ShopPanel/Top/Reroll/Reroll/RerollCost").SafeGetComponent<TextMeshProUGUI>();
        _plugGridScroller = transform.Find("ShipPlugSlots/Scroll View").SafeGetComponent<EnhancedScroller>();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<RogueEvent>();
        GetGUIComponent<Button>("Launch").onClick.AddListener(OnLaunchBtnPressed);
        GetGUIComponent<Button>("Reroll").onClick.AddListener(OnRerollBtnClick);
        _hoverCmpt.SetActive(false);
        InitShopContent();
        RefreshGeneral();
    }

    public override void Hidden( )
    {
        this.EventStopListening<RogueEvent>();
        base.Hidden();
    }

    public void OnLaunchBtnPressed()
    {
        GameStateTransitionEvent.Trigger(EGameState.EGameState_GameHarbor);
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

            case RogueEventType.HoverUnitDisplay:
                OnHoverUnitDisplay((Unit)evt.param[0]);
                break;

            case RogueEventType.HideHoverUnitDisplay:
                OnHideHoverUnitDisplay((Unit)evt.param[0]);
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

}
