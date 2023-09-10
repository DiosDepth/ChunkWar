using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CampBuffItemCmpt : EnhancedScrollerCellView
{
    private Image _icon;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _currentValueText;
    private TextMeshProUGUI _costValueText;

    private Transform _costInfoTrans;
    private Transform _maxTrans;
    private Button _upgradeBtn;

    protected override void Awake()
    {
        base.Awake();
        _costInfoTrans = transform.Find("Content/UpgradeButton/CostInfo");
        _maxTrans = transform.Find("Content/UpgradeButton/MaxInfo");
        _icon = transform.Find("Content/Title/Icon").SafeGetComponent<Image>();
        _nameText = transform.Find("Content/Title/Name").SafeGetComponent<TextMeshProUGUI>();
        _currentValueText = transform.Find("Content/Current/Right/Value").SafeGetComponent<TextMeshProUGUI>();
        _costValueText = transform.Find("Content/UpgradeButton/CostInfo/Value").SafeGetComponent<TextMeshProUGUI>();
        _upgradeBtn = transform.Find("Content/UpgradeButton").SafeGetComponent<Button>();
        _upgradeBtn.onClick.AddListener(OnUpgradeClick);
    }

    public override void SetData(int index, SelectableItemBase item)
    {
        base.SetData(index, item);
    }

    public override void RefreshCellView()
    {
        int campID = (int)(ItemUID / 10000);
        var CampData = GameManager.Instance.GetCampDataByID(campID);
        if (CampData == null)
            return;

        PropertyModifyKey propertyKey = (PropertyModifyKey)(ItemUID % 10000);
        var keyCfg = DataManager.Instance.battleCfg.GetPropertyDisplayConfig(propertyKey);
        if (keyCfg == null)
            return;

        _icon.sprite = keyCfg.Icon;
        _nameText.text = LocalizationManager.Instance.GetTextValue(keyCfg.NameText);

        if (CampData.BuffLevelMap.ContainsKey(propertyKey))
        {
            byte level = CampData.BuffLevelMap[propertyKey];
            var modifyValue = CampData.GetValueModify(propertyKey, level);
            _currentValueText.text = keyCfg.IsPercent ? string.Format("{0} %", modifyValue) : modifyValue.ToString();
        }
        SetUpMaxLevel(CampData, propertyKey);
    }

    private void OnUpgradeClick()
    {

    }

    protected override void SelectedChanged(bool selected)
    {
        base.SelectedChanged(selected);
    }

    private void SetUpMaxLevel(CampData data, PropertyModifyKey buffKey)
    {
        bool maxLevel = data.IsMaxBuffLevel(buffKey);
        _costInfoTrans.SafeSetActive(!maxLevel);
        _maxTrans.SafeSetActive(maxLevel);
        _upgradeBtn.interactable = !maxLevel;

        if (!maxLevel)
        {
            _costValueText.text = data.GetCurrentCostValue(buffKey).ToString();
        }
    }
}
