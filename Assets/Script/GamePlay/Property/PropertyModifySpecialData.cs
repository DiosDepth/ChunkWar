using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 非静态加成数据
/// </summary>
public class PropertyModifySpecialData 
{
    public PropertyModifyConfig Config;

    private uint UID;
    public uint GetUID
    {
        get { return UID; }
    }


    public PropertyModifySpecialData(PropertyModifyConfig cfg, uint parentUID)
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

        if (Config.SpecialType == ModifySpecialType.Less100)
        {
            ///不足100%的部分等比转换
            switch (Config.SpecialKeyParam)
            {
                case "LoadPercent":
                    mgr.OnWreckageLoadPercentChange += OnPercentChange_Less100;
                    OnPercentChange_Less100(mgr.WreckageLoadPercent);
                    break;

                case "EnergyPercent":
                    mgr.OnEnergyPercentChange += OnPercentChange_Less100;
                    var currentShipEnergyPercent = RogueManager.Instance.currentShip.EnergyPercent;
                    OnPercentChange_Less100(currentShipEnergyPercent);
                    break;
            }
        }
        else if (Config.SpecialType == ModifySpecialType.More100)
        {
            ///超过00%的部分等比转换
            switch (Config.SpecialKeyParam)
            {
                case "LoadPercent":
                    mgr.OnWreckageLoadPercentChange += OnPercentChange_More100;
                    OnPercentChange_More100(mgr.WreckageLoadPercent);
                    break;

                case "EnergyPercent":
                    mgr.OnEnergyPercentChange += OnPercentChange_More100;
                    var currentShipEnergyPercent = RogueManager.Instance.currentShip.EnergyPercent;
                    OnPercentChange_More100(currentShipEnergyPercent);
                    break;

                case "Critical":
                    mgr.MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.Critical, OnCriticalChange_More100);
                    OnCriticalChange_More100();
                    break;
            }
        }
        else if(Config.SpecialType == ModifySpecialType.Count)
        {
            switch (Config.SpecialKeyParam)
            {
                case "EnemyCount":
                    LevelManager.Instance.OnEnemyCountChange += OnEnemyCountChange;
                    var currentEnemyCount = ECSManager.Instance.CurrentAIAgentCount;
                    OnEnemyCountChange(currentEnemyCount);
                    break;
            }
        }
    }

    public void OnRemove()
    {
        var mgr = RogueManager.Instance;
        if (Config.SpecialType == ModifySpecialType.Less100)
        {
            ///不足100%的部分转换
            switch (Config.SpecialKeyParam)
            {
                case "LoadPercent":
                    mgr.OnWreckageLoadPercentChange -= OnPercentChange_Less100;
                    break;
                case "EnergyPercent":
                    mgr.OnEnergyPercentChange -= OnPercentChange_Less100;
                    break;
            }
        }
        else if (Config.SpecialType == ModifySpecialType.More100)
        {
            switch (Config.SpecialKeyParam)
            {
                case "LoadPercent":
                    mgr.OnWreckageLoadPercentChange -= OnPercentChange_More100;
                    break;
                case "EnergyPercent":
                    mgr.OnEnergyPercentChange -= OnPercentChange_More100;
                    break;
                case "Critical":
                    mgr.MainPropertyData.UnBindPropertyChangeAction(PropertyModifyKey.Critical, OnCriticalChange_More100);
                    break;
            }
        }
        else if (Config.SpecialType == ModifySpecialType.Count)
        {
            switch (Config.SpecialKeyParam)
            {
                case "EnemyCount":
                    LevelManager.Instance.OnEnemyCountChange -= OnEnemyCountChange;
                    break;
            }
        }
        mgr.MainPropertyData.RemovePropertyModifyValue(Config.ModifyKey, PropertyModifyType.Modify, UID);
    }

    /// <summary>
    /// 获取最终值
    /// </summary>
    /// <returns></returns>
    private float GetFinialConfigValue()
    {
        ///惩罚效果比例
        if(UID == GameGlobalConfig.PropertyModifyUID_WreckageOverload_GlobalBuff)
        {
            var modifyRate = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.LoadPunishRate);
            modifyRate = Mathf.Clamp(modifyRate, -100f, float.MaxValue);

            return Config.Value * (1 + modifyRate / 100f);
        }
        else if (UID == GameGlobalConfig.PropertyModifyUID_EnergyOverload_GlobalBuff)
        {
            var modifyRate = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.EnergyPunishRate);
            modifyRate = Mathf.Clamp(modifyRate, -100f, float.MaxValue);

            return Config.Value * (1 + modifyRate / 100f);
        }

        return Config.Value;
    }

    private void OnPercentChange_Less100(float percent)
    {
        if(percent >= 100)
        {
            ///加成归0
            RogueManager.Instance.MainPropertyData.SetPropertyModifyValue(Config.ModifyKey, PropertyModifyType.Modify, UID, 0);
        }

        float delta = 100 - percent;
        delta = Mathf.Clamp(delta, 0, 100f);
        RogueManager.Instance.MainPropertyData.SetPropertyModifyValue(Config.ModifyKey, PropertyModifyType.Modify, UID, delta * GetFinialConfigValue());
    }


    private void OnPercentChange_More100(float percent)
    {
        if (percent <= 100)
        {
            ///加成归0
            RogueManager.Instance.MainPropertyData.SetPropertyModifyValue(Config.ModifyKey, PropertyModifyType.Modify, UID, 0);
        }

        float delta = percent - 100;
        ///上限设置
        if (UID == GameGlobalConfig.PropertyModifyUID_WreckageOverload_GlobalBuff)
        {
            delta = Mathf.Clamp(delta, 0, DataManager.Instance.gameMiscCfg.WreckageOverload_MaxPercent);
        }
        else if(UID == GameGlobalConfig.PropertyModifyUID_EnergyOverload_GlobalBuff)
        {
            delta = Mathf.Clamp(delta, 0, DataManager.Instance.gameMiscCfg.EnergyOverload_MaxPercent);
        }

        RogueManager.Instance.MainPropertyData.SetPropertyModifyValue(Config.ModifyKey, PropertyModifyType.Modify, UID, delta * GetFinialConfigValue());
    }

    /// <summary>
    /// 给Property 属性用
    /// </summary>
    /// <param name="value"></param>
    /// <param name="nil"></param>
    private void OnCriticalChange_More100()
    {
        var value = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.Critical);
        OnPercentChange_More100(value);
    }

    /// <summary>
    /// 敌人数量变化
    /// </summary>
    /// <param name="count"></param>
    private void OnEnemyCountChange(int count)
    {
        var finalValue = Config.Value * count;
        RogueManager.Instance.MainPropertyData.SetPropertyModifyValue(Config.ModifyKey, PropertyModifyType.Modify, UID, finalValue);
    }
    
}
