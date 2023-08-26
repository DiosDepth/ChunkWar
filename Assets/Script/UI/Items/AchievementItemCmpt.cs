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
    private TextMeshProUGUI _statisticsText;

    private CanvasGroup _progressCanvas;
    private CanvasGroup _finishCanvas;
    private Transform _unfinishMask;
    private Transform _statisticsContent;

    protected override void Awake()
    {
        base.Awake();
        _icon = transform.Find("Content/Icon/Image").SafeGetComponent<Image>();
        _nameText = transform.Find("Content/Info/Name").SafeGetComponent<TextMeshProUGUI>();
        _descText = transform.Find("Content/Info/Desc").SafeGetComponent<TextMeshProUGUI>();
        _finishTimeText = transform.Find("Content/Complete/Finish/Time").SafeGetComponent<TextMeshProUGUI>();
        _statisticsText = transform.Find("Content/Complete/Finish/Statistics/Value").SafeGetComponent<TextMeshProUGUI>();

        _progressCanvas = transform.Find("Content/Complete/Progress").SafeGetComponent<CanvasGroup>();
        _progressText = transform.Find("Content/Complete/Progress").SafeGetComponent<TextMeshProUGUI>();
        _finishCanvas = transform.Find("Content/Complete/Finish").SafeGetComponent<CanvasGroup>();
        _unfinishMask = transform.Find("Content/Icon/UnfinishMask");
        _statisticsContent = transform.Find("Content/Complete/Finish/Statistics");

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
                SetProgressText(info);
            }
        }
    }

    private void SetProgressText(AchievementItemConfig cfg)
    {
        var cons = cfg.Conditions;
        for(int i = 0; i < cons.Count; i++)
        {
            var con = cons[i];
            if (con.DisplayProgress)
            {

                ///Show Progress
                int totalProgress = con.CheckBoolValue ? 1 : con.Value;
                var currentProgress = AchievementManager.Instance.GetStatisticsValue(con.AchievementKey);
                _progressText.text = string.Format("{0} / {1}", currentProgress, totalProgress);

                ///累计进度显示
                _statisticsContent.SafeSetActive(!con.CheckBoolValue);
                if (!con.CheckBoolValue)
                {
                    _statisticsText.text = currentProgress.ToString();
                }
                return;
            }
        }
    }

    protected override void SelectedChanged(bool selected)
    {
        base.SelectedChanged(selected);
    }
}
