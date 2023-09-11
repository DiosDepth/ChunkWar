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
    private TextMeshProUGUI _nextValueText;
    private TextMeshProUGUI _costValueText;

    private Transform _costInfoTrans;
    private Transform _maxTrans;
    private CanvasGroup _nextLevelCanvas;
    private Button _upgradeBtn;

    private PropertyModifyKey propertyKey;
    private CampData _campData;

    protected override void Awake()
    {
        base.Awake();
        _costInfoTrans = transform.Find("Content/UpgradeButton/CostInfo");
        _maxTrans = transform.Find("Content/UpgradeButton/MaxInfo");
        _nextLevelCanvas = transform.Find("Content/Next").SafeGetComponent<CanvasGroup>();
        _icon = transform.Find("Content/Title/Icon").SafeGetComponent<Image>();
        _nameText = transform.Find("Content/Title/Name").SafeGetComponent<TextMeshProUGUI>();
        _currentValueText = transform.Find("Content/Current/Right/Value").SafeGetComponent<TextMeshProUGUI>();
        _nextValueText = transform.Find("Content/Next/Right/Value").SafeGetComponent<TextMeshProUGUI>();
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
        _campData = GameManager.Instance.GetCampDataByID(campID);
        if (_campData == null)
            return;

        propertyKey = (PropertyModifyKey)(ItemUID % 10000);
        var keyCfg = DataManager.Instance.battleCfg.GetPropertyDisplayConfig(propertyKey);
        if (keyCfg == null)
            return;

        _icon.sprite = keyCfg.Icon;
        _nameText.text = LocalizationManager.Instance.GetTextValue(keyCfg.NameText);

        if (_campData.BuffLevelMap.ContainsKey(propertyKey))
        {
            byte level = _campData.BuffLevelMap[propertyKey];
            var modifyValue = _campData.GetValueModify(propertyKey, level);
            _campData.GetNextBuffUpgradeInfo(propertyKey, out int nextLevel, out float nextValue);
            _currentValueText.text = keyCfg.IsPercent ? string.Format("{0} %", modifyValue) : modifyValue.ToString();
            _nextValueText.text = keyCfg.IsPercent ? string.Format("{0} %", nextValue) : nextValue.ToString();
        }
        SetUpgradeButton();
    }

    private void OnUpgradeClick()
    {
        if (_campData == null)
            return;

        if(_campData.UpgradeBuff(propertyKey, out CampBuffUpgradeDialog.BuffUpgradeInfo info))
        {
            UIManager.Instance.ShowUI<CampBuffUpgradeDialog>("CampBuffUpgradeDialog", E_UI_Layer.Top, null, (panel) =>
            {
                panel.Initialization(info);
            });
        }
    }

    protected override void SelectedChanged(bool selected)
    {
        base.SelectedChanged(selected);
    }

    private void SetUpgradeButton()
    {
        bool maxLevel = _campData.IsMaxBuffLevel(propertyKey);
        _costInfoTrans.SafeSetActive(!maxLevel);
        _maxTrans.SafeSetActive(maxLevel);
        if (!maxLevel)
        {
            _costValueText.text = _campData.GetCurrentCostValue(propertyKey).ToString();
        }
        _nextLevelCanvas.ActiveCanvasGroup(!maxLevel);

        bool costVaild = _campData.CheckBuffUpgradeCostEnough(propertyKey);
        _costValueText.color = costVaild ? Color.white : Color.red;

        _upgradeBtn.interactable = costVaild && !maxLevel;
    }
}
