using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ShipPlugConfig : SerializedScriptableObject
{
    [ListDrawerSettings( NumberOfItemsPerPage = 10, CustomAddFunction = "AddItem")]
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
    [LabelText("信息配置")]
    [LabelWidth(80)]
    [HideReferenceObjectPicker]
    public GeneralItemConfig GeneralConfig = new GeneralItemConfig();

    [FoldoutGroup("修正配置")]
    [HorizontalGroup("修正配置/B", 500)]
    [LabelText("属性修正")]
    [LabelWidth(80)]
    [ListDrawerSettings(DraggableItems = false, CustomAddFunction = "AddPropertyMidifyConfig")]
    [HideReferenceObjectPicker]
    public PropertyModifyConfig[] PropertyModify = new PropertyModifyConfig[0];

    [FoldoutGroup("其他配置")]
    [LabelText("效果描述")]
    [LabelWidth(80)]
    public string EffectDesc;

    [FoldoutGroup("其他配置")]
    [LabelText("TAG")]
    [LabelWidth(80)]
    [EnumToggleButtons]
    public ItemTag ItemTags;

    [FoldoutGroup("修正配置")]
    [HorizontalGroup("修正配置/B", 500)]
    [LabelText("属性百分比修改")]
    [LabelWidth(80)]
    [ListDrawerSettings(DraggableItems = false, CustomAddFunction = "AddPropertyMidifyConfig")]
    [HideReferenceObjectPicker]
    public PropertyModifyConfig[] PropertyPercentModify = new PropertyModifyConfig[0];

    [HorizontalGroup("A", 1000)]
    [LabelText("触发配置")]
    [LabelWidth(80)]
    [DisableContextMenu(DisableForMember = true, DisableForCollectionElements = true)]
    [ValueDropdown("GetTriggerLst", DrawDropdownForListElements = false)]
    [HideReferenceObjectPicker]
    public ModifyTriggerConfig[] ModifyTriggers = new ModifyTriggerConfig[0];

    public bool HasUnitTag(ItemTag tag)
    {
        return ItemTags.HasFlag(tag);
    }


    private ValueDropdownList<ModifyTriggerConfig> GetTriggerLst()
    {
        return ModifyTriggerConfig.GetModifyTriggerList();
    }


    private PropertyModifyConfig AddPropertyMidifyConfig()
    {
        return new PropertyModifyConfig();
    }
}