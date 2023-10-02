using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CampLevelItemCmpt : EnhancedScrollerCellView, IHoverUIItem
{
    private Image _fillImage;
    private Image _icon;
    private Image _frameBG;
    private TextMeshProUGUI _levelText;
    private TextMeshProUGUI _nameText;

    private CampLevelConfig _cfg;
    private DetailHoverItemBase _hoverItem;

    protected override void Awake()
    {
        base.Awake();
        transform.Find("Top/Icon").SafeGetComponent<GeneralHoverItemControl>().item = this;
        _icon = transform.Find("Top/Icon/Icon").SafeGetComponent<Image>();
        _frameBG = transform.Find("Top/Icon/Frame").SafeGetComponent<Image>();
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
        if (levelIndex <= currentLevel)
        {
            _fillImage.fillAmount = 1;
        }
        else if (levelIndex == currentLevel + 1)
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
        _cfg = cfg;
        Sprite sprite = null;
        Sprite frame = null;
        string name = string.Empty;

        if(cfg.UnlockType == GeneralUnlockItemType.ShipUnit)
        {
            var unitItem = DataManager.Instance.GetUnitConfig(cfg.UnlockItemID);
            if(unitItem != null)
            {
                sprite = unitItem.GeneralConfig.IconSprite;
                name = unitItem.GeneralConfig.Name;
                frame = GameHelper.GetRarityBGSprite(unitItem.GeneralConfig.Rarity);
            }
        }
        else if (cfg.UnlockType == GeneralUnlockItemType.ShipPlug)
        {
            var plugItem = DataManager.Instance.GetShipPlugItemConfig(cfg.UnlockItemID);
            if(plugItem != null)
            {
                sprite = plugItem.GeneralConfig.IconSprite;
                name = plugItem.GeneralConfig.Name;
                frame = GameHelper.GetRarityBGSprite(plugItem.GeneralConfig.Rarity);
            }
        }
        _frameBG.sprite = frame;
        _icon.sprite = sprite;
        _nameText.text = LocalizationManager.Instance.GetTextValue(name);
    }

    protected override void SelectedChanged(bool selected)
    {
        base.SelectedChanged(selected);
    }

    public void OnHoverEnter()
    {
        if (_cfg == null)
            return;

        if(_cfg.UnlockType == GeneralUnlockItemType.ShipPlug)
        {
            UIManager.Instance.CreatePoolerUI<PlugDetailHover>("PlugDetailHover", true, E_UI_Layer.Top, null, (panel) =>
            {
                panel.Initialization(_cfg.UnlockItemID);
                _hoverItem = panel;
            });
        }
        else if(_cfg.UnlockType == GeneralUnlockItemType.ShipUnit)
        {
            UIManager.Instance.CreatePoolerUI<UnitDetailHover>("UnitDetailHover", true, E_UI_Layer.Top, null, (panel) =>
            {
                panel.Initialization(_cfg.UnlockItemID);
                _hoverItem = panel;
            });
        }
    }

    public void OnHoverExit()
    {
        if(_hoverItem != null)
        {
            _hoverItem.PoolableDestroy();
        }
    }

    private void OnDestroy()
    {
        if (_hoverItem != null)
        {
            _hoverItem.PoolableDestroy();
        }
    }

}
