using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopSlotItem : MonoBehaviour
{
    private int itemTypeID;

    private Image _icon;
    private Image _rarityBG;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _descText;
    private TextMeshProUGUI _costText;
    private TextMeshProUGUI _limitText;
    private TextMeshProUGUI _effectDesc1;
    private TextMeshProUGUI _effectDesc2;

    private Transform PropertyModifyRoot;
    private RectTransform _tagRoot;
    private RectTransform _detailContentRect;

    private ShopGoodsInfo _goodsInfo;
    private ScrollRect _detailScroll;
    private Button _buyButton;
    private Transform _unitInfoRoot;
    private Transform LimitInfoTrans;

    private static Color _costNormalColor = new Color(1f, 1f, 1f);
    private static Color _costRedColor = new Color(0.6f, 0, 0);
    private float ScrollHeight;

    private const string ShopPropertyItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShopItemProperty";
    private const string UnitInfo_PropertyItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/UnitPropertyItem";
    private const string ItemTag_PrefabPath = "Prefab/GUIPrefab/PoolUI/ItemTagCmpt";

    private const string ShopGoods_DiscountText = "ShopGoods_DiscountText";
    private void Awake()
    {
        _detailScroll = transform.Find("Content/DetailInfo").SafeGetComponent<ScrollRect>();
        ScrollHeight = _detailScroll.transform.SafeGetComponent<RectTransform>().rect.height;

        LimitInfoTrans = transform.Find("Content/Info/Detail/LimitInfo");
        _limitText = LimitInfoTrans.Find("Content/Value").SafeGetComponent<TextMeshProUGUI>();
        _detailContentRect = transform.Find("Content/DetailInfo/Viewport/Content").SafeGetComponent<RectTransform>();
        _tagRoot = transform.Find("Content/Info/Detail/TypeInfo").SafeGetComponent<RectTransform>();
        _icon = transform.Find("Content/Info/Icon/Image").GetComponent<Image>();
        _rarityBG = transform.Find("BG").SafeGetComponent<Image>();
        _nameText = transform.Find("Content/Info/Detail/Name").GetComponent<TextMeshProUGUI>();
        _effectDesc1 = _detailContentRect.Find("EffectDesc").SafeGetComponent<TextMeshProUGUI>();
        _effectDesc2 = _detailContentRect.Find("EffectDesc/Desc").SafeGetComponent<TextMeshProUGUI>();

        PropertyModifyRoot = _detailContentRect.Find("PropertyModify");
        _unitInfoRoot = _detailContentRect.Find("UnitInfo");
        _descText = _detailContentRect.Find("Desc").GetComponent<TextMeshProUGUI>();
        _costText = transform.Find("Content/Buy/Value").GetComponent<TextMeshProUGUI>();
        _buyButton = transform.Find("Content/Buy").GetComponent<Button>();
        _buyButton.onClick.AddListener(OnBuyButtonClick);
        transform.Find("Lock").GetComponent<Button>().onClick.AddListener(OnLockButtonClick);
    }

    public void SetUp(ShopGoodsInfo info)
    {
        _goodsInfo = info;
        if (info == null)
            return;

        itemTypeID = info._cfg.TypeID;
        var plugCfg = DataManager.Instance.GetShipPlugItemConfig(itemTypeID);
        if (plugCfg == null)
            return;

        _rarityBG.sprite = GameHelper.GetRarityBG_Big(plugCfg.GeneralConfig.Rarity);
        _nameText.color = GameHelper.GetRarityColor(plugCfg.GeneralConfig.Rarity);
        SetEffectDesc(LocalizationManager.Instance.GetTextValue(plugCfg.EffectDesc), LocalizationManager.Instance.GetTextValue(plugCfg.GeneralConfig.Desc));
        SetUpProperty();
        SetUpBuyLimit(_goodsInfo);
        _nameText.text = LocalizationManager.Instance.GetTextValue(plugCfg.GeneralConfig.Name);
        _icon.sprite = plugCfg.GeneralConfig.IconSprite;

        SetUpDiscount();
        RefreshCost();
        SetUpTag(plugCfg);
        SetSold(false);
        SetLockState(info.IsLock);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_detailContentRect);

        ///设置是否可以滑动
        bool enableScroll = _detailContentRect.rect.height > ScrollHeight;
        _detailScroll.transform.SafeGetComponent<Image>().raycastTarget = enableScroll;
        _detailScroll.vertical = enableScroll;
    }

    /// <summary>
    /// 刷新武器信息的指定属性
    /// </summary>
    /// <param name="type"></param>
    public void RefreshWeaponProperty(UI_WeaponUnitPropertyType type)
    {
        Func<List<UnitPropertyItemCmpt>, UI_WeaponUnitPropertyType, UnitPropertyItemCmpt> FindCmptByType = (cmpts, type) =>
       {
           return cmpts.Find(x => x.WeaponPropertyType == type);
       };

        var unitCfg = DataManager.Instance.GetUnitConfig(itemTypeID);
        var allCmpt = _unitInfoRoot.GetComponentsInChildren<UnitPropertyItemCmpt>().ToList();

        if (unitCfg.unitType == UnitType.Weapons)
        {
            WeaponConfig weaponCfg = unitCfg as WeaponConfig;
            if (weaponCfg == null)
                return;

            var cmpt = FindCmptByType(allCmpt, type);
            if(cmpt != null)
            {
                var content = GameHelper.GetWeaponPropertyDescContent(type, weaponCfg);
                cmpt.RefreshContent(content);
            }
        }
    }

    /// <summary>
    /// 刷新花费
    /// </summary>
    public void RefreshCost()
    {
        if (_goodsInfo == null)
            return;

        _costText.text = _goodsInfo.Cost.ToString();
        bool costEnough = _goodsInfo.CostEnough;
        _costText.color = costEnough ? _costNormalColor : _costRedColor;
        _buyButton.interactable = _goodsInfo.CheckCanBuy();
    }

    private void OnBuyButtonClick()
    {
        if (_goodsInfo == null)
            return;

        if (RogueManager.Instance.BuyItem(_goodsInfo))
        {
            ///Set Buy
            SetSold(true);
            SetUpBuyLimit(_goodsInfo);
        }
    }

    private void OnLockButtonClick()
    {
        bool lockState = RogueManager.Instance.ChangeShopItemLockState(_goodsInfo);
        SetLockState(lockState);
    }

    private void SetLockState(bool isLock)
    {
        ///Lock
        transform.Find("Lock/Text").transform.SafeSetActive(isLock);
    }

    private void SetUpDiscount()
    {
        var disCountTrans = transform.Find("Content/DisCount");
        disCountTrans.SafeSetActive(_goodsInfo.DiscountValue != 0);
        if(_goodsInfo.DiscountValue != 0)
        {
            disCountTrans.Find("Value").SafeGetComponent<TextMeshProUGUI>().text = string.Format("{0}{1}", _goodsInfo.DiscountValue, LocalizationManager.Instance.GetTextValue(ShopGoods_DiscountText));
        }
    }

    private void SetEffectDesc(string effectText, string descText)
    {
        if (string.IsNullOrEmpty(effectText))
        {
            _effectDesc1.transform.SafeSetActive(false);
        }
        else
        {
            _effectDesc1.text = effectText;
            _effectDesc2.text = effectText;
            _effectDesc1.transform.SafeSetActive(true);
        }

        if (string.IsNullOrEmpty(descText))
        {
            _descText.transform.SafeSetActive(false);
        }
        else
        {
            _descText.text = descText;
            _descText.transform.SafeSetActive(true);
        }
    }

    private void SetUpUintInfo(BaseUnitConfig cfg)
    {
        _unitInfoRoot.Pool_BackAllChilds(UnitInfo_PropertyItem_PrefabPath);
        if(cfg.unitType == UnitType.Weapons)
        {
            WeaponConfig weaponCfg = cfg as WeaponConfig;
            if (weaponCfg == null)
                return;

            foreach(UI_WeaponUnitPropertyType type in System.Enum.GetValues(typeof(UI_WeaponUnitPropertyType)))
            {
                if (type == UI_WeaponUnitPropertyType.ShieldTransfixion)
                    continue;

                if (!weaponCfg.UseDamageRatio && type == UI_WeaponUnitPropertyType.DamageRatio)
                    continue;

                PoolManager.Instance.GetObjectSync(UnitInfo_PropertyItem_PrefabPath, true, (obj) =>
                {
                    string content = GameHelper.GetWeaponPropertyDescContent(type, weaponCfg);
                    var cmpt = obj.GetComponent<UnitPropertyItemCmpt>();
                    cmpt.SetUpWeapon(type, content);
                }, _unitInfoRoot);
            }
        }
    }

    private void SetSold(bool sold)
    {
        _buyButton.interactable = !sold;

        transform.Find("Sold").SafeSetActive(sold);
        transform.Find("Lock").SafeSetActive(!sold);
    }

    private void SetUpProperty()
    {
        var plugCfg = DataManager.Instance.GetShipPlugItemConfig(_goodsInfo._cfg.TypeID);
        PropertyModifyRoot.Pool_BackAllChilds(ShopPropertyItem_PrefabPath);
        if (plugCfg != null)
        {
            SetUpPropertyCmpt(plugCfg.PropertyModify, false);
            SetUpPropertyCmpt(plugCfg.PropertyPercentModify, true);
        }

        var rect = PropertyModifyRoot.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }

    private void SetUpPropertyCmpt(PropertyModifyConfig[] cfgs, bool modifyPercent)
    {
        if (cfgs == null || cfgs.Length <= 0)
            return;

        var index = 0;
        for (int i = 0; i < cfgs.Length; i++)
        {
            if (cfgs[i].BySpecialValue)
                continue;

            PoolManager.Instance.GetObjectSync(ShopPropertyItem_PrefabPath, true, (obj) =>
            {
                var cmpt = obj.GetComponent<ItemPropertyModifyCmpt>();
                cmpt.SetUp(cfgs[index].ModifyKey, cfgs[index].Value, modifyPercent);
                index++;
            }, PropertyModifyRoot);
        }
    }

    private void SetUpBuyLimit(ShopGoodsInfo info)
    {
        var limit = info.GetBuyLimit;
        if(limit <= -1)
        {
            LimitInfoTrans.SafeSetActive(false);
            return;
        }
        var currentCount = RogueManager.Instance.GetCurrentPlugCount(itemTypeID);
        _limitText.text = string.Format("{0}/{1}", currentCount, limit);
        LimitInfoTrans.SafeSetActive(true);
    }

    private void SetUpTag(ShipPlugItemConfig cfg)
    {
        _tagRoot.Pool_BackAllChilds(ItemTag_PrefabPath);

        foreach (ItemTag tag in System.Enum.GetValues(typeof(ItemTag)))
        {
            if (cfg.HasUnitTag(tag))
            {
                PoolManager.Instance.GetObjectSync(ItemTag_PrefabPath, true, (obj) =>
                {
                    var cmpt = obj.transform.SafeGetComponent<ItemTagCmpt>();
                    cmpt.SetUp(tag);
                }, _tagRoot);
            }
        }

        if(_tagRoot.childCount == 0)
        {
            _tagRoot.transform.SafeSetActive(false);
        }
        else
        {
            _tagRoot.transform.SafeSetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_tagRoot);
        }
    }
}
