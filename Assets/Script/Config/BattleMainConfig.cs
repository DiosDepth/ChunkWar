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

public enum ModifySpecialType
{
    /// <summary>
    /// 低于100%的部分
    /// </summary>
    Less100,
    /// <summary>
    /// 超出100%的部分
    /// </summary>
    More100,
    /// <summary>
    /// 数量
    /// </summary>
    Count,
}

public enum ModifyTriggerType
{
    OnKillEnemy,
    OnWaveEnd,
    OnWaveStart,
    /// <summary>
    /// 属性互相转化
    /// </summary>
    PropertyTransfer,
    /// <summary>
    /// 根据物件转化
    /// </summary>
    ItemTransfer,
    WaveState,
    Timer,
    /// <summary>
    /// 添加时
    /// </summary>
    OnAdd,
    OnPlayerShipMove,
    OnRefreshShop,
    ItemRarityCount,
    OnEnterHarbor,
    /// <summary>
    /// 核心百分比
    /// </summary>
    ByCoreHPPercent,
    OnShieldRecover,
    OnWeaponHitTarget,
    OnPlayerWeaponReload,
    OnPlayerWeaponFire,
    OnPlayerShipParry,
    OnShieldBroken,
    OnPlayerCoreUnitTakeDamage,
}

public enum ModifyTriggerEffectType
{
    AddPropertyValue,
    SetPropertyMaxValue,
    SetPropertyValue,
    TempReduceShopPrice,
    AddPropertyValueBySpecialCount,
    SetUnitPropertyModifyKey,
    EnterUnitState,
    GainDropWaste,
    AddGlobalTimerModifier,
    AddUnitTimerModifier,
    CreateExplode,
    CreateDamage,
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

    public byte ShipMaxLevel;
    public float TransfixionReduce_Max = 0;
    /// <summary>
    /// 装甲减伤计算参数
    /// </summary>
    public float PlayerShip_ArmorDamageReduce_Param = 25;
    public float PlayerShip_ShieldDamageReduce_Param = 25;

    /// <summary>
    /// 护盾多久恢复一次
    /// </summary>
    public float ShieldRecoverCD = 0.5f;
    public float ShieldRatioMin = 1f;
    public float ShipBaseSuckerRange = 10f;
    public float ShipMinSuckerRange = 5f;
    public int ShipLoadBase = 50;
    public float Drop_Waste_LoadCostBase;

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
    [FoldoutGroup("基础信息")]
    public string Name;
    [FoldoutGroup("基础信息")]
    public string Desc;
    [FoldoutGroup("基础信息")]
    public string PropertyDesc;
    [FoldoutGroup("基础信息")]
    public string UnlockDesc;
    [FoldoutGroup("基础信息")]
    public int PreviewScore;
    [FoldoutGroup("基础信息")]
    public Sprite Icon;
    [FoldoutGroup("基础信息")]
    public bool DefaultUnlock;
    [FoldoutGroup("基础信息")]
    public int LevelPresetID;
    [FoldoutGroup("基础信息")]
    public float ScoreRatio;

    [FoldoutGroup("初始设置")]
    public int StartCurrency;

    [FoldoutGroup("初始设置")]
    [DictionaryDrawerSettings()]
    public Dictionary<GoodsItemRarity, byte> StartWreckageDic = new Dictionary<GoodsItemRarity, byte>();

    [FoldoutGroup("初始设置")]
    [DictionaryDrawerSettings()]
    public Dictionary<HardLevelModifyType, float> ModifyDic = new Dictionary<HardLevelModifyType, float>();

}

[System.Serializable]
[HideReferenceObjectPicker]
public class PropertyDisplayConfig
{
    public string NameText;
    public string ModifyPercentNameText;
    public string DescText;
    public Sprite Icon;
    public bool IsPercent;
    public bool ReverseColor = false;
}

[System.Serializable]
public class PropertyModifyConfig
{
    [HorizontalGroup("B", 250)]
    [LabelText("修正Key")]
    [LabelWidth(60)]
    public PropertyModifyKey ModifyKey;

    [HorizontalGroup("B", 100)]
    [LabelText("值")]
    [LabelWidth(30)]
    public float Value;

    [HorizontalGroup("B", 100)]
    [LabelText("使用特殊修正")]
    [LabelWidth(80)]
    public bool BySpecialValue;

    [ShowIf("BySpecialValue")]
    [HorizontalGroup("C", 200)]
    [LabelText("类型")]
    [LabelWidth(30)]
    public ModifySpecialType SpecialType;

    [ShowIf("BySpecialValue")]
    [HorizontalGroup("C", 250)]
    [LabelText("Key参数")]
    [LabelWidth(60)]
    public string SpecialKeyParam;

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
