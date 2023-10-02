using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlugDetailHover : DetailHoverItemBase
{
    protected override void Awake()
    {
        base.Awake();
    }

    public override void Initialization(params object[] param)
    {
        base.Initialization(param);
    }

    protected override void SetUp(int id)
    {
        var plugCfg = DataManager.Instance.GetShipPlugItemConfig(id);
        if (plugCfg == null)
            return;

        _nameText.text = LocalizationManager.Instance.GetTextValue(plugCfg.GeneralConfig.Name);
        _nameText.color = GameHelper.GetRarityColor(plugCfg.GeneralConfig.Rarity);
        _rarityBG.sprite = GameHelper.GetRarityBGSprite(plugCfg.GeneralConfig.Rarity);
        _icon.sprite = plugCfg.GeneralConfig.IconSprite;
        if (!string.IsNullOrEmpty(plugCfg.GeneralConfig.Desc))
        {
            _descText.text = LocalizationManager.Instance.GetTextValue(plugCfg.GeneralConfig.Desc);
        }
        else
        {
            _descText.text = string.Empty;
        }
        SetUpProperty(plugCfg);
        base.SetUp(id);
    }


}
