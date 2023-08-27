using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementUI : GUIBasePanel, IPoolable
{
    private Image _icon;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _descText;

    public bool IsFinish = false;

    protected override void Awake()
    {
        base.Awake();
        _icon = transform.Find("Content/Icon").SafeGetComponent<Image>();
        _nameText = transform.Find("Content/Info/Name").SafeGetComponent<TextMeshProUGUI>();
        _descText = transform.Find("Content/Info/Desc").SafeGetComponent<TextMeshProUGUI>();
    }

    public override void Initialization()
    {
        base.Initialization();
        IsFinish = false;

    }

    public void SetUp(AchievementItemConfig cfg)
    {
        _icon.sprite = cfg.Icon;
        _nameText.text = LocalizationManager.Instance.GetTextValue(cfg.AchievementName);
        _descText.text = LocalizationManager.Instance.GetTextValue(cfg.AchievementDesc);
        transform.SafeGetComponent<Animator>().Play("AchievementShow");
    }

    public override void Hidden()
    {
        base.Hidden();
        IsFinish = true;
        PoolableDestroy();
    }

    public void PoolableDestroy()
    {
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableReset()
    {

    }

    public void PoolableSetActive(bool isactive = true)
    {

    }
}
