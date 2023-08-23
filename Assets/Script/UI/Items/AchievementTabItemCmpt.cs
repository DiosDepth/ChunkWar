using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementTabItemCmpt : MonoBehaviour, IPoolable
{
    public AchievementGroupType GroupType;

    private Image _icon;
    private TextMeshProUGUI _nameText;
    private Transform _selectedObj;

    public void Awake()
    {
        _icon = transform.Find("Content/Icon").SafeGetComponent<Image>();
        _nameText = transform.Find("Content/Info/Name").SafeGetComponent<TextMeshProUGUI>();
        _selectedObj = transform.Find("Selected");
        transform.Find("BG").SafeGetComponent<Button>().onClick.AddListener(OnBtnClick);
        SetSelected(false);
    }

    public void SetUp(AchievementGroupType type)
    {
        GroupType = type;
        var cfg = DataManager.Instance.gameMiscCfg.GetAchievementGroupConfig(type);
        if(cfg != null)
        {
            _nameText.text = LocalizationManager.Instance.GetTextValue(cfg.GroupName);
            _icon.sprite = cfg.GroupIcon;
        }
    }

    private void OnBtnClick()
    {
        AchievementEvent.Trigger(AchievementEventType.UI_GroupSelected, GroupType);
        SetSelected(true);
    }

    public void SetSelected(bool selected)
    {
        _selectedObj.SafeSetActive(selected);
    }

    public void PoolableDestroy()
    {

    }

    public void PoolableReset()
    {

    }

    public void PoolableSetActive(bool isactive = true)
    {

    }
}
