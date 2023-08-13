using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InventoryEvent
{
    public InventoryEventType type;
    public string name;

    public InventoryEvent(InventoryEventType m_type, string m_name)
    {
        type = m_type;

        name = m_name;
    }
    public static InventoryEvent e;
    public static void Trigger(InventoryEventType m_type, string m_name)
    {
        e.type = m_type;

        e.name = m_name;
        EventCenter.Instance.TriggerEvent<InventoryEvent>(e);
    }
}

[System.Serializable]
public class InventoryItem
{
    public bool iscountless;
    public int count;
    public BaseConfig itemconfig;

    public InventoryItem(BaseConfig m_item, int m_count = 1, bool m_iscountless = true)
    {
        iscountless = m_iscountless;
        count = m_count;
        itemconfig = m_item;
    }

    public int GetUpgradeGroupID()
    {
        if(itemconfig != null && itemconfig is BaseUnitConfig)
        {
            return (itemconfig as BaseUnitConfig).UpgradeGroupID;
        }
        return -1;
    }
}
