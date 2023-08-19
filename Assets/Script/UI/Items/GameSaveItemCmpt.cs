using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameSaveItemCmpt : EnhancedScrollerCellView
{
    private TextMeshProUGUI _indexText;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _gameTime;
    private TextMeshProUGUI _saveTime;
    private TextMeshProUGUI _modeText;
    private Image _indexBG;

    private static Color Color_Default = new Color(0, 0.2f, 0.2f);
    private static Color Color_Select = new Color(0.61f, 0.23f, 0f);
    protected override void Awake()
    {
        _indexBG = transform.Find("Content/Index/BG").SafeGetComponent<Image>();
        _indexText = transform.Find("Content/Index/Text").SafeGetComponent<TextMeshProUGUI>();
        _nameText = transform.Find("Content/Info/Name").SafeGetComponent<TextMeshProUGUI>();
        _gameTime = transform.Find("Content/Info/GameTime/Time").SafeGetComponent<TextMeshProUGUI>();
        _saveTime = transform.Find("Content/Info/SaveTime/SaveTime").SafeGetComponent<TextMeshProUGUI>();
        _modeText = transform.Find("Content/Info/GameMode/Name").SafeGetComponent<TextMeshProUGUI>();
        transform.Find("BG").SafeGetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    public override void SetData(int index, SelectableItemBase item)
    {
        base.SetData(index, item);
    }

    public override void RefreshCellView()
    {
        var saveInfo = SaveLoadManager.Instance.GetSaveDataByIndex((int)ItemUID);
        if (saveInfo == null)
            return;

        _indexText.text = (DataIndex + 1).ToString();
        _nameText.text = saveInfo.SaveName;
        _saveTime.text = saveInfo.date;
        _gameTime.text = GameHelper.GetTimeStringBySeconds(saveInfo.GameTime);

        var hardLevel = DataManager.Instance.battleCfg.GetHardLevelConfig(saveInfo.ModeID);
        if(hardLevel != null)
        {
            _modeText.text = LocalizationManager.Instance.GetTextValue(hardLevel.Name);
        }
        else
        {
            _modeText.text = string.Empty;
        }
    }

    protected override void SelectedChanged(bool selected)
    {
        base.SelectedChanged(selected);
        _indexBG.color = selected ? Color_Select : Color_Default;
    }

    private void OnButtonClick()
    {

    }
}
