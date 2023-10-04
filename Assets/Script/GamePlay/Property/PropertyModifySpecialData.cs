using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �Ǿ�̬�ӳ�����
/// </summary>
public class PropertyModifySpecialData 
{
    public PropertyModifyConfig Config;

    private uint UID;

    public PropertyModifySpecialData(PropertyModifyConfig cfg, uint parentUID)
    {
        this.Config = cfg;
        this.UID = parentUID;
    }

    /// <summary>
    /// ������������ӳ�
    /// </summary>
    /// <param name="cfg"></param>
    public void HandlePropetyModifyBySpecialValue()
    {
        var mgr = RogueManager.Instance;

        if (Config.SpecialType == ModifySpecialType.Less100)
        {
            ///����100%�Ĳ��ֵȱ�ת��
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
            ///����00%�Ĳ��ֵȱ�ת��
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
            }
        }
        else if(Config.SpecialType == ModifySpecialType.Count)
        {
            switch (Config.SpecialKeyParam)
            {
                case "EnemyCount":
                    LevelManager.Instance.OnEnemyCountChange += OnEnemyCountChange;
                    var currentEnemyCount = AIManager.Instance.ShipCount;
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
            ///����100%�Ĳ���ת��
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
    /// ��ȡ����ֵ
    /// </summary>
    /// <returns></returns>
    private float GetFinialConfigValue()
    {
        ///�ͷ�Ч������
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
            ///�ӳɹ�0
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
            ///�ӳɹ�0
            RogueManager.Instance.MainPropertyData.SetPropertyModifyValue(Config.ModifyKey, PropertyModifyType.Modify, UID, 0);
        }

        float delta = percent - 100;
        delta = Mathf.Clamp(delta, 0, float.MaxValue);
        RogueManager.Instance.MainPropertyData.SetPropertyModifyValue(Config.ModifyKey, PropertyModifyType.Modify, UID, delta * GetFinialConfigValue());
    }

    /// <summary>
    /// ���������仯
    /// </summary>
    /// <param name="count"></param>
    private void OnEnemyCountChange(int count)
    {
        var finalValue = Config.Value * count;
        RogueManager.Instance.MainPropertyData.SetPropertyModifyValue(Config.ModifyKey, PropertyModifyType.Modify, UID, finalValue);
    }
    
}
