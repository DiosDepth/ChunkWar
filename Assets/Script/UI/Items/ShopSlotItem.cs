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
    private Transform PropertyModifyRoot;

    private ShopGoodsInfo _goodsInfo;
    private Button _buyButton;

    private static Color _costNormalColor = new Color(1f, 1f, 1f);
    private static Color _costRedColor = new Color(0.6f, 0, 0);
    private const string ShopPropertyItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShopItemProperty";

    private void Awake()
    {
        PropertyModifyRoot = transform.Find("Content/PropertyModify");
        _icon = transform.Find("Content/Info/Icon").GetComponent<Image>();
        _nameText = transform.Find("Content/Info/Detail/Name").GetComponent<Text>();
        _descText = transform.Find("Content/Desc").GetComponent<TextMeshProUGUI>();
        _costText = transform.Find("Content/Buy/Value").GetComponent<Text>();
        _buyButton = transform.Find("Content/Buy").GetComponent<Button>();
        _buyButton.onClick.AddListener(OnBuyButtonClick);
        transform.Find("Functions/Lock").GetComponent<Button>().onClick.AddListener(OnLockButtonClick);
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

        bool costEnough = info.CostEnough;
        _costText.color = costEnough ? _costNormalColor : _costRedColor;
        _buyButton.interactable = info.CheckCanBuy();
        SetUpProperty();
        SetSold(false);
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

    private void SetUpProperty()
    {
        var type = _goodsInfo._cfg.ItemType;
        if(type == GoodsItemType.ShipPlug)
        {
            var plugCfg = DataManager.Instance.GetShipPlugItemConfig(_goodsInfo._cfg.TypeID);
            if(plugCfg != null)
            {
                SetUpPropertyCmpt(plugCfg.PropertyModify);
            }
        }
    }

    private void SetUpPropertyCmpt(List<PropertyMidifyConfig> cfgs)
    {
        PropertyModifyRoot.Pool_BackAllChilds(ShopPropertyItem_PrefabPath);
        if (cfgs == null || cfgs.Count <= 0)
            return;

        var index = 0;
        for (int i = 0; i < cfgs.Count; i++)
        {
            PoolManager.Instance.GetObjectAsync(ShopPropertyItem_PrefabPath, true, (obj) =>
            {
                var cmpt = obj.GetComponent<ShopItemPropertyCmpt>();
                cmpt.SetUp(cfgs[index].ModifyKey, cfgs[index].Value);
                index++;
            }, PropertyModifyRoot);
        }
    }
}
