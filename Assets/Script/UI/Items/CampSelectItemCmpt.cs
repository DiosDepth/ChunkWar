using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CampSelectItemCmpt : EnhancedScrollerCellView
{
    private Image _icon;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _descText;
    private TextMeshProUGUI _levelText;
    private TextMeshProUGUI _scoreText;

    private Transform _scoreContent;
    private Transform _lockContent;

    protected override void Awake()
    {
        base.Awake();
        _icon = transform.Find("Content/Icon").SafeGetComponent<Image>();
        _nameText = transform.Find("Content/Name").SafeGetComponent<TextMeshProUGUI>();
        _descText = transform.Find("Content/Desc").SafeGetComponent<TextMeshProUGUI>();
        _levelText = transform.Find("Content/Level").SafeGetComponent<TextMeshProUGUI>();
        _scoreContent = transform.Find("Content/ScoreContent");
        _lockContent = transform.Find("Lock");
        _scoreText = transform.Find("Content/ScoreContent/Value").SafeGetComponent<TextMeshProUGUI>();

        transform.SafeGetComponent<Button>().onClick.AddListener(OnCampClick);
    }

    public override void SetData(int index, SelectableItemBase item)
    {
        base.SetData(index, item);
    }

    public override void RefreshCellView()
    {
        var info = GameManager.Instance.GetCampDataByID((int)ItemUID);
        if (info == null)
            return;

        _nameText.text = info.CampName;
        _descText.text = info.CampDesc;
        _levelText.text = string.Format("Lv. {0}", info.GetCampLevel);
        SetLockState(info.Unlock);
    }

    private void OnCampClick()
    {

    }

    protected override void SelectedChanged(bool selected)
    {
        base.SelectedChanged(selected);
    }

    private void SetLockState(bool unlock)
    {
        _levelText.transform.SafeSetActive(unlock);
        _scoreContent.SafeSetActive(unlock);
        _lockContent.SafeSetActive(!unlock);
    }
}
