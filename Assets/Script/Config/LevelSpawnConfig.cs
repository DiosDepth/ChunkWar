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
    [LabelText("关卡预设ID")]
    [ReadOnly]
    public int LevelPresetID;

    [ListDrawerSettings(CustomAddFunction = "AddNewWave")]
    [Title("波次信息配置", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("波次")]
    public List<WaveConfig> WaveConfig = new List<WaveConfig>();

    private WaveConfig AddNewWave()
    {
        return new WaveConfig();
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

    [LabelText("敌人波次配置")]
    [ListDrawerSettings(CustomAddFunction = "AddNewWaveSpawn")]
    [OnCollectionChanged("OnAddedWave")]
    public List<WaveEnemySpawnConfig> SpawnConfig = new List<WaveEnemySpawnConfig>();

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

    [HorizontalGroup("BB", 200)]
    [LabelText("单位类型")]
    [LabelWidth(80)]
    public AvaliableAIType AIType = AvaliableAIType.AI_Flyings;

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

    [HorizontalGroup("CC", 160)]
    [LabelText("敌人数量")]
    [LabelWidth(80)]
    [MinValue(1)]
    public int TotalCount = 1;

    [HorizontalGroup("CC", 160)]
    [LabelText("每行敌人")]
    [LabelWidth(80)]
    [MinValue(1)]
    public int MaxRowCount = 1;

    [HorizontalGroup("CC", 160)]
    [LabelText("队形")]
    [LabelWidth(50)]
    public Shape SpawnShpe = Shape.Rectangle;

    [HorizontalGroup("CC", 160)]
    [LabelText("生成间隔")]
    [LabelWidth(80)]
    [MinValue(0)]
    public float SpawnIntervalTime = 0;
}