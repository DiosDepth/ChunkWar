using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ShipPlugConfig : SerializedScriptableObject
{
    [ListDrawerSettings( NumberOfItemsPerPage = 15)]
    [HideReferenceObjectPicker]
    [LabelText("插件列表")]
    public List<ShipPlugItemConfig> PlugConfigs = new List<ShipPlugItemConfig>();

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


public class ShipPlugItemConfig
{
    [HorizontalGroup("A", 70)]
    [LabelText("ID")]
    [LabelWidth(30)]
    public int ID;

    [HorizontalGroup("A", 400)]
    [LabelText("信息配置")]
    [LabelWidth(80)]
    [HideReferenceObjectPicker]
    public GeneralItemConfig GeneralConfig = new GeneralItemConfig();

    [FoldoutGroup("修正配置")]
    [HorizontalGroup("修正配置/B", 500)]
    [LabelText("属性修正")]
    [LabelWidth(80)]
    [ListDrawerSettings(DraggableItems = false)]
    [HideReferenceObjectPicker]
    public List<PropertyMidifyConfig> PropertyModify = new List<PropertyMidifyConfig>();

    [FoldoutGroup("修正配置")]
    [HorizontalGroup("修正配置/B", 500)]
    [LabelText("属性百分比修改")]
    [LabelWidth(80)]
    [ListDrawerSettings(DraggableItems = false)]
    [HideReferenceObjectPicker]
    public List<PropertyMidifyConfig> PropertyPercentModify = new List<PropertyMidifyConfig>();

    [HorizontalGroup("A", 600)]
    [LabelText("触发配置")]
    [LabelWidth(80)]
    [DisableContextMenu(DisableForMember = true, DisableForCollectionElements = true)]
    [ValueDropdown("GetTriggerLst", DrawDropdownForListElements = false)]
    [OnCollectionChanged("Change")]
    [HideReferenceObjectPicker]
    public List<ModifyTriggerConfig> ModifyTriggers = new List<ModifyTriggerConfig>();


    private ValueDropdownList<ModifyTriggerConfig> GetTriggerLst()
    {
        return ModifyTriggerConfig.GetModifyTriggerList();
    }

    private void Change(CollectionChangeInfo info, object lst)
    {
        if (info.ChangeType == CollectionChangeType.Add)
        {

        }
    }
}