using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MT_ItemRarityCount : ModifyTriggerData
{
    private MTC_ByItemRarityCount _itemCfg;

    public int ItemCount
    {
        get;
        private set;
    }

    public MT_ItemRarityCount(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _itemCfg = cfg as MTC_ByItemRarityCount;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        RogueManager.Instance.OnItemCountChange += OnItemCountChange;
    }

    public override void OnTriggerRemove()
    {
        base.OnTriggerRemove();
        RogueManager.Instance.OnItemCountChange -= OnItemCountChange;
    }

    private void OnItemCountChange()
    {
        ItemCount = 0;
        if (_itemCfg.Type == MTC_ByItemRarityCount.ItemType.Plug || _itemCfg.Type == MTC_ByItemRarityCount.ItemType.All)
        {
            for (int i = 0; i < _itemCfg.VaildRarity.Count; i++) 
            {
                ItemCount += RogueManager.Instance.GetPlugCountByItemRarity(_itemCfg.VaildRarity[i]);
            }
        }

        if(_itemCfg.Type == MTC_ByItemRarityCount.ItemType.Unit || _itemCfg.Type == MTC_ByItemRarityCount.ItemType.All)
        {
            for (int i = 0; i < _itemCfg.VaildRarity.Count; i++)
            {
                ItemCount += RogueManager.Instance.GetShipUnitCountByItemRarity(_itemCfg.VaildRarity[i]);
            }
        }

        Trigger();
    }

    
}
