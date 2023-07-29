using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipBuilderHUD : GUIBasePanel, EventListener<InventoryEvent>, EventListener<RogueEvent>
{
    public List<ItemGUISlot> buildingSlotList = new List<ItemGUISlot>();

    public RectTransform buildingSlotGroup;

    private Transform _shopContentRoot;
    private TextMeshProUGUI _currencyText;
    private RectTransform _currencyContent;
    private Text _rerollCostText;


    private const string ShipGoodsItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShopSlotItem";
    private List<ShopSlotItem> allShopSlotItems = new List<ShopSlotItem>();

    protected override void Awake()
    {
        base.Awake();
        _shopContentRoot = transform.Find("uiGroup/ShopPanel/ShopContent");
        _currencyContent = transform.Find("uiGroup/ShopPanel/Top/ResCount").SafeGetComponent<RectTransform>();
        _currencyText = _currencyContent.Find("CurrencyText").SafeGetComponent<TextMeshProUGUI>();
        _rerollCostText = GetGUIComponent<Text>("RerollCost");
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


        foreach(KeyValuePair<string, InventoryItem> kv in inventory)
        {
            switch (type)
            {
                case UnitType.Buildings:
                    AddSlot(list, kv.Value, buildingSlotGroup);
                    break;
            }

        }

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
        }
    }

    private void RefreshGeneral()
    {
        RefreshCurrency();
        RefreshReroll();
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
    /// Ë¢ÐÂ»õ±Ò
    /// </summary>
    private void RefreshCurrency()
    {
        _currencyText.text = RogueManager.Instance.CurrentCurrency.ToString();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_currencyContent);
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
}
