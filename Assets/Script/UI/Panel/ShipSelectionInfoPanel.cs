using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipSelectionInfoPanel : MonoBehaviour
{
    private Text _nameText;
    private Image _classIcon;
    private TextMeshProUGUI _className;
    private Transform _propertyContent;
    private TextMeshProUGUI _descText;
    private RectTransform _infoRect;
    private UnitDetailInfoPanel _weaponInfoPanel;

    private const string ShipProperty_ItemPrefabPath = "Prefab/GUIPrefab/CmptItems/ShipItemProperty";

    public void Awake()
    {
        _weaponInfoPanel = transform.Find("WeaponInfo").SafeGetComponent<UnitDetailInfoPanel>();
        _propertyContent = transform.Find("ShipInfo/PropertyContent");
        _nameText = transform.Find("ShipInfo/ShipClassInfo/Name").SafeGetComponent<Text>();
        _classIcon = transform.Find("ShipInfo/ShipClassInfo/ClassIcon").SafeGetComponent<Image>();
        _className = transform.Find("ShipInfo/ShipClassInfo/ClassName").SafeGetComponent<TextMeshProUGUI>();
        _descText = transform.Find("ShipInfo/Desc").SafeGetComponent<TextMeshProUGUI>();
        _infoRect = transform.Find("ShipInfo").SafeGetComponent<RectTransform>();
    }


    public void SetUpShipInfo(PlayerShipConfig cfg)
    {
        _nameText.text = LocalizationManager.Instance.GetTextValue(cfg.GeneralConfig.Name);
        _descText.text = LocalizationManager.Instance.GetTextValue(cfg.GeneralConfig.Desc);
        _propertyContent.Pool_BackAllChilds(ShipProperty_ItemPrefabPath);

        var classCfg = DataManager.Instance.gameMiscCfg.GetShipClassConfig(cfg.ShipClass);
        if(classCfg != null)
        {
            _classIcon.sprite = classCfg.ClassIcon;
            _className.text = LocalizationManager.Instance.GetTextValue(classCfg.ClassName);
        }

        var plugCfg = DataManager.Instance.GetShipPlugItemConfig(cfg.CorePlugID);
        if(plugCfg != null)
        {
            foreach(var property in plugCfg.PropertyModify)
            {
                if (property.Value == 0)
                    continue;

                PoolManager.Instance.GetObjectSync(ShipProperty_ItemPrefabPath, true, (obj) =>
                {
                    var cmpt = obj.GetComponent<ItemPropertyModifyCmpt>();
                    cmpt.SetUp(property.ModifyKey, property.Value, false);
                }, _propertyContent);
            }

            foreach (var property in plugCfg.PropertyPercentModify)
            {
                if (property.Value == 0)
                    continue;

                PoolManager.Instance.GetObjectSync(ShipProperty_ItemPrefabPath, true, (obj) =>
                {
                    var cmpt = obj.GetComponent<ItemPropertyModifyCmpt>();
                    cmpt.SetUp(property.ModifyKey, property.Value, true);
                }, _propertyContent);
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(_infoRect);

        var weaponCfg = DataManager.Instance.GetUnitConfig(cfg.MainWeaponID);
        if (weaponCfg != null)
        {
            _weaponInfoPanel.SetUpInfo(weaponCfg);
        }
    }
}
