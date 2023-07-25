using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipBuilderHUD : GUIBasePanel, EventListener<InventoryEvent>
{


    public List<ItemGUISlot> buildingSlotList = new List<ItemGUISlot>();

    public RectTransform buildingSlotGroup;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void Initialization()
    {
        base.Initialization();
      
        
        this.EventStartListening<InventoryEvent>();
        GetGUIComponent<Button>("Launch").onClick.AddListener(OnLaunchBtnPressed);
        UpdateSlotList(buildingSlotList, GameManager.Instance.gameEntity.buildingInventory, UnitType.Buildings);
       


    }
    // Update is called once per frame
    void Update()
    {
        
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
}
