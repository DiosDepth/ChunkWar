using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WreckageSlotItemCmpt : MonoBehaviour, IScrollGirdCmpt
{
    public SelectedDelegate selected;

    public int DataIndex { get; set; }
    public uint ItemUID { get; set; }

    private SelectableItemBase _item;
    private Image _icon;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _descText;
    private Text _costText;
    private Transform _unitInfoRoot;
    private Text _typeText;

    private WreckageItemInfo _info;
    private const string UnitInfo_PropertyItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/UnitPropertyItem";

    public void Awake()
    {
        _icon = transform.Find("Content/Info/Icon").SafeGetComponent<Image>();
        _nameText = transform.Find("Content/Info/Detail/Name").GetComponent<TextMeshProUGUI>();
        _descText = transform.Find("Content/Desc").GetComponent<TextMeshProUGUI>();
        _typeText = transform.Find("Content/Info/Detail/TypeInfo/Text").GetComponent<Text>();
        _unitInfoRoot = transform.Find("Content/UnitInfo");
        transform.Find("BG").SafeGetComponent<Button>().onClick.AddListener(OnBtnClick);
    }

    public void SetDataGrid(int dataIndex, SelectableItemBase item, SelectedDelegate selected)
    {
        this.selected = selected;
        transform.SafeGetComponent<CanvasGroup>().ActiveCanvasGroup(item != null);
        if (item != null)
        {
            ItemUID = (uint)item.content;

            _info = RogueManager.Instance.GetCurrentWreckageByUID(ItemUID);
            SetUp(_info);
        }

        if (_item != null)
        {
            _item.selectedChanged -= SelectedChanged;
        }
        DataIndex = dataIndex;
        _item = item;
        if (item != null)
        {
            _item.selectedChanged -= SelectedChanged;
            _item.selectedChanged += SelectedChanged;
            SelectedChanged(_item.Selected);
        }
    }

    private void SetUp(WreckageItemInfo info)
    {
        if (info == null)
            return;

        var cfg = DataManager.Instance.GetUnitConfig(info.UnitID);
        if (cfg == null)
            return;

        _nameText.text = LocalizationManager.Instance.GetTextValue(cfg.GeneralConfig.Name);
        _descText.text = LocalizationManager.Instance.GetTextValue(cfg.GeneralConfig.Desc);
        _icon.sprite = cfg.GeneralConfig.IconSprite;
        _nameText.color = GameHelper.GetRarityColor(cfg.GeneralConfig.Rarity);
        if (cfg.unitType == UnitType.Weapons)
        {
            _typeText.text = LocalizationManager.Instance.GetTextValue(GameHelper.ShopItemType_ShipWeapon_Text);
        }
        else if (cfg.unitType == UnitType.Buildings)
        {
            _typeText.text = LocalizationManager.Instance.GetTextValue(GameHelper.ShopItemType_ShipBuilding_Text);
        }
        SetUpUintInfo(cfg);

        var contentRect = transform.Find("Content").SafeGetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
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

        var allCmpt = _unitInfoRoot.GetComponentsInChildren<UnitPropertyItemCmpt>().ToList();

        var cfg = DataManager.Instance.GetUnitConfig(_info.UnitID);
        if (cfg.unitType == UnitType.Weapons)
        {
            WeaponConfig weaponCfg = cfg as WeaponConfig;
            if (weaponCfg == null)
                return;

            var cmpt = FindCmptByType(allCmpt, type);
            if (cmpt != null)
            {
                var content = GameHelper.GetWeaponPropertyDescContent(type, weaponCfg);
                cmpt.RefreshContent(content);
            }
        }
    }

    private void SetUpUintInfo(BaseUnitConfig cfg)
    {
        _unitInfoRoot.Pool_BackAllChilds(UnitInfo_PropertyItem_PrefabPath);
        if (cfg.unitType == UnitType.Weapons)
        {
            WeaponConfig weaponCfg = cfg as WeaponConfig;
            if (weaponCfg == null)
                return;

            foreach (UI_WeaponUnitPropertyType type in System.Enum.GetValues(typeof(UI_WeaponUnitPropertyType)))
            {
                if (type == UI_WeaponUnitPropertyType.ShieldTransfixion)
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

    private void SelectedChanged(bool select)
    {
        transform.Find("Selected").SafeSetActive(select);
    }

    private void OnBtnClick()
    {
        _item.Selected = true;
        selected?.Invoke(this);
        var cfg = DataManager.Instance.GetUnitConfig(_info.UnitID);
        var shipBuilder = ShipBuilder.instance;
        if (shipBuilder == null || cfg == null)
            return;

        var item = new InventoryItem(cfg);
        item.RefUID = _info.UID;
        shipBuilder.currentInventoryItem = item;
        shipBuilder.SetBrushSprite();
    }
}
