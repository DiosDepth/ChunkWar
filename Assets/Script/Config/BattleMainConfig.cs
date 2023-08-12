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

    /// <summary>
    /// hardLevelÿX������1
    /// </summary>
    public ushort HardLevelDeltaSeconds = 120;
    public byte HardLevelWaveIndexMultiple = 20;

    public byte ShipMaxLevel;

    public byte[] EvolveRequireMap = new byte[3];
    public byte[] EvolveAddMap = new byte[4]; 

    [ListDrawerSettings(ShowIndexLabels = true, NumberOfItemsPerPage = 10)]
    public int[] EXPMap;

    public List<HardLevelConfig> HardLevels = new List<HardLevelConfig>();

    [DictionaryDrawerSettings()]
    public Dictionary<PropertyModifyKey, PropertyDisplayConfig> PropertyDisplay = new Dictionary<PropertyModifyKey, PropertyDisplayConfig>();

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
public class HardLevelConfig
{
    public int HardLevelID;
    public string Name;
    public string Desc;
    public Sprite Icon;

    [DictionaryDrawerSettings()]
    public Dictionary<HardLevelModifyType, float> ModifyDic = new Dictionary<HardLevelModifyType, float>();

    public List<WaveConfig> WaveConfig = new List<WaveConfig>();
}

[HideReferenceObjectPicker]
public class WaveConfig
{
    public int WaveIndex;
    public ushort DurationTime;

    [TableList]
    [LabelText("���˲�������")]
    public List<WaveEnemySpawnConfig> SpawnConfig = new List<WaveEnemySpawnConfig>();
}

/// <summary>
/// ����ˢ������
/// </summary>
public class WaveEnemySpawnConfig
{
    public int ID;
    public AvaliableAIType AIType;
    public int DurationDelta;
    public int LoopCount;
    public int StartTime;
    public int TotalCount;
    public int MaxRowCount;
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
    [LabelText("����Key")]
    [LabelWidth(100)]
    public PropertyModifyKey ModifyKey;

    [HorizontalGroup("B", 200)]
    [LabelText("ֵ")]
    [LabelWidth(80)]
    public float Value;
}
