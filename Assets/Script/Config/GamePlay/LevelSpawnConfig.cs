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
        [LabelText("����ID")]
        public int WaveIndex;
        [LabelText("���˾�ӢID")]
        public int[] ExceptEliteIDs = new int[0];
        [LabelText("����ʱ���б�")]
        public ushort[] EliteSpawnTimeList = new ushort[0];
    }

    [System.Serializable]
    public class BossSpawnConfig
    {
        [LabelText("�������һ��")]
        [LabelWidth(150)]
        public bool RandomOne = true;

        [LabelText("���һ������ʱ��")]
        [LabelWidth(150)]
        public ushort OneSpawnStartTime;

        [LabelText("ʱ���б�")]
        public ushort[] BOSSSpawnTimeList = new ushort[0];

        [LabelText("˳��BOSSID�б�")]
        public int[] BOSSSpawnIDs = new int[0];
    }

    [LabelText("�ؿ�Ԥ��ID")]
    [ReadOnly]
    public int LevelPresetID;

    public string BGMEvent;

    public string LevelName;

    [FoldoutGroup("��������������")]
    [HorizontalGroup("��������������/A", 300)]
    [LabelText("���þ�Ӣ����")]
    [LabelWidth(100)]
    public bool EnableSpecialEnemySpawn = false;

    [FoldoutGroup("��������������")]
    [HorizontalGroup("��������������/A")]
    [LabelText("��Ӣ�ۼ����ɵĲ�����")]
    [LabelWidth(150)]
    public byte TotalSpawnWaveCount = 0;

    [FoldoutGroup("��������������")]
    [LabelText("��Ӣ��������")]
    [LabelWidth(100)]
    public List<EliteRandomSpawnConfig> EliteRandomSpawnCfg = new List<EliteRandomSpawnConfig>();

    [FoldoutGroup("��������������")]
    [LabelText("BOSS��������")]
    [LabelWidth(100)]
    public BossSpawnConfig BossConfig;

    [ListDrawerSettings(CustomAddFunction = "AddNewWave", NumberOfItemsPerPage = 3)]
    [Title("������Ϣ����", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("����")]
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

    [FoldoutGroup("Ԥ��")]
    [Button("������ϢԤ��")]
    private void Refresh()
    {
        WaveThreadLog = new List<WaveThread_LOG>();
        for(int i = 0; i < WaveConfig.Count; i++)
        {
            var log = WaveConfig[i].GetWaveThread_LOG();
            WaveThreadLog.Add(log);
        }
    }


    [ShowInInspector]
    [FoldoutGroup("Ԥ��")]
    [ListDrawerSettings(IsReadOnly = true, ShowFoldout = true)]
    private List<WaveThread_LOG> WaveThreadLog;

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

    [LabelText("���λ���")]
    [LabelWidth(80)]
    [HorizontalGroup("AA", 200)]
    public int waveScore;

#if GMDEBUG && UNITY_EDITOR

    [FoldoutGroup("��Ϣ")]
    [HorizontalGroup("��Ϣ/A")]
    [Button("ˢ����Ϣ"), GUIColor(0,1,0)]
    private void RefreshInfo()
    {
        float totalThread = 0;
        for(int i = 0; i < SpawnConfig.Count; i++)
        {
            var cfg = SpawnConfig[i];
            var shipCfg = DataManager.Instance.GetAIShipConfig(cfg.AITypeID);
            if (shipCfg == null)
                continue;

            if (cfg.LoopCount <= 0)
            {
                ///Loop
                int totalLoop = Mathf.RoundToInt((DurationTime - cfg.StartTime) / cfg.DurationDelta);
                totalThread += totalLoop * cfg.TotalCount * shipCfg.SectorThreadValue;
            }
            else
            {
                for(int j = 0; j < cfg.LoopCount; j++)
                {
                    var time = cfg.LoopCount * cfg.DurationDelta + cfg.StartTime;
                    if(time < DurationTime)
                    {
                        totalThread += cfg.TotalCount * shipCfg.SectorThreadValue;
                    }
                }
            }
        }

        TotalThread = totalThread;
        AVG_TotalThread = TotalThread / DurationTime;
    }

    [FoldoutGroup("��Ϣ")]
    [HorizontalGroup("��Ϣ/A")]
    [Button("Ԥ��"), GUIColor(0, 1, 0)]
    private void Preview()
    {
        if (!EditorApplication.isPlaying) 
        {
            return;
        }
        UIManager.Instance.ShowUI<LevelTimelinePreviewPage>("LevelTimelinePreviewPage", E_UI_Layer.Mid, null, (panel) =>
        {
            panel.Initialization(this);
        });
    }

    [FoldoutGroup("��Ϣ")]
    [HorizontalGroup("��Ϣ/A")]
    [LabelText("�ܼ���вֵ")]
    [ReadOnly]
    [ShowInInspector]
    private float TotalThread;

    [FoldoutGroup("��Ϣ")]
    [HorizontalGroup("��Ϣ/A")]
    [LabelText("ƽ����вֵ")]
    [ReadOnly]
    [ShowInInspector]
    private float AVG_TotalThread;

    public WaveThread_LOG GetWaveThread_LOG()
    {
        RefreshInfo();
        WaveThread_LOG log = new WaveThread_LOG();
        log.WaveIndex = WaveIndex;
        log.TotalThread = TotalThread;
        log.AVG_TotalThread = AVG_TotalThread;
        return log;
    }

#endif

    [LabelText("���˲�������")]
    [ListDrawerSettings(CustomAddFunction = "AddNewWaveSpawn")]
    [OnCollectionChanged("OnAddedWave")]
    public List<WaveEnemySpawnConfig> SpawnConfig = new List<WaveEnemySpawnConfig>();

    [FoldoutGroup("����Ȩ������")]
    [LabelText("����Ȩ������")]
    public SectorThreadSortType SectorSortType = SectorThreadSortType.Balance;

    [FoldoutGroup("����Ȩ������")]
    [LabelText("����Ȩ�ظ���ʱ���")]
    public int[] SectorAdjacentUpdateTime = new int[1] { 0 };

    [FoldoutGroup("����Ȩ������")]
    [LabelText("����Ȩ�ظ���ֵ")]
    public byte[] SectorAdjacentWeightValue = new byte[1] { 0 };

    [FoldoutGroup("�Զ���������")]
    [LabelText("��ʯ����")]
    public bool GenerateMeteorite = true;

    /// <summary>
    /// ��ʯ����ϡ�ж�Ȩ��
    /// </summary>
    [FoldoutGroup("�Զ���������")]
    [LabelText("��ʯ��������")]
    [ShowIf("GenerateMeteorite")]
    public Dictionary<GoodsItemRarity, byte> MeteoriteGenerateRate_RarityMap = new Dictionary<GoodsItemRarity, byte>()
    {
        { GoodsItemRarity.Tier1, 0 },
        { GoodsItemRarity.Tier2, 0 },
        { GoodsItemRarity.Tier3, 0 },
        { GoodsItemRarity.Tier4, 0 },
    };

    [FoldoutGroup("�Զ���������")]
    [HorizontalGroup("�Զ���������/A")]
    [LabelText("Զ�ŷɴ�����")]
    [LabelWidth(100)]
    public bool UseAncientUnitSpawnProtect = false;

    [FoldoutGroup("�Զ���������")]
    [HorizontalGroup("�Զ���������/A")]
    [LabelText("Զ�ŷɴ�������")]
    [LabelWidth(100)]
    public int AncientUnitSpawnProtectedCount;

    [FoldoutGroup("�Զ���������")]
    [HorizontalGroup("�Զ���������/A")]
    [LabelText("Զ�ŷɴ��������")]
    [LabelWidth(100)]
    public int AncientUnitSpawnMax;

    [FoldoutGroup("�Զ���������")]
    [HorizontalGroup("�Զ���������/A")]
    [LabelText("����Զ�ŷɴ����")]
    [LabelWidth(100)]
    public bool OverrideAncientUnitSpawnSoomthTimeDelta = false;

    [FoldoutGroup("�Զ���������")]
    [HorizontalGroup("�Զ���������/A")]
    [LabelText("�������")]
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

    [HorizontalGroup("BB", 150)]
    [LabelText("��λ����ID")]
    [LabelWidth(80)]
    [OnValueChanged("OnTypeIDChange")]
    [DelayedProperty]
    public int AITypeID;

    [HorizontalGroup("BB", 200)]
    [ShowInInspector]
    [LabelText("AI��")]
    [LabelWidth(50)]
    [ReadOnly]
    private string AITypeName;

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

    [FoldoutGroup("��������������")]
    [HorizontalGroup("��������������/CC", 160)]
    [LabelText("��������")]
    [LabelWidth(80)]
    [MinValue(1)]
    public int TotalCount = 1;

    [FoldoutGroup("��������������")]
    [HorizontalGroup("��������������/CC", 160)]
    [LabelText("ÿ�е���")]
    [LabelWidth(80)]
    [MinValue(1)]
    public int MaxRowCount = 1;

    [FoldoutGroup("��������������")]
    [HorizontalGroup("��������������/CC", 160)]
    [LabelText("����")]
    [LabelWidth(50)]
    public SpawnShape SpawnShpe = SpawnShape.Rectangle;

    [FoldoutGroup("��������������")]
    [HorizontalGroup("��������������/CC", 160)]
    [LabelText("ʱ����")]
    [LabelWidth(80)]
    [MinValue(0)]
    public float SpawnIntervalTime = 0;

    [FoldoutGroup("��������������")]
    [HorizontalGroup("��������������/CC", 160)]
    [LabelText("������")]
    [LabelWidth(80)]
    [MinValue(0)]
    public float SpawnSizeInterval = 0.8f;

    [FoldoutGroup("��������������")]
    [HorizontalGroup("��������������/CC", 160)]
    [LabelText("����������")]
    [LabelWidth(80)]
    public bool OverrideDistanceMax = false;

    [FoldoutGroup("��������������")]
    [HorizontalGroup("��������������/CC", 160)]
    [LabelText("������")]
    [LabelWidth(80)]
    [ShowIf("OverrideDistanceMax")]
    public float DistanceMax = 45;

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

    [HorizontalGroup("BB", 450)]
    [LabelText("Ԥ��ɫ")]
    [LabelWidth(60)]
    [ColorPalette("Preview")]
    public Color EditorPreviewColor = Color.cyan;

    [OnInspectorInit]
    private void OnInit()
    {
        OnTypeIDChange();
    }
#endif
}

#if GMDEBUG

[System.Serializable]
[HideReferenceObjectPicker]
public class WaveThread_LOG
{
    [HorizontalGroup("��Ϣ",300)]
    [LabelText("Index")]
    [ReadOnly]
    [LabelWidth(50)]
    public int WaveIndex;

    [HorizontalGroup("��Ϣ")]
    [LabelText("�ܼ���вֵ")]
    [ReadOnly]
    public float TotalThread;

    [HorizontalGroup("��Ϣ")]
    [LabelText("ƽ����вֵ")]
    [ReadOnly]
    [ShowInInspector]
    public float AVG_TotalThread;
}

#endif