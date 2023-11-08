using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GoodsItemType
{
    ShipPlug,
    ShipUnit
}

public class GeneralPreviewItemSlot : MonoBehaviour, IHoverUIItem, IPoolable
{
    private GoodsItemType itemType;
    private GeneralUnlockItemType unlockItemType;

    private bool useUnlockType = false;

    private int itemID;
    private uint _itemUID;

    private DetailHoverItemBase _hoverItem;
    private bool _isHide = false;
    private bool _isShowing = false;

    public void Awake()
    {
        transform.SafeGetComponent<GeneralHoverItemControl>().item = this;
    }

    public void SetUp(GoodsItemType type, int itemID, uint itemUID)
    {
        this.itemType = type;
        this.itemID = itemID;
        this._itemUID = itemUID;

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

    public void SetUpUnlockItemPreview(GeneralUnlockItemType type, int itemID)
    {
        this.itemID = itemID;
        this.unlockItemType = type;
        useUnlockType = true;
        Sprite icon = null;
        Sprite frameBG = null;

        if (type ==  GeneralUnlockItemType.ShipPlug)
        {
            var plugCfg = DataManager.Instance.GetShipPlugItemConfig(itemID);
            if (plugCfg != null)
            {
                icon = plugCfg.GeneralConfig.IconSprite;
                frameBG = GameHelper.GetRarityBGSprite(plugCfg.GeneralConfig.Rarity);
            }
        }
        else if (type ==  GeneralUnlockItemType.ShipUnit)
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

    public void SetHide()
    {
        _isHide = true;
        transform.SafeGetComponent<CanvasGroup>().alpha = 0;
    }

    public async UniTask DoShow()
    {
        if (_isShowing || !_isHide)
            return;

        _isShowing = true;
        transform.GetComponent<Animation>().Play();
        await UniTask.Delay(500);
        _isHide = false;
        _isShowing = false;
    }

    public void OnHoverEnter()
    {
        if (_isHide)
            return;

        if (itemType == GoodsItemType.ShipUnit || (useUnlockType && unlockItemType == GeneralUnlockItemType.ShipUnit))
        {
            UIManager.Instance.CreatePoolerUI<UnitDetailHover>("UnitDetailHover", true, E_UI_Layer.Top, null, (panel) =>
            {
                panel.Initialization(itemID, _itemUID);
                _hoverItem = panel;
            });
        } 
        else if (itemType == GoodsItemType.ShipPlug || (useUnlockType && unlockItemType == GeneralUnlockItemType.ShipPlug))
        {
            UIManager.Instance.CreatePoolerUI<PlugDetailHover>("PlugDetailHover", true, E_UI_Layer.Top, null, (panel) =>
            {
                panel.Initialization(itemID);
                _hoverItem = panel;
            });
        }
    }

    public void OnHoverExit()
    {
        DestroyHover();
    }

    public void PoolableReset()
    {
        itemID = 0;
        useUnlockType = false;
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        DestroyHover();
        PoolManager.Instance.BackObject(transform.name, gameObject);
    }

    public void PoolableSetActive(bool isactive = true)
    {

    }

    private void DestroyHover()
    {
        if (_hoverItem == null)
            return;

        _hoverItem.PoolableDestroy();
    }
}
