using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlotItem : MonoBehaviour
{
    private Image _icon;
    private Text _nameText;
    private TextMeshProUGUI _descText;
    private Text _costText;

    private ShopGoodsInfo _goodsInfo;
    private Button _buyButton;

    private void Awake()
    {
        _icon = transform.Find("Content/Icon").GetComponent<Image>();
        _nameText = transform.Find("Content/Name").GetComponent<Text>();
        _descText = transform.Find("Content/Desc").GetComponent<TextMeshProUGUI>();
        _costText = transform.Find("Content/Cost/Value").GetComponent<Text>();
        _buyButton = transform.Find("Functions/Buy").GetComponent<Button>();
        _buyButton.onClick.AddListener(OnBuyButtonClick);
        transform.Find("Functions/Lock").GetComponent<Button>().onClick.AddListener(OnBuyButtonClick);
        SetSold(false);
    }

    public void SetUp(ShopGoodsInfo info)
    {
        _goodsInfo = info;
        if (info == null)
            return;

        _nameText.text = info.ItemName;
        _descText.text = info.ItemDesc;
        _icon.sprite = info._cfg.IconSprite;
        _costText.text = info.Cost.ToString();
        _buyButton.interactable = info.CheckCanBuy();
    }

    private void OnBuyButtonClick()
    {
        if (_goodsInfo == null)
            return;

        if (RogueManager.Instance.BuyItem(_goodsInfo))
        {
            ///Set Buy
            SetSold(true);
        }
    }

    private void OnLockButtonClick()
    {

    }

    private void SetSold(bool sold)
    {
        transform.Find("Content").SafeSetActive(!sold);
        transform.Find("Sold").SafeSetActive(sold);
        transform.Find("Functions").SafeSetActive(!sold);
    }
}
