using Cysharp.Threading.Tasks;
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
    private Image _rarityBG;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _descText;
    private TextMeshProUGUI _sellText;
    private TextMeshProUGUI _energyCostText;
    private TextMeshProUGUI _loadCostText;
    private Transform _unitInfoRoot;
    private RectTransform _tagRoot;
    private CanvasGroup _contentCanvas;
    private CanvasGroup _wasteCanvas;

    private TextMeshProUGUI _wasteValueText;
    private TextMeshProUGUI _wasteLoadValueText;

    private RectTransform _detailContentRect;
    private ScrollRect _detailScroll;

    private WreckageItemInfo _info;
    private float ScrollHeight;
    private const string UnitInfo_PropertyItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/UnitPropertyItem";
    private const string ItemTag_PrefabPath = "Prefab/GUIPrefab/PoolUI/ItemTagCmpt";

    public void Awake()
    {
        _tagRoot = transform.Find("Content/Info/Detail/TypeInfo").SafeGetComponent<RectTransform>();
        _detailScroll = transform.Find("Content/DetailInfo").SafeGetComponent<ScrollRect>();
        ScrollHeight = _detailScroll.transform.SafeGetComponent<RectTransform>().rect.height;
        _detailContentRect = transform.Find("Content/DetailInfo/Viewport/Content").SafeGetComponent<RectTransform>();

        _energyCostText = transform.Find("Content/Content/Energy/EnergyValue").SafeGetComponent<TextMeshProUGUI>();
        _loadCostText = transform.Find("Content/Content/Load/LoadValue").SafeGetComponent<TextMeshProUGUI>();

        _wasteCanvas = transform.Find("WasteContent").SafeGetComponent<CanvasGroup>();
        _contentCanvas = transform.Find("Content").SafeGetComponent<CanvasGroup>();
        _icon = transform.Find("Content/Info/Icon/Image").SafeGetComponent<Image>();
        _rarityBG = transform.Find("BG").SafeGetComponent<Image>();
        _nameText = transform.Find("Content/Info/Detail/Name").GetComponent<TextMeshProUGUI>();

        _descText = _detailContentRect.Find("Desc").GetComponent<TextMeshProUGUI>();
        _sellText = transform.Find("Content/Sell/Value").SafeGetComponent<TextMeshProUGUI>();
        _unitInfoRoot = _detailContentRect.Find("UnitInfo");

        _wasteValueText = _wasteCanvas.transform.Find("Count/Value").SafeGetComponent<TextMeshProUGUI>();
        _wasteLoadValueText = _wasteCanvas.transform.Find("Load/Value").SafeGetComponent<TextMeshProUGUI>();

        transform.Find("BG").SafeGetComponent<Button>().onClick.AddListener(OnBtnClick);
        transform.Find("Content/Sell").SafeGetComponent<Button>().onClick.AddListener(OnSellBtnClick);
        transform.Find("WasteContent/WasteSell").SafeGetComponent<Button>().onClick.AddListener(OnWasteSellClick);
    }

    public void SetDataGrid(int dataIndex, SelectableItemBase item, SelectedDelegate selected)
    {
        this.selected = selected;
        if(item == null)
        {
            transform.SafeGetComponent<CanvasGroup>().ActiveCanvasGroup(false);
            return;
        }

        transform.SafeGetComponent<CanvasGroup>().ActiveCanvasGroup(true);

        if (item != null)
        {
            ItemUID = (uint)item.content;
            if(ItemUID == 0)
            {
                _contentCanvas.ActiveCanvasGroup(false);
                _wasteCanvas.ActiveCanvasGroup(true);
                SetUpWaste();
            }
            else
            {
                _contentCanvas.ActiveCanvasGroup(true);
                _wasteCanvas.ActiveCanvasGroup(false);
                _info = RogueManager.Instance.GetCurrentWreckageByUID(ItemUID);
                SetUp(_info);
            }
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

    /// <summary>
    /// 废品信息
    /// </summary>
    private void SetUpWaste()
    {
        _rarityBG.sprite = GameHelper.GetRarityBG_Big(GoodsItemRarity.Tier1);
        _wasteValueText.text = RogueManager.Instance.GetDropWasteCount.ToString();
        _wasteLoadValueText.text = string.Format("{0:F1}", RogueManager.Instance.GetDropWasteLoad);
    }

    private async void SetUp(WreckageItemInfo info)
    {
        if (info == null)
            return;

        _nameText.text = info.Name;
        _descText.text = info.Desc;
        _icon.sprite = info.UnitConfig.GeneralConfig.IconSprite;
        _nameText.color = info.RarityColor;
        _rarityBG.sprite = GameHelper.GetRarityBG_Big(info.Rarity);
        _sellText.text = info.SellPrice.ToString();

        _loadCostText.text = string.Format("{0:F1}", info.LoadCost);
        _energyCostText.text = string.Format("{0:F1}", info.GetEnergyCost());

        SetUpUintInfo(info.UnitConfig);
        SetUpTag(info.UnitConfig);
        await UniTask.WaitForFixedUpdate();
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

    private void SetUpTag(BaseUnitConfig cfg)
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

        LayoutRebuilder.ForceRebuildLayoutImmediate(_tagRoot);
    }

    private void SelectedChanged(bool select)
    {
        transform.Find("Selected").SafeSetActive(select);
    }

    private void OnBtnClick()
    {
        _item.Selected = true;
        selected?.Invoke(this);

        if (ItemUID == 0)
        {
            return;
        }
            
        var cfg = DataManager.Instance.GetUnitConfig(_info.UnitID);
        var shipBuilder = ShipBuilder.instance;
        if (shipBuilder == null || cfg == null)
            return;

        var item = new InventoryItem(cfg);
        item.RefUID = _info.UID;
        shipBuilder.CurrentInventoryItem = item;
        shipBuilder.SetBrushSprite();
    }

    private void OnSellBtnClick()
    {
        if (_info == null)
            return;

        _info.Sell();
    }

    private void OnWasteSellClick()
    {
        if (ItemUID != 0)
            return;

        ///ShowSell Dialog
        UIManager.Instance.ShowUI<WasteSellDialog>("WasteSellDialog", E_UI_Layer.Top, this, (panel)=> 
        {
            panel.Initialization();
        });
    }
}
