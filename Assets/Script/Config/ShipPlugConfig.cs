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
    [ListDrawerSettings( NumberOfItemsPerPage = 15, CustomAddFunction = "AddItem")]
    [HideReferenceObjectPicker]
    [LabelText("����б�")]
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

    private ShipPlugItemConfig AddItem()
    {
        return new ShipPlugItemConfig();
    }
}


public class ShipPlugItemConfig
{
    [HorizontalGroup("A", 80)]
    [LabelText("ID")]
    [LabelWidth(30)]
    public int ID;

    [HorizontalGroup("A", 400)]
    [LabelText("��Ϣ����")]
    [LabelWidth(80)]
    [HideReferenceObjectPicker]
    public GeneralItemConfig GeneralConfig = new GeneralItemConfig();

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/B", 500)]
    [LabelText("��������")]
    [LabelWidth(80)]
    [ListDrawerSettings(DraggableItems = false, CustomAddFunction = "AddPropertyMidifyConfig")]
    [HideReferenceObjectPicker]
    public PropertyModifyConfig[] PropertyModify = new PropertyModifyConfig[0];

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/B", 500)]
    [LabelText("���԰ٷֱ��޸�")]
    [LabelWidth(80)]
    [ListDrawerSettings(DraggableItems = false, CustomAddFunction = "AddPropertyMidifyConfig")]
    [HideReferenceObjectPicker]
    public PropertyModifyConfig[] PropertyPercentModify = new PropertyModifyConfig[0];

    [HorizontalGroup("A", 1000)]
    [LabelText("��������")]
    [LabelWidth(80)]
    [DisableContextMenu(DisableForMember = true, DisableForCollectionElements = true)]
    [ValueDropdown("GetTriggerLst", DrawDropdownForListElements = false)]
    [HideReferenceObjectPicker]
    public ModifyTriggerConfig[] ModifyTriggers = new ModifyTriggerConfig[0];


    private ValueDropdownList<ModifyTriggerConfig> GetTriggerLst()
    {
        return ModifyTriggerConfig.GetModifyTriggerList();
    }


    private PropertyModifyConfig AddPropertyMidifyConfig()
    {
        return new PropertyModifyConfig();
    }
}