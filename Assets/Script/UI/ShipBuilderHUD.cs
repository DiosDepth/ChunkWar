using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ShipBuilderHUD : GUIBasePanel, EventListener<InventoryEvent>, EventListener<RogueEvent>
{
    public List<ItemGUISlot> buildingSlotList = new List<ItemGUISlot>();

    public RectTransform buildingSlotGroup;

    private Transform _shopContentRoot;
    private TextMeshProUGUI _currencyText;
    private RectTransform _currencyContent;
    private Text _rerollCostText;
    private EnhancedScroller _plugGridScroller;


    private const string ShipGoodsItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShopSlotItem";
    private const string ShipPlugGridItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShipPlugGroupItem";
    private List<ShopSlotItem> allShopSlotItems = new List<ShopSlotItem>();
    private List<ShipPropertyItemCmpt> propertyCmpts = new List<ShipPropertyItemCmpt>();
    private GeneralScrollerGirdItemController _plugGridController;

    protected override void Awake()
    {
        base.Awake();
        _shopContentRoot = transform.Find("ShopPanel/ShopContent");
        _currencyContent = transform.Find("ShopPanel/Top/ResCount").SafeGetComponent<RectTransform>();
        propertyCmpts = transform.Find("ShopPanel/PropertyPanel").GetComponentsInChildren<ShipPropertyItemCmpt>().ToList();
        _currencyText = _currencyContent.Find("CurrencyText").SafeGetComponent<TextMeshProUGUI>();
        _rerollCostText = GetGUIComponent<Text>("RerollCost");
        _plugGridScroller = transform.Find("ShipPlugSlots/Scroll View").SafeGetComponent<EnhancedScroller>();
    }

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<InventoryEvent>();
        this.EventStartListening<RogueEvent>();
        GetGUIComponent<Button>("Launch").onClick.AddListener(OnLaunchBtnPressed);
        GetGUIComponent<Button>("Reroll").onClick.AddListener(OnRerollBtnClick);
        UpdateSlotList(buildingSlotList, GameManager.Instance.gameEntity.buildingInventory, UnitType.Buildings);
        InitShopContent();
        RefreshGeneral();
    }

    public override void Hidden( )
    {
        this.EventStopListening<InventoryEvent>();
        base.Hidden();

    }

    public void OnLaunchBtnPressed()
    {
        GameStateTransitionEvent.Trigger(EGameState.EGameState_GameCompleted);
    }

    public void UpdateSlotList(List<ItemGUISlot> list, Inventory inventory,UnitType type)
    {
        if(inventory.IsEmpty())
        {
            return;
        }


        for (int i = 0; i < list.Count; i++)
        {
            Destroy(list[i].gameObject);
        }
        list.Clear();


        //foreach(KeyValuePair<string, InventoryItem> kv in inventory)
        //{
        //    switch (type)
        //    {
        //        case UnitType.Buildings:
        //            AddSlot(list, kv.Value, buildingSlotGroup);
        //            break;
        //    }

        //}

    }

    public void AddSlot(List<ItemGUISlot> list,InventoryItem m_item, RectTransform m_slotgroup)
    {
        var obj = ResManager.Instance.Load<GameObject>(UIManager.Instance.resPath + "ItemGUISlot");
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(m_slotgroup);
        rect.localScale = Vector3.one;


        var slotinfo = obj.GetComponent<ItemGUISlot>();
        list.Add(slotinfo);
        slotinfo.Initialization(list.Count - 1, uiGroup.GetComponent<RectTransform>(), m_item);
    }

    public void RemoveSlot(int m_index)
    {

    }

    public void OnEvent(InventoryEvent evt)
    {
        switch (evt.type)
        {
            case InventoryEventType.Checkin:
    
                break;
            case InventoryEventType.CheckOut:
 
                break;
            case InventoryEventType.Clear:
     
                break;
        }
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
        }
    }

    private void RefreshGeneral()
    {
        RefreshCurrency();
        RefreshReroll();
        propertyCmpts.ForEach(x => x.SetUp());
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
    /// ˢ�»���
    /// </summary>
    private void RefreshCurrency()
    {
        _currencyText.text = RogueManager.Instance.CurrentCurrency.ToString();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_currencyContent);
    }

    /// <summary>
    /// ˢ��������Ʒ����
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
}
