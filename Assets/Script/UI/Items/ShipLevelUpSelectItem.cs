using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShipLevelUpSelectItem : MonoBehaviour, IPoolable, IHoverUIItem
{
    private Image _icon;
    private TextMeshProUGUI _nameText;
    private ItemPropertyModifyCmpt _propertyCmpt;
    private Transform _hoverObj;

    private ShipLevelUpItem _levelItem;
    private Action onClickAction;

    public void Awake()
    {
        _hoverObj = transform.Find("Hover");
        _icon = transform.Find("Content/Icon/Image").SafeGetComponent<Image>();
        _nameText = transform.Find("Content/Name").SafeGetComponent<TextMeshProUGUI>();
        _propertyCmpt = transform.Find("Content/ShipLevelUpProperty").SafeGetComponent<ItemPropertyModifyCmpt>();
        transform.SafeGetComponent<GeneralHoverItemControl>().item = this;
        _hoverObj.SafeSetActive(false);
        transform.Find("BG").SafeGetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void SetUp(ShipLevelUpItem item, Action onClickAction)
    {
        this.onClickAction = onClickAction;
        this._levelItem = item;
        _icon.sprite = item.Config.Icon;
        _nameText.text = LocalizationManager.Instance.GetTextValue(item.Config.Name);
        _propertyCmpt.SetUp(item.Config.ModifyKey, item.GetModifyValue(), false);
        transform.Find("Frame").SafeGetComponent<Image>().sprite = GameHelper.GetShipLevelUpRarityFrameSprite(item.Rarity);
        transform.Find("Content/Icon/Rarity").SafeGetComponent<Image>().sprite = GameHelper.GetRarityBGSprite(item.Rarity);
    }

    private void OnClick()
    {
        RogueManager.Instance.SelectShipLevelUpItem(_levelItem);
        onClickAction?.Invoke();
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableReset()
    {

    }

    public void PoolableSetActive(bool isactive = true)
    {
        transform.SafeSetActive(isactive);
    }

    public void OnHoverEnter()
    {
        _hoverObj.SafeSetActive(true);
        LeanTween.moveLocalY(gameObject, 30, 0.1f);
    }

    public void OnHoverExit()
    {
        _hoverObj.SafeSetActive(false);
        LeanTween.moveLocalY(gameObject, 0, 0.1f);
    }
}
