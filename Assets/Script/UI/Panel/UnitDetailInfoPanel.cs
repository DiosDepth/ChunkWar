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
    private RectTransform _parentTrans;

    private int _mode = 1;
    private int UnitID = -1;

    private bool _isMoving = false;
    private bool _isShowing = true;

    private const string UnitInfo_PropertyItem_PrefabPath = "Prefab/GUIPrefab/CmptItems/UnitPropertyItem_Large";
    private const string UnitInfo_PropertyItem_PrefabPath2 = "Prefab/GUIPrefab/CmptItems/UnitPropertyItem_Large2";

    public void Awake()
    {
        _icon = transform.Find("IconContent/Icon").SafeGetComponent<Image>();
        _nameText = transform.Find("IconContent/Info/Name").SafeGetComponent<Text>();
        _damageType = transform.Find("IconContent/Info/DamageType").SafeGetComponent<Text>();
        _unitInfoRoot = transform.Find("InfoContent");
        _root = transform.SafeGetComponent<RectTransform>();
        _descText = transform.Find("Desc").SafeGetComponent<TextMeshProUGUI>();
        _isShowing = gameObject.activeSelf;
        _parentTrans = transform.parent.SafeGetComponent<RectTransform>();
    }

    public void Hide()
    {
        if (_isShowing && _isMoving)
            return;

        ///如果显示，则关闭
        if (_isShowing)
        {
            _isMoving = true;
            LeanTween.move(_parentTrans, new Vector3 (700f,0,0), 0.1f).setOnComplete(() =>
            {
                _parentTrans.transform.SafeSetActive(false);
                _isMoving = false;
                _isShowing = false;
            });
        }
    }

    public void Show()
    {
        if (!_isShowing && _isMoving)
            return;

        if (!_isShowing)
        {
            _isMoving = true;
            _parentTrans.transform.SafeSetActive(true);
            LeanTween.move(_parentTrans, new Vector3(0, 0, 0),0.1f).setOnComplete(() =>
            {
                _isMoving = false;
                _isShowing = true;
            });
        }
    }

    public void SetUpInfo(BaseUnitConfig cfg, int mode = 1)
    {
        _mode = mode;
        if (cfg == null)
            return;

        UnitID = cfg.ID;
        _icon.sprite = cfg.GeneralConfig.IconSprite;
        _nameText.text = LocalizationManager.Instance.GetTextValue(cfg.GeneralConfig.Name);
        _descText.text = LocalizationManager.Instance.GetTextValue(cfg.GeneralConfig.Desc);
        _nameText.color = GameHelper.GetRarityColor(cfg.GeneralConfig.Rarity);
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
