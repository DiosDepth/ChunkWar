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
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _descText;
    private TextMeshProUGUI _sellText;
    private Transform _unitInfoRoot;
    private Text _typeText;
    private CanvasGroup _contentCanvas;
    private CanvasGroup _wasteCanvas;

    private TextMeshProUGUI _wasteValueText;
    private TextMeshProUGUI _wasteLoadValueText;

    private RectTransform _mainContentRect;

    private WreckageItemInfo _info;
    private const string UnitInfo_PropertyItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/UnitPropertyItem";

    public void Awake()
    {
        _mainContentRect = transform.Find("Content").SafeGetComponent<RectTransform>();

        _wasteCanvas = transform.Find("WasteContent").SafeGetComponent<CanvasGroup>();
        _contentCanvas = transform.Find("Content").SafeGetComponent<CanvasGroup>();
        _icon = transform.Find("Content/Info/Icon").SafeGetComponent<Image>();
        _nameText = transform.Find("Content/Info/Detail/Name").GetComponent<TextMeshProUGUI>();
        _descText = transform.Find("Content/Desc").GetComponent<TextMeshProUGUI>();
        _typeText = transform.Find("Content/Info/Detail/TypeInfo/Text").GetComponent<Text>();
        _sellText = transform.Find("Content/Sell/Value").SafeGetComponent<TextMeshProUGUI>();
        _unitInfoRoot = transform.Find("Content/UnitInfo");

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
    /// ��Ʒ��Ϣ
    /// </summary>
    private void SetUpWaste()
    {
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
        _typeText.text = info.TypeName;
        _sellText.text = info.SellPrice.ToString();

        SetUpUintInfo(info.UnitConfig);
        await UniTask.WaitForFixedUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_mainContentRect);
    }

    /// <summary>
    /// ˢ��������Ϣ��ָ������
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
        shipBuilder.currentInventoryItem = item;
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
