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

    private ShopGoodsInfo _goodsInfo;

    private void Awake()
    {
        _icon = transform.Find("Content/Icon").GetComponent<Image>();
        _nameText = transform.Find("Content/Name").GetComponent<Text>();
        _descText = transform.Find("Content/Desc").GetComponent<TextMeshProUGUI>();
        transform.Find("Functions/Buy").GetComponent<Button>().onClick.AddListener(OnBuyButtonClick);
        transform.Find("Functions/Lock").GetComponent<Button>().onClick.AddListener(OnBuyButtonClick);
    }

    public void SetUp(ShopGoodsInfo info)
    {
        _goodsInfo = info;
        if (info == null)
            return;

        _nameText.text = info.ItemName;
        _descText.text = info.ItemDesc;
        _icon.sprite = info._cfg.IconSprite;
    }

    private void OnBuyButtonClick()
    {

    }

    private void OnLockButtonClick()
    {

    }
}
