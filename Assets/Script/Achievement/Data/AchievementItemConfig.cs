using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

public enum AchievementConLstType
{
    And,
    Or
}

public enum AchievementGroupType
{
    NONE,
    Normal,
    Battle,
    Economy
}

public class AchievementItemConfig : SerializedScriptableObject, IComparable<AchievementItemConfig>
{
    [LabelText("成就ID")]
    [LabelWidth(80)]
    [ReadOnly]
    [HorizontalGroup("AA", 200)]
    public int AchievementID;

    [LabelText("成就名")]
    [LabelWidth(80)]
    [ReadOnly]
    [HorizontalGroup("AA", 300)]
    public string AchievementName;


    [LabelText("描述")]
    [LabelWidth(80)]
    [ReadOnly]
    [HorizontalGroup("AA", 300)]
    public string AchievementDesc;

    [HideLabel]
    [ShowInInspector]
    [ReadOnly]
    private string Name;

    [HideLabel]
    [ShowInInspector]
    [ReadOnly]
    private string Desc;

    [PreviewField(200, Alignment = ObjectFieldAlignment.Left)]
    public Sprite Icon;

    [LabelText("排序")]
    [LabelWidth(80)]
    [HorizontalGroup("AB", 300)]
    public int Order;

    [LabelText("分组")]
    [LabelWidth(80)]
    [HorizontalGroup("AB", 300)]
    public AchievementGroupType GroupType;

    public AchievementConLstType ListType;

    public List<AchievementConditionConfig> Conditions = new List<AchievementConditionConfig>();

#if UNITY_EDITOR
    [OnInspectorInit]
    private void OnInit()
    {
        if (string.IsNullOrEmpty(AchievementName))
        {
            AchievementName = string.Format("AchievementName_{0}", AchievementID);
        }
        else
        {
            Name = LocalizationManager.Instance.GetTextValue(AchievementName);
        }

        if (string.IsNullOrEmpty(AchievementDesc))
        {
            AchievementDesc = string.Format("AchievementDesc_{0}", AchievementID);
        }
        else
        {
            Desc = LocalizationManager.Instance.GetTextValue(AchievementDesc);
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

#endif
    public int CompareTo(AchievementItemConfig other)
    {
        var unlockSelf = SaveLoadManager.Instance.GetAchievementUnlockState(this.AchievementID);
        var unlockOther = SaveLoadManager.Instance.GetAchievementUnlockState(other.AchievementID);
        return unlockSelf.CompareTo(unlockOther);
    }
}

[HideReferenceObjectPicker]
[System.Serializable]
public class AchievementConditionConfig
{
    public string AchievementKey;

    public bool DisplayProgress = true; 
    public CompareType Compare;
    public int Value;

    /// <summary>
    /// Bool类型
    /// </summary>
    public bool CheckBoolValue = false;
}
