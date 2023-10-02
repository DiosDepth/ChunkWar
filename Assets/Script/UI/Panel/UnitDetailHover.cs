using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitDetailHover : GUIBasePanel, IPoolable
{
    private CanvasGroup _mainCanvas;
    private Image _icon;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _descText;

    private Transform _unitInfoRoot;
    private RectTransform _contentRect;

    private bool _enable = false;

    private const string UnitInfo_PropertyItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/UnitPropertyItem";

    protected override void Awake()
    {
        _mainCanvas = transform.SafeGetComponent<CanvasGroup>();
        _contentRect = transform.Find("Content").SafeGetComponent<RectTransform>();
        _icon = _contentRect.Find("Info/Icon/Image").SafeGetComponent<Image>();
        _nameText = _contentRect.Find("Info/Detail/Name").GetComponent<TextMeshProUGUI>();
        _descText = _contentRect.Find("Desc").SafeGetComponent<TextMeshProUGUI>();
        _unitInfoRoot = _contentRect.Find("UnitInfo");
        base.Awake();
    }


    public override void Initialization(params object[] param)
    {
        base.Initialization(param);
        int UnitID = (int)param[0];
        SetUp(UnitID);
    }

    private void SetUp(int unitID)
    {
        var unitCfg = DataManager.Instance.GetUnitConfig(unitID);
        if (unitCfg == null)
            return;

        _nameText.text = LocalizationManager.Instance.GetTextValue(unitCfg.GeneralConfig.Name);
        _nameText.color = GameHelper.GetRarityColor(unitCfg.GeneralConfig.Rarity);
        _icon.sprite = unitCfg.GeneralConfig.IconSprite;
        if (!string.IsNullOrEmpty(unitCfg.GeneralConfig.Desc))
        {
            _descText.text = LocalizationManager.Instance.GetTextValue(unitCfg.GeneralConfig.Desc);
        }

        SetUpUintInfo(unitCfg);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRect);
        _mainCanvas.ActiveCanvasGroup(true);
        _enable = true;
    }

    public void Update()
    {
        if (_enable)
        {
            var mousePos = UIManager.Instance.GetUIPosByMousePos();
            transform.localPosition = mousePos;
        }
    }

    private void SetUpUintInfo(BaseUnitConfig cfg)
    {
        _unitInfoRoot.Pool_BackAllChilds(UnitInfo_PropertyItem_PrefabPath);
        if (cfg.unitType == UnitType.Weapons || cfg.unitType == UnitType.MainWeapons)
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

    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(transform.name, gameObject);
    }

    public void PoolableReset()
    {
        _enable = false;
        _mainCanvas.ActiveCanvasGroup(false);
    }

    public void PoolableSetActive(bool isactive = true)
    {
    }
}
