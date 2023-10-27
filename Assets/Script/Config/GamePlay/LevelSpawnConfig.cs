using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSpawnConfig : SerializedScriptableObject
{
    [System.Serializable]
    public class EliteRandomSpawnConfig
    {
        [LabelText("波次ID")]
        public int WaveIndex;
        [LabelText("过滤精英ID")]
        public int[] ExceptEliteIDs = new int[0];
        [LabelText("生成时间列表")]
        public ushort[] EliteSpawnTimeList = new ushort[0];
    }

    [LabelText("关卡预设ID")]
    [ReadOnly]
    public int LevelPresetID;

    public string BGMEvent;

    [FoldoutGroup("特殊敌人随机生成")]
    [HorizontalGroup("特殊敌人随机生成/A", 300)]
    [LabelText("启用生成")]
    [LabelWidth(100)]
    public bool EnableSpecialEnemySpawn = false;

    [FoldoutGroup("特殊敌人随机生成")]
    [HorizontalGroup("特殊敌人随机生成/A")]
    [LabelText("精英累计生成的波次数")]
    [LabelWidth(150)]
    public byte TotalSpawnWaveCount = 0;

    [FoldoutGroup("特殊敌人随机生成")]
    [HorizontalGroup("特殊敌人随机生成/C")]
    [LabelText("生成配置")]
    [LabelWidth(100)]
    public List<EliteRandomSpawnConfig> EliteRandomSpawnCfg = new List<EliteRandomSpawnConfig>();

    [ListDrawerSettings(CustomAddFunction = "AddNewWave", NumberOfItemsPerPage = 3)]
    [Title("波次信息配置", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("波次")]
    public List<WaveConfig> WaveConfig = new List<WaveConfig>();

    private WaveConfig AddNewWave()
    {
        return new WaveConfig();
    }

    public EliteRandomSpawnConfig GetEliteSpawnConfig(int waveIndex)
    {
        return EliteRandomSpawnCfg.Find(x => x.WaveIndex == waveIndex);
    }

#if UNITY_EDITOR
    [OnInspectorDispose]
    private void OnDispose()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

#endif
}

[HideReferenceObjectPicker]
public class WaveConfig
{
    [LabelText("波次ID")]
    [LabelWidth(80)]
    [HorizontalGroup("AA", 200)]
    public int WaveIndex;

    [LabelText("持续时长")]
    [LabelWidth(80)]
    [HorizontalGroup("AA", 200)]
    public ushort DurationTime;

    [LabelText("商船刷新表")]
    [LabelWidth(80)]
    [HorizontalGroup("AA", 200)]
    public ushort[] ShopRefreshTimeMap = new ushort[0];

    [LabelText("无限敌人")]
    [LabelWidth(80)]
    [HorizontalGroup("AA", 150)]
    public bool EndlessEnemy;

    [LabelText("波次积分")]
    [LabelWidth(80)]
    [HorizontalGroup("AA", 200)]
    public int waveScore;

    [LabelText("敌人波次配置")]
    [ListDrawerSettings(CustomAddFunction = "AddNewWaveSpawn")]
    [OnCollectionChanged("OnAddedWave")]
    public List<WaveEnemySpawnConfig> SpawnConfig = new List<WaveEnemySpawnConfig>();

    /// <summary>
    /// 陨石生成稀有度权重
    /// </summary>
    [FoldoutGroup("自动生成配置")]
    [LabelText("陨石生成配置")]
    public Dictionary<GoodsItemRarity, byte> MeteoriteGenerateRate_RarityMap = new Dictionary<GoodsItemRarity, byte>()
    {
        { GoodsItemRarity.Tier1, 0 },
        { GoodsItemRarity.Tier2, 0 },
        { GoodsItemRarity.Tier3, 0 },
        { GoodsItemRarity.Tier4, 0 },
    };

    [FoldoutGroup("自动生成配置")]
    [HorizontalGroup("自动生成配置/A")]
    [LabelText("远古飞船保底")]
    [LabelWidth(100)]
    public bool UseAncientUnitSpawnProtect = false;

    [FoldoutGroup("自动生成配置")]
    [HorizontalGroup("自动生成配置/A")]
    [LabelText("远古飞船保底数")]
    [LabelWidth(100)]
    public int AncientUnitSpawnProtectedCount;

    [FoldoutGroup("自动生成配置")]
    [HorizontalGroup("自动生成配置/A")]
    [LabelText("远古飞船最大数量")]
    [LabelWidth(100)]
    public int AncientUnitSpawnMax;

    [FoldoutGroup("自动生成配置")]
    [HorizontalGroup("自动生成配置/A")]
    [LabelText("覆盖远古飞船间隔")]
    [LabelWidth(100)]
    public bool OverrideAncientUnitSpawnSoomthTimeDelta = false;

    [FoldoutGroup("自动生成配置")]
    [HorizontalGroup("自动生成配置/A")]
    [LabelText("间隔覆盖")]
    [LabelWidth(100)]
    public int OverrideSpawnTime;

    private WaveEnemySpawnConfig AddNewWaveSpawn()
    {
        return new WaveEnemySpawnConfig();
    }

#if UNITY_EDITOR
    private void OnAddedWave(CollectionChangeInfo info, object value)
    {
        for (int i = 0; i < SpawnConfig.Count; i++) 
        {
            SpawnConfig[i].ID = i + 1;
        }
    }

#endif
}

/// <summary>
/// 敌人刷新配置
/// </summary>
[HideReferenceObjectPicker]
public class WaveEnemySpawnConfig
{
    [ReadOnly]
    [HorizontalGroup("BB", 100)]
    [LabelText("ID")]
    [LabelWidth(20)]
    public int ID;

    [HorizontalGroup("BB", 150)]
    [LabelText("单位类型ID")]
    [LabelWidth(80)]
    [OnValueChanged("OnTypeIDChange")]
    [DelayedProperty]
    public int AITypeID;

    [HorizontalGroup("BB", 200)]
    [ShowInInspector]
    [LabelText("AI名")]
    [LabelWidth(50)]
    [ReadOnly]
    private string AITypeName;

    [HorizontalGroup("BB", 160)]
    [LabelText("循环间隔")]
    [LabelWidth(60)]
    public int DurationDelta;

    [HorizontalGroup("BB", 160)]
    [LabelText("循环次数")]
    [LabelWidth(60)]
    public int LoopCount;

    [HorizontalGroup("BB", 160)]
    [LabelText("开始时间（秒）")]
    [LabelWidth(100)]
    public int StartTime;

    [FoldoutGroup("队形与数量配置")]
    [HorizontalGroup("队形与数量配置/CC", 160)]
    [LabelText("敌人数量")]
    [LabelWidth(80)]
    [MinValue(1)]
    public int TotalCount = 1;

    [FoldoutGroup("队形与数量配置")]
    [HorizontalGroup("队形与数量配置/CC", 160)]
    [LabelText("每行敌人")]
    [LabelWidth(80)]
    [MinValue(1)]
    public int MaxRowCount = 1;

    [FoldoutGroup("队形与数量配置")]
    [HorizontalGroup("队形与数量配置/CC", 160)]
    [LabelText("队形")]
    [LabelWidth(50)]
    public SpawnShape SpawnShpe = SpawnShape.Rectangle;

    [FoldoutGroup("队形与数量配置")]
    [HorizontalGroup("队形与数量配置/CC", 160)]
    [LabelText("时间间隔")]
    [LabelWidth(80)]
    [MinValue(0)]
    public float SpawnIntervalTime = 0;

    [FoldoutGroup("队形与数量配置")]
    [HorizontalGroup("队形与数量配置/CC", 160)]
    [LabelText("距离间隔")]
    [LabelWidth(80)]
    [MinValue(0)]
    public float SpawnSizeInterval = 0.8f;

    private void OnTypeIDChange()
    {
        if (AITypeID == 0)
            return;

        var aiCfg = DataManager.Instance.GetAIShipConfig(AITypeID);
        if(aiCfg != null)
        {
            AITypeName = LocalizationManager.Instance.GetTextValue(aiCfg.GeneralConfig.Name);
        }
    }

#if UNITY_EDITOR
    [OnInspectorInit]
    private void OnInit()
    {
        OnTypeIDChange();
    }
#endif
}

