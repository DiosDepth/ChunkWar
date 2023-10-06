using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ShipPlugConfig : SerializedScriptableObject
{
    [ListDrawerSettings( NumberOfItemsPerPage = 5, CustomAddFunction = "AddItem")]
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
    [LabelText("Ч������")]
    [LabelWidth(80)]
    public string EffectDesc;

    [FoldoutGroup("��������")]
    [LabelText("Ч������")]
    [LabelWidth(80)]
    public bool EffectDescDamageParam = false;

    [FoldoutGroup("��������")]
    [LabelText("TAG")]
    [LabelWidth(80)]
    [EnumToggleButtons]
    public ItemTag ItemTags;

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

    public bool HasUnitTag(ItemTag tag)
    {
        return ItemTags.HasFlag(tag);
    }

    public string GetEffectDesc()
    {
        if (EffectDescDamageParam)
        {
            ///Param
            for (int i = 0; i < ModifyTriggers.Length; i++)
            {

                var trigger = ModifyTriggers[i];
                for (int j = 0; j < trigger.Effects.Length; i++)
                {
                    var effect = trigger.Effects[i];
                    if (effect.EffectType == ModifyTriggerEffectType.CreateDamage)
                    {
                        MTEC_CreateDamage damage = effect as MTEC_CreateDamage;
                        var finalDamage = GameHelper.CalculatePlayerDamageWithModify(damage.Damage, damage.DamageModifyFrom, out bool critical);

                        ///ͼ��TODO
                        var descRow = LocalizationManager.Instance.GetTextValue(EffectDesc);
                        return LocalizationManager.Instance.ReplaceTextBySpecialValue(descRow, finalDamage.ToString());
                    }
                    else if (effect.EffectType == ModifyTriggerEffectType.CreateExplode)
                    {
                        MTEC_CreateExplode explde = effect as MTEC_CreateExplode;
                        var finalDamage = GameHelper.CalculatePlayerDamageWithModify(explde.ExplodeDamageBase, explde.DamageModifyFrom, out bool critical);

                        ///ͼ��TODO
                        var descRow = LocalizationManager.Instance.GetTextValue(EffectDesc);
                        return LocalizationManager.Instance.ReplaceTextBySpecialValue(descRow, finalDamage.ToString());
                    }
                }
            }
            return LocalizationManager.Instance.GetTextValue(EffectDesc);
        }
        else
        {
            return LocalizationManager.Instance.GetTextValue(EffectDesc);
        }
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