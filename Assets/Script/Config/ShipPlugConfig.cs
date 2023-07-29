using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShipPlugConfig : SerializedScriptableObject
{
    [ListDrawerSettings()]
    [LabelText("插件列表")]
    public List<ShipPlugItemConfig> PlugConfigs = new List<ShipPlugItemConfig>();
}

[System.Serializable]
public class ShipPlugItemConfig
{
    [HorizontalGroup("A", 200)]
    [LabelText("ID")]
    [LabelWidth(80)]
    public int ID;

    [HorizontalGroup("A", 800)]
    [LabelText("属性修正")]
    [LabelWidth(80)]
    [ListDrawerSettings(DraggableItems = false)]
    public List<PropertyMidifyConfig> PropertyModify = new List<PropertyMidifyConfig>();
}