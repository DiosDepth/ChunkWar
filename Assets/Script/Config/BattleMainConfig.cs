using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Editor;

public class BattleMainConfig : SerializedScriptableObject
{
    public byte RogueShop_Origin_RefreshNum = 4;

    public byte ShipMaxLevel;

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
}

[System.Serializable]
[HideReferenceObjectPicker]
public class HardLevelConfig
{
    public int HardLevelID;
    public string Name;

    public List<WaveConfig> WaveConfig = new List<WaveConfig>();
}

[System.Serializable]
[HideReferenceObjectPicker]
public class WaveConfig
{
    public int WaveIndex;
    public ushort DurationTime;
}

[System.Serializable]
[HideReferenceObjectPicker]
public class PropertyDisplayConfig
{
    public string NameText;
    public Sprite Icon;
    public bool IsPercent;
}

[System.Serializable]
public class PropertyMidifyConfig
{
    [HorizontalGroup("B", 300)]
    [LabelText("ÐÞÕýKey")]
    [LabelWidth(100)]
    public PropertyModifyKey ModifyKey;

    [HorizontalGroup("B", 200)]
    [LabelText("Öµ")]
    [LabelWidth(80)]
    public float Value;
}
