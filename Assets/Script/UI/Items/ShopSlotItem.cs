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
    private Text _typeText;

    private ShopGoodsInfo _goodsInfo;
    private Button _buyButton;
    private Transform _unitInfoRoot;

    private static Color _costNormalColor = new Color(1f, 1f, 1f);
    private static Color _costRedColor = new Color(0.6f, 0, 0);
    private const string ShopPropertyItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShopItemProperty";
    private const string UnitInfo_PropertyItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/UnitPropertyItem";

    private void Awake()
    {
        PropertyModifyRoot = transform.Find("Content/PropertyModify");
        _unitInfoRoot = transform.Find("Content/UnitInfo");
        _icon = transform.Find("Content/Info/Icon").GetComponent<Image>();
        _nameText = transform.Find("Content/Info/Detail/Name").GetComponent<Text>();
        _descText = transform.Find("Content/Desc").GetComponent<TextMeshProUGUI>();
        _costText = transform.Find("Content/Buy/Value").GetComponent<Text>();
        _typeText = transform.Find("Content/Info/Detail/TypeInfo/Text").GetComponent<Text>();
        _buyButton = transform.Find("Content/Buy").GetComponent<Button>();
        _buyButton.onClick.AddListener(OnBuyButtonClick);
        transform.Find("Functions/Lock").GetComponent<Button>().onClick.AddListener(OnLockButtonClick);
    }

    public void SetUp(ShopGoodsInfo info)
    {
        _goodsInfo = info;
        if (info == null)
            return;

        string name = string.Empty;
        string desc = string.Empty;
        Sprite icon = null;

        if(info._cfg.ItemType == GoodsItemType.ShipPlug)
        {
            _unitInfoRoot.transform.SafeSetActive(false);
            PropertyModifyRoot.transform.SafeSetActive(true);
            var plugCfg = DataManager.Instance.GetShipPlugItemConfig(info._cfg.TypeID);
            if(plugCfg != null)
            {
                name = LocalizationManager.Instance.GetTextValue(plugCfg.GeneralConfig.Name);
                desc = LocalizationManager.Instance.GetTextValue(plugCfg.GeneralConfig.Desc);
                icon = plugCfg.GeneralConfig.IconSprite;
            }
            _typeText.text = LocalizationManager.Instance.GetTextValue(GameHelper.ShopItemType_ShipPlug_Text);
            SetUpProperty();
        }
        else if (info._cfg.ItemType == GoodsItemType.ShipUnit)
        {
            _unitInfoRoot.transform.SafeSetActive(true);
            PropertyModifyRoot.transform.SafeSetActive(false);
            var unitCfg = DataManager.Instance.GetUnitConfig(info._cfg.TypeID);
            if(unitCfg != null)
            {
                name = LocalizationManager.Instance.GetTextValue(unitCfg.GeneralConfig.Name);
                desc = LocalizationManager.Instance.GetTextValue(unitCfg.GeneralConfig.Desc);
                icon = unitCfg.GeneralConfig.IconSprite;

                if(unitCfg.unitType == UnitType.Weapons)
                {
                    _typeText.text = LocalizationManager.Instance.GetTextValue(GameHelper.ShopItemType_ShipWeapon_Text);
                }
                else if (unitCfg.unitType == UnitType.Buildings)
                {
                    _typeText.text = LocalizationManager.Instance.GetTextValue(GameHelper.ShopItemType_ShipBuilding_Text);
                }
                SetUpUintInfo(unitCfg);
            }
        }
        _nameText.text = name;
        _descText.text = desc;
        _icon.sprite = icon;
      
        RefreshCost();
        SetSold(false);

        var contentRect = transform.Find("Content").SafeGetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }

    /// <summary>
    /// ˢ�»���
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
        }
    }

    private void OnLockButtonClick()
    {

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
        var rect = PropertyModifyRoot.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }

    private void SetUpPropertyCmpt(List<PropertyMidifyConfig> cfgs)
    {
        PropertyModifyRoot.Pool_BackAllChilds(ShopPropertyItem_PrefabPath);
        if (cfgs == null || cfgs.Count <= 0)
            return;

        var index = 0;
        for (int i = 0; i < cfgs.Count; i++)
        {
            PoolManager.Instance.GetObjectSync(ShopPropertyItem_PrefabPath, true, (obj) =>
            {
                var cmpt = obj.GetComponent<ItemPropertyModifyCmpt>();
                cmpt.SetUp(cfgs[index].ModifyKey, cfgs[index].Value);
                index++;
            }, PropertyModifyRoot);
        }
        
    }
}
