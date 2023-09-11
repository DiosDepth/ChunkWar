using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CampBuffUpgradeDialog : GUIBasePanel
{
    public struct BuffUpgradeInfo
    {
        public PropertyModifyKey PropertyKey;
        public string CampName;
        public float currentValue;
        public float nextValue;
    }

    private static string CampBuffUpgrade_DescText = "CampBuffUpgrade_DescText";

    protected override void Awake()
    {
        base.Awake();
        GetGUIComponent<Button>("GeneralBackBtn").onClick.AddListener(OnCloseDialogClick);
        transform.Find("BG").SafeGetComponent<Button>().onClick.AddListener(OnCloseDialogClick);
    }

    public override void Initialization(params object[] param)
    {
        base.Initialization(param);
        BuffUpgradeInfo info = (BuffUpgradeInfo)param[0];
        SetUp(info);
    }

    private void SetUp(BuffUpgradeInfo info)
    {
        var infoTrans = transform.Find("Content/Info");

        string descText = LocalizationManager.Instance.GetTextValue(CampBuffUpgrade_DescText);
        descText = LocalizationManager.Instance.ReplaceTextBySpecialValue(descText, info.CampName, info.CampName);
        infoTrans.Find("Desc").SafeGetComponent<TextMeshProUGUI>().text = descText;

        var propertyConfig = DataManager.Instance.battleCfg.GetPropertyDisplayConfig(info.PropertyKey);
        if (propertyConfig == null)
            return;

        infoTrans.Find("UpgradeInfo/CurrentBuff/Value").SafeGetComponent<TextMeshProUGUI>().text = propertyConfig.IsPercent ? string.Format("{0}%", info.currentValue) : info.currentValue.ToString();
        infoTrans.Find("UpgradeInfo/NextBuff/Value").SafeGetComponent<TextMeshProUGUI>().text = propertyConfig.IsPercent ? string.Format("{0}%", info.nextValue) : info.nextValue.ToString();

        infoTrans.Find("BuffInfo/Icon").SafeGetComponent<Image>().sprite = propertyConfig.Icon;
        infoTrans.Find("BuffInfo/BuffName").SafeGetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetTextValue(propertyConfig.NameText);
    }

    private void OnCloseDialogClick()
    {
        UIManager.Instance.HiddenUI("CampBuffUpgradeDialog");
    }
}
