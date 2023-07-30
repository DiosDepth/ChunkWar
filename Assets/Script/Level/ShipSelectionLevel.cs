using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSelectionLevel : LevelEntity, EventListener<InventoryOperationEvent>
{
    
    public Inventory shipInventory;
    public InventoryItem currentShipSelection;

    public override void Initialization()
    {
        base.Initialization();
        this.EventStartListening<InventoryOperationEvent>();
        foreach(KeyValuePair<string, ShipConfig> kv in  DataManager.Instance.ShipConfigDic)
        {
            shipInventory.CheckIn(new InventoryItem(kv.Value));
        }
        
    }

    public override void Unload()
    {
        base.Unload();
        this.EventStopListening<InventoryOperationEvent>();
        RogueManager.Instance.currentShipSelection = currentShipSelection;
        

    }


    public void OnEvent(InventoryOperationEvent evt)
    {
        switch (evt.type)
        {
            case InventoryOperationType.ItemSelect:

                currentShipSelection  = shipInventory.Peek(evt.name);
                break;
            case InventoryOperationType.ItemUnselect:
                break;
            case InventoryOperationType.ItemRemove:
                break;
        }
    }
}