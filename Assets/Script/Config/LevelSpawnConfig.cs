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
    [LabelText("�ؿ�Ԥ��ID")]
    [ReadOnly]
    public int LevelPresetID;

    [ListDrawerSettings(CustomAddFunction = "AddNewWave")]
    [Title("������Ϣ����", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("����")]
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
    [LabelText("����ID")]
    [LabelWidth(80)]
    [HorizontalGroup("AA", 200)]
    public int WaveIndex;

    [LabelText("����ʱ��")]
    [LabelWidth(80)]
    [HorizontalGroup("AA", 200)]
    public ushort DurationTime;

    [LabelText("�̴�ˢ�±�")]
    [LabelWidth(80)]
    [HorizontalGroup("AA", 200)]
    public ushort[] ShopRefreshTimeMap = new ushort[0];

    [LabelText("���޵���")]
    [LabelWidth(80)]
    [HorizontalGroup("AA", 150)]
    public bool EndlessEnemy;

    [LabelText("���˲�������")]
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
/// ����ˢ������
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
    [LabelText("��λ����")]
    [LabelWidth(80)]
    public AvaliableAIType AIType = AvaliableAIType.AI_Flyings;

    [HorizontalGroup("BB", 160)]
    [LabelText("ѭ�����")]
    [LabelWidth(60)]
    public int DurationDelta;

    [HorizontalGroup("BB", 160)]
    [LabelText("ѭ������")]
    [LabelWidth(60)]
    public int LoopCount;

    [HorizontalGroup("BB", 160)]
    [LabelText("��ʼʱ�䣨�룩")]
    [LabelWidth(100)]
    public int StartTime;

    [HorizontalGroup("CC", 160)]
    [LabelText("��������")]
    [LabelWidth(80)]
    [MinValue(1)]
    public int TotalCount = 1;

    [HorizontalGroup("CC", 160)]
    [LabelText("ÿ�е���")]
    [LabelWidth(80)]
    [MinValue(1)]
    public int MaxRowCount = 1;

    [HorizontalGroup("CC", 160)]
    [LabelText("����")]
    [LabelWidth(50)]
    public Shape SpawnShpe = Shape.Rectangle;

    [HorizontalGroup("CC", 160)]
    [LabelText("���ɼ��")]
    [LabelWidth(80)]
    [MinValue(0)]
    public float SpawnIntervalTime = 0;
}