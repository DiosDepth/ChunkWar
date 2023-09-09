using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 非静态加成数据
/// </summary>
public class PropertyModifySpecialData 
{
    public PropertyMidifyConfig Config;

    private uint UID;

    public PropertyModifySpecialData(PropertyMidifyConfig cfg, uint parentUID)
    {
        this.Config = cfg;
        this.UID = parentUID;
    }

    /// <summary>
    /// 处理属性特殊加成
    /// </summary>
    /// <param name="cfg"></param>
    public void HandlePropetyModifyBySpecialValue()
    {
        var mgr = RogueManager.Instance;

        if (Config.SpecialType == ModifySpecialType.Less100OneByOne)
        {
            ///不足100%的部分等比转换
            switch (Config.SpecialKeyParam)
            {
                case "LoadPercent":
                    mgr.OnWreckageLoadPercentChange += OnWreckageLoadPercentChange_Less100;
                    OnWreckageLoadPercentChange_Less100(mgr.WreckageLoadPercent * 100);
                    break;
            }
        }
    }

    public void OnRemove()
    {
        var mgr = RogueManager.Instance;
        if (Config.SpecialType == ModifySpecialType.Less100OneByOne)
        {
            ///不足100%的部分等比转换
            switch (Config.SpecialKeyParam)
            {
                case "LoadPercent":
                    mgr.OnWreckageLoadPercentChange -= OnWreckageLoadPercentChange_Less100;
                    mgr.MainPropertyData.RemovePropertyModifyValue(Config.ModifyKey, PropertyModifyType.Modify, UID);
                    break;
            }
        }
    }

    private void OnWreckageLoadPercentChange_Less100(float percent)
    {
        if(percent >= 100)
        {
            ///加成归0
            RogueManager.Instance.MainPropertyData.SetPropertyModifyValue(Config.ModifyKey, PropertyModifyType.Modify, UID, 0);
        }

        float delta = 100 - percent;
        delta = Mathf.Clamp(delta, 0, 100f);
        RogueManager.Instance.MainPropertyData.SetPropertyModifyValue(Config.ModifyKey, PropertyModifyType.Modify, UID, delta);
    }

    
}
