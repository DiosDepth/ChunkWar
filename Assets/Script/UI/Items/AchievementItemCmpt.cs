using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementItemCmpt : EnhancedScrollerCellView
{
    private Image _icon;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _descText;
    private TextMeshProUGUI _progressText;
    private TextMeshProUGUI _finishTimeText;

    private CanvasGroup _progressCanvas;
    private CanvasGroup _finishCanvas;
    private Transform _unfinishMask;

    protected override void Awake()
    {
        base.Awake();
        _icon = transform.Find("Content/Icon/Image").SafeGetComponent<Image>();
        _nameText = transform.Find("Content/Info/Name").SafeGetComponent<TextMeshProUGUI>();
        _descText = transform.Find("Content/Info/Desc").SafeGetComponent<TextMeshProUGUI>();
        _finishTimeText = transform.Find("Content/Complete/Finish/Time").SafeGetComponent<TextMeshProUGUI>();

        _progressCanvas = transform.Find("Content/Complete/Progress").SafeGetComponent<CanvasGroup>();
        _progressText = transform.Find("Content/Complete/Progress").SafeGetComponent<TextMeshProUGUI>();
        _finishCanvas = transform.Find("Content/Complete/Finish").SafeGetComponent<CanvasGroup>();
        _unfinishMask = transform.Find("Content/Icon/UnfinishMask");

    }

    public override void SetData(int index, SelectableItemBase item)
    {
        base.SetData(index, item);
    }

    public override void RefreshCellView()
    {
        var info = DataManager.Instance.GetAchievementItemConfig((int)ItemUID);
        if (info == null)
            return;

        _icon.sprite = info.Icon;
        _nameText.text = LocalizationManager.Instance.GetTextValue(info.AchievementName);
        _descText.text = LocalizationManager.Instance.GetTextValue(info.AchievementDesc);
        var saveInfo = SaveLoadManager.Instance.GetAchievementSaveDataByID((int)ItemUID);
        if(saveInfo != null)
        {
            _progressCanvas.ActiveCanvasGroup(!saveInfo.Unlock);
            _finishCanvas.ActiveCanvasGroup(saveInfo.Unlock);
            _unfinishMask.SafeSetActive(!saveInfo.Unlock);

            if (saveInfo.Unlock)
            {
                _finishTimeText.text = saveInfo.FinishTime;
            }
            else
            {

            }
        }
    }

    protected override void SelectedChanged(bool selected)
    {
        base.SelectedChanged(selected);
    }
}
