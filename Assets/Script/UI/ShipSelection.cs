using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShipSelection : GUIBasePanel, EventListener<InventoryOperationEvent>
{


    public RectTransform slotGroup;
    public List<ItemGUISlot> shipSlotList = new List<ItemGUISlot>();
    public Image icon;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    public override void Initialization()
    {
        base.Initialization();

        GetGUIComponent<Button>("Apply").onClick.AddListener(ApplyBtnPressed);
        GetGUIComponent<Button>("Back").onClick.AddListener(BackBtnPressed);

        this.EventStartListening<InventoryOperationEvent>();
        UpdateSlotList(shipSlotList, (owner as ShipSelectionLevel).shipInventory);
        shipSlotList[0].slotbtn.Select();

        icon.sprite = shipSlotList[0].item.itemconfig.Icon;
        InventoryOperationEvent.Trigger(InventoryOperationType.ItemSelect, shipSlotList[0].slotIndex, shipSlotList[0].item.itemconfig.UnitName);
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void ApplyBtnPressed()
    {
        //LeanTween.alpha(root, 0, 0.25f);
        GameStateTransitionEvent.Trigger(EGameState.EGameState_GamePrepare);
    }

    public void BackBtnPressed()
    {
        //LeanTween.alpha(root, 0, 0.25f);
        GameStateTransitionEvent.Trigger(EGameState.EGameState_MainMenu);
    }




    public override void Hidden( )
    {

        this.EventStopListening<InventoryOperationEvent>();
        base.Hidden();


    }

    public void UpdateSlotList(List<ItemGUISlot> list, Inventory inventory)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Destroy(list[i].gameObject);
        }
        list.Clear();
        
        foreach (KeyValuePair<string, InventoryItem> kv in inventory)
        {
            AddSlot(shipSlotList, kv.Value, slotGroup);
        }

    }


    public void AddSlot(List<ItemGUISlot> list, InventoryItem m_item, RectTransform m_slotgroup)
    {
        var obj = ResManager.Instance.Load<GameObject>(UIManager.Instance.resPath + "CmptItems/ShipBuildingSlot");
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

    public void OnEvent(InventoryOperationEvent evt)
    {
        switch (evt.type)
        {
            case InventoryOperationType.ItemSelect:
                icon.sprite = (owner as ShipSelectionLevel).shipInventory.Peek(evt.name).itemconfig.Icon;
                break;
            case InventoryOperationType.ItemUnselect:
                break;
            case InventoryOperationType.ItemRemove:
                break;
        }
    }
}