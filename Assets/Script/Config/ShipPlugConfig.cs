using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShipPlugConfig : SerializedScriptableObject
{
    [ListDrawerSettings()]
    [LabelText("����б�")]
    public List<ShipPlugItemConfig> PlugConfigs = new List<ShipPlugItemConfig>();
}

[System.Serializable]
public class ShipPlugItemConfig
{
    [HorizontalGroup("A", 150)]
    [LabelText("ID")]
    [LabelWidth(80)]
    public int ID;

    [HorizontalGroup("A", 300)]
    [LabelText("��Ϣ����")]
    [LabelWidth(80)]
    public GeneralItemConfig GeneralConfig = new GeneralItemConfig();

    [HorizontalGroup("A", 800)]
    [LabelText("��������")]
    [LabelWidth(80)]
    [ListDrawerSettings(DraggableItems = false)]
    public List<PropertyMidifyConfig> PropertyModify = new List<PropertyMidifyConfig>();
}