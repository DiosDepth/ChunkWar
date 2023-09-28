using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneralPreviewItemSlot : MonoBehaviour, IHoverUIItem
{
    private GoodsItemType itemType;
    private int itemID;

    public void Awake()
    {
        transform.SafeGetComponent<GeneralHoverItemControl>().item = this;
    }

    public void SetUp(GoodsItemType type, int itemID)
    {
        this.itemType = type;
        this.itemID = itemID;

        Sprite icon = null;
        Sprite frameBG = null;

        if (type == GoodsItemType.ShipPlug)
        {
            var plugCfg = DataManager.Instance.GetShipPlugItemConfig(itemID);
            if(plugCfg != null)
            {
                icon = plugCfg.GeneralConfig.IconSprite;
                frameBG = GameHelper.GetRarityBGSprite(plugCfg.GeneralConfig.Rarity);
            }
        }
        else if(type == GoodsItemType.ShipUnit)
        {
            var unitCfg = DataManager.Instance.GetUnitConfig(itemID);
            if (unitCfg != null)
            {
                icon = unitCfg.GeneralConfig.IconSprite;
                frameBG = GameHelper.GetRarityBGSprite(unitCfg.GeneralConfig.Rarity);
            }
        }
        transform.Find("Icon").SafeGetComponent<Image>().sprite = icon;
        transform.Find("FrameBG").SafeGetComponent<Image>().sprite = frameBG;
    }

    public void OnHoverEnter()
    {

    }

    public void OnHoverExit()
    {

    }
}
