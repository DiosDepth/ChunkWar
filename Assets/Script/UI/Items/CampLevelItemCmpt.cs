using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CampLevelItemCmpt : EnhancedScrollerCellView
{
    private Image _fillImage;
    private Image _icon;
    private TextMeshProUGUI _levelText;
    private TextMeshProUGUI _nameText;

    protected override void Awake()
    {
        base.Awake();
        _icon = transform.Find("Top/Icon").SafeGetComponent<Image>();
        _fillImage = transform.Find("Slider/SliderImage").SafeGetComponent<Image>();
        _levelText = transform.Find("Slider/LevelInfo/Level").SafeGetComponent<TextMeshProUGUI>();
        _nameText = transform.Find("Top/Name").SafeGetComponent<TextMeshProUGUI>();
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

        int levelIndex = (int)(ItemUID % 10000);
        var levelCfg = CampData.GetLevelConfig(levelIndex);
        if(levelCfg != null)
        {
            _levelText.text = string.Format("LV.{0}", levelCfg.LevelIndex);
            SetUpUnlockItem(levelCfg);
        }

        ///Fill Slider
        var currentLevel = CampData.GetCampLevel;
        if (levelIndex < currentLevel)
        {
            _fillImage.fillAmount = 1;
        }
        else if (levelIndex == currentLevel)
        {
            _fillImage.fillAmount = CampData.GetCurrentLevelEXPProgress();
        }
        else
        {
            _fillImage.fillAmount = 0;
        }
    }

    private void SetUpUnlockItem(CampLevelConfig cfg)
    {
        Sprite sprite = null;
        string name = string.Empty;

        if(cfg.UnlockType == GeneralUnlockItemType.ShipUnit)
        {

        }
        else if (cfg.UnlockType == GeneralUnlockItemType.ShipPlug)
        {
            var plugItem = DataManager.Instance.GetShipPlugItemConfig(cfg.UnlockItemID);
            if(plugItem != null)
            {
                sprite = plugItem.GeneralConfig.IconSprite;
                name = plugItem.GeneralConfig.Name;
            }
        }
        _icon.sprite = sprite;
        _nameText.text = LocalizationManager.Instance.GetTextValue(name);
    }

    protected override void SelectedChanged(bool selected)
    {
        base.SelectedChanged(selected);
    }
}
