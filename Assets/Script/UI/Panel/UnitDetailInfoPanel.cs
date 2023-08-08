using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitDetailInfoPanel : MonoBehaviour
{
    private Image _icon;
    private Text _nameText;
    private Text _damageType;
    private Transform _unitInfoRoot;
    private RectTransform _root;
    private TextMeshProUGUI _descText;

    private CanvasGroup _canvas;

    private int _mode = 1;

    private const string UnitInfo_PropertyItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/UnitPropertyItem_Large";
    private const string UnitInfo_PropertyItem_PrefabPath2 = "Prefab/GUIPrefab/CmptItems/UnitPropertyItem_Large2";

    public void Awake()
    {
        _canvas = transform.SafeGetComponent<CanvasGroup>();
        _icon = transform.Find("IconContent/Icon").SafeGetComponent<Image>();
        _nameText = transform.Find("IconContent/Info/Name").SafeGetComponent<Text>();
        _damageType = transform.Find("IconContent/Info/DamageType").SafeGetComponent<Text>();
        _unitInfoRoot = transform.Find("InfoContent");
        _root = transform.SafeGetComponent<RectTransform>();
        _descText = transform.Find("Desc").SafeGetComponent<TextMeshProUGUI>();
    }

    public void Hide()
    {
        _canvas.ActiveCanvasGroup(false);
    }

    public void SetUpInfo(BaseUnitConfig cfg, int mode = 1)
    {
        _mode = mode;
        _icon.sprite = cfg.GeneralConfig.IconSprite;
        _nameText.text = LocalizationManager.Instance.GetTextValue(cfg.GeneralConfig.Name);
        _descText.text = LocalizationManager.Instance.GetTextValue(cfg.GeneralConfig.Desc);
        if (cfg.unitType == UnitType.MainWeapons || cfg.unitType == UnitType.Weapons)
        {
            _damageType.transform.SafeSetActive(true);
            var weaponCfg = cfg as WeaponConfig;
            _damageType.text =  GameHelper.GetWeaponDamageTypeText(weaponCfg.DamageType);
        }
        else
        {
            _damageType.transform.SafeSetActive(false);
        }

        SetUpUintInfo(cfg);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_root);
    }

    private void SetUpUintInfo(BaseUnitConfig cfg)
    {
        var path = string.Empty;
        if(_mode == 1)
        {
            path = UnitInfo_PropertyItem_PrefabPath;
        }
        else
        {
            path = UnitInfo_PropertyItem_PrefabPath2;
        }

        _unitInfoRoot.Pool_BackAllChilds(path);
        if (cfg.unitType == UnitType.Weapons || cfg.unitType == UnitType.MainWeapons)
        {
            WeaponConfig weaponCfg = cfg as WeaponConfig;
            if (weaponCfg == null)
                return;

            foreach (UI_WeaponUnitPropertyType type in System.Enum.GetValues(typeof(UI_WeaponUnitPropertyType)))
            {
                PoolManager.Instance.GetObjectSync(path, true, (obj) =>
                {
                    string content = GameHelper.GetWeaponPropertyDescContent(type, weaponCfg);
                    var cmpt = obj.GetComponent<UnitPropertyItemCmpt>();
                    cmpt.SetUpWeapon(type, content);
                }, _unitInfoRoot);
            }
        }
    }
}
