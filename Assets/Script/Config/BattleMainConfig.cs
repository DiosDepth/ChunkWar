using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public enum HardLevelModifyType
{
    EnemyHP,
    EnemyDamage,
}

public class BattleMainConfig : SerializedScriptableObject
{
    public byte RogueShop_Origin_RefreshNum = 4;
    public byte HarborMapTempUnitSlotCount = 6;
    public byte ShipLevelUp_GrowthItem_Count = 4;

    /// <summary>
    /// hardLevel每X秒增加1
    /// </summary>
    public ushort HardLevelDeltaSeconds = 120;
    public byte HardLevelWaveIndexMultiple = 20;

    public byte ShipMaxLevel;
    public float TransfixionReduce_Max = 0;
    /// <summary>
    /// 装甲减伤计算参数
    /// </summary>
    public float PlayerShip_ArmorDamageReduce_Param = 25;

    /// <summary>
    /// 护盾多久恢复一次
    /// </summary>
    public float ShieldRecoverCD = 0.5f;
    public float ShieldRatioMin = 1f;
    public float ShipBaseSuckerRange = 10f;
    public float ShipMinSuckerRange = 5f;
    public int ShipLoadBase = 50;

    public byte[] EvolveRequireMap = new byte[3];
    public byte[] EvolveAddMap = new byte[4]; 

    [ListDrawerSettings(ShowIndexLabels = true, NumberOfItemsPerPage = 10)]
    public int[] EXPMap;

    public List<HardLevelConfig> HardLevels = new List<HardLevelConfig>();

    [DictionaryDrawerSettings()]
    public Dictionary<PropertyModifyKey, PropertyDisplayConfig> PropertyDisplay = new Dictionary<PropertyModifyKey, PropertyDisplayConfig>();

    public List<ShipLevelUpGrowthItemConfig> ShipLevelUpGrowthItems = new List<ShipLevelUpGrowthItemConfig>();
    public List<ShipLevelUpItemRarityConfig> ShipLevelUpItemRarityMap = new List<ShipLevelUpItemRarityConfig>();

    public PropertyDisplayConfig GetPropertyDisplayConfig(PropertyModifyKey key)
    {
        if (PropertyDisplay.ContainsKey(key))
            return PropertyDisplay[key];
        return null;
    }

    public HardLevelConfig GetHardLevelConfig(int hardLevelID)
    {
        return HardLevels.Find(x => x.HardLevelID == hardLevelID);
    }

    public ShipLevelUpItemRarityConfig GetShipLevelUpItemRarityConfigByRarity(GoodsItemRarity rarity)
    {
        return ShipLevelUpItemRarityMap.Find(x => x.Rarity == rarity);
    }

#if UNITY_EDITOR

    [OnInspectorDispose]
    private void OnDispose()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public void DataCheck()
    {
        for (int i = 0; i < ShipLevelUpGrowthItems.Count; i++)
        {
            if(ShipLevelUpGrowthItems[i].RarityValueMap.Length != 4)
            {
                Debug.LogError("舰船升级属性稀有度配置错误！ Name = " + ShipLevelUpGrowthItems[i].Name);
            }
        }
    }

#endif
}

[HideReferenceObjectPicker]
public class HardLevelConfig
{
    public int HardLevelID;
    public string Name;
    public string Desc;
    public Sprite Icon;

    public int LevelPresetID;

    [DictionaryDrawerSettings()]
    public Dictionary<HardLevelModifyType, float> ModifyDic = new Dictionary<HardLevelModifyType, float>();

}

[System.Serializable]
[HideReferenceObjectPicker]
public class PropertyDisplayConfig
{
    public string NameText;
    public Sprite Icon;
    public bool IsPercent;
    public bool ReverseColor = false;
}

[System.Serializable]
public class PropertyMidifyConfig
{
    [HorizontalGroup("B", 300)]
    [LabelText("修正Key")]
    [LabelWidth(100)]
    public PropertyModifyKey ModifyKey;

    [HorizontalGroup("B", 200)]
    [LabelText("值")]
    [LabelWidth(80)]
    public float Value;
}

[System.Serializable]
public class ShipLevelUpGrowthItemConfig
{
    public Sprite Icon;
    public string Name;
    public PropertyModifyKey ModifyKey;
    public float[] RarityValueMap = new float[4];
}

[System.Serializable]
public class ShipLevelUpItemRarityConfig
{
    public GoodsItemRarity Rarity;
    public int MinLevel;

    [HorizontalGroup("Ship", 200)]
    [LabelText("基础权重")]
    [LabelWidth(80)]
    public float WeightBase;

    [HorizontalGroup("Ship", 200)]
    [LabelText("每等级增加权重")]
    [LabelWidth(80)]
    public float WeightAddPerLevel;

    [HorizontalGroup("Ship", 200)]
    [LabelText("最大权重")]
    [LabelWidth(80)]
    public float WeightMax;
}