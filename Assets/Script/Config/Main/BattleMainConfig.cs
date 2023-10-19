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
    /// ����100%�Ĳ���
    /// </summary>
    Less100,
    /// <summary>
    /// ����100%�Ĳ���
    /// </summary>
    More100,
    /// <summary>
    /// ����
    /// </summary>
    Count,
}

public enum ModifyTriggerType
{
    OnKillEnemy,
    OnWaveEnd,
    OnWaveStart,
    /// <summary>
    /// ���Ի���ת��
    /// </summary>
    PropertyTransfer,
    /// <summary>
    /// �������ת��
    /// </summary>
    ItemTransfer,
    WaveState,
    Timer,
    /// <summary>
    /// ���ʱ
    /// </summary>
    OnAdd,
    OnPlayerShipMove,
    OnRefreshShop,
    ItemRarityCount,
    OnEnterHarbor,
    /// <summary>
    /// ���İٷֱ�
    /// </summary>
    ByCoreHPPercent,
    OnShieldRecover,
    OnWeaponHitTarget,
    OnPlayerWeaponReload,
    OnPlayerWeaponFire,
    OnPlayerShipParry,
    OnShieldBroken,
    OnPlayerUnitTakeDamage,
    OnPlayerCreateExplode,
    OnBuyShopItem,
    OnCollectPickable,
    /// <summary>
    /// ��λ̱��
    /// </summary>
    OnPlayerUnitParalysis,
    OnEnterShop,
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
    GainCurrency,
    ModifyDamgeByTargetDistance,
    AddUnitModifier,
    HealUnitHP,
}

public class BattleMainConfig : SerializedScriptableObject
{
    public byte RogueShop_Origin_RefreshNum = 4;
    public byte HarborMapTempUnitSlotCount = 6;
    public byte ShipLevelUp_GrowthItem_Count = 4;
    public float BattleOption_ColliderRaidus = 5;

    /// <summary>
    /// hardLevelÿX������1
    /// </summary>
    public ushort HardLevelDeltaSeconds = 120;

    public byte ShipMaxLevel;
    public float TransfixionDamage_Base = 60;
    public float TransfixionDamage_Max = 100;
    /// <summary>
    /// װ�׼��˼������
    /// </summary>
    public float PlayerShip_ArmorDamageReduce_Param = 25;
    public float PlayerShip_ShieldDamageReduce_Param = 25;

    /// <summary>
    /// ���ܶ�ûָ�һ��
    /// </summary>
    public float ShieldRecoverCD = 0.5f;
    public float ShieldRatioMin = 1f;
    public float ShipBaseSuckerRange = 10f;
    public float ShipMinSuckerRange = 5f;
    public int ShipLoadBase = 50;
    public float Drop_Waste_LoadCostBase;

    public BattleSpecialEntitySpawnConfig EntitySpawnConfig = new BattleSpecialEntitySpawnConfig();

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
                Debug.LogError("������������ϡ�ж����ô��� Name = " + ShipLevelUpGrowthItems[i].Name);
            }
        }
    }

#endif
}

[HideReferenceObjectPicker]
public class HardLevelConfig
{
    public int HardLevelID;
    [FoldoutGroup("������Ϣ")]
    public string Name;
    [FoldoutGroup("������Ϣ")]
    public string Desc;
    [FoldoutGroup("������Ϣ")]
    public string PropertyDesc;
    [FoldoutGroup("������Ϣ")]
    public string UnlockDesc;
    [FoldoutGroup("������Ϣ")]
    public int PreviewScore;
    [FoldoutGroup("������Ϣ")]
    public Sprite Icon;
    [FoldoutGroup("������Ϣ")]
    public bool DefaultUnlock;
    [FoldoutGroup("������Ϣ")]
    public int LevelPresetID;
    [FoldoutGroup("������Ϣ")]
    public float ScoreRatio;

    [FoldoutGroup("��ʼ����")]
    public int StartCurrency;

    [FoldoutGroup("��ʼ����")]
    [DictionaryDrawerSettings()]
    public Dictionary<GoodsItemRarity, byte> StartWreckageDic = new Dictionary<GoodsItemRarity, byte>();

    [FoldoutGroup("��ʼ����")]
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
    public int TextSpriteIndex = -1;
}

[System.Serializable]
public class PropertyModifyConfig
{
    [HorizontalGroup("B", 250)]
    [LabelText("����Key")]
    [LabelWidth(60)]
    public PropertyModifyKey ModifyKey;

    [HorizontalGroup("B", 100)]
    [LabelText("ֵ")]
    [LabelWidth(30)]
    public float Value;

    [HorizontalGroup("B", 100)]
    [LabelText("ʹ����������")]
    [LabelWidth(80)]
    public bool BySpecialValue;

    [ShowIf("BySpecialValue")]
    [HorizontalGroup("C", 200)]
    [LabelText("����")]
    [LabelWidth(30)]
    public ModifySpecialType SpecialType;

    [ShowIf("BySpecialValue")]
    [HorizontalGroup("C", 250)]
    [LabelText("Key����")]
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
    [LabelText("����Ȩ��")]
    [LabelWidth(80)]
    public float WeightBase;

    [HorizontalGroup("Ship", 200)]
    [LabelText("ÿ�ȼ�����Ȩ��")]
    [LabelWidth(80)]
    public float WeightAddPerLevel;

    [HorizontalGroup("Ship", 200)]
    [LabelText("���Ȩ��")]
    [LabelWidth(80)]
    public float WeightMax;
}

[HideReferenceObjectPicker]
public class BattleSpecialEntitySpawnConfig
{
    public float EnemyGenerate_Inner_Range = 25f;
    public float EnemyGenerate_Outer_Range = 45f;
    /// <summary>
    /// ��ʯ���ɸ��ʲ���1
    /// </summary>
    public float MeteoriteGenerate_Rate1;
    public int MeteoriteGenerate_Timer1;
    /// <summary>
    /// ��ʯƽ������
    /// </summary>
    public float MeteoriteGenerate_Rate2;
    public int MeteoriteGenerate_Timer2;

    public float AncientUnitGenerate_Rate1;
    public int AncientUnitGenerate_Timer1;
    public float AncientUnitGenerate_Rate2;
    public int AncientUnitGenerate_Timer2;

    public int AncientUnit_ShipID;
    public int AncientUnit_ProtectRate_Add;

    public Dictionary<GoodsItemRarity, int> MeteoriteGenerate_TypeID_Map = new Dictionary<GoodsItemRarity, int>();

    public int GetMeteoriteGenerate_TypeID(GoodsItemRarity rarity)
    {
        if (MeteoriteGenerate_TypeID_Map.ContainsKey(rarity))
            return MeteoriteGenerate_TypeID_Map[rarity];

        return -1;
    }
}