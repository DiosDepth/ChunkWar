using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ShipPlugDataItemConfig : SerializedScriptableObject
{
    [HorizontalGroup("A", 80, Order = -200)]
    [LabelText("ID")]
    [LabelWidth(30)]
    public int ID;

    [FoldoutGroup("基础信息", -100)]
    [HideLabel()]
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
    [LabelText("效果描述")]
    [LabelWidth(80)]
    public bool EffectDescDamageParam = false;

    [FoldoutGroup("其他配置")]
    [LabelText("TAG")]
    [LabelWidth(80)]
    [EnumToggleButtons]
    public ShipPlugTag PlugTags;

    [FoldoutGroup("修正配置")]
    [HorizontalGroup("修正配置/B", 500)]
    [LabelText("属性百分比修改")]
    [LabelWidth(80)]
    [ListDrawerSettings(DraggableItems = false, CustomAddFunction = "AddPropertyMidifyConfig")]
    [HideReferenceObjectPicker]
    public PropertyModifyConfig[] PropertyPercentModify = new PropertyModifyConfig[0];

    [FoldoutGroup("触发配置")]
    [LabelText("触发")]
    [DisableContextMenu(DisableForMember = true, DisableForCollectionElements = true)]
    [ValueDropdown("GetTriggerLst", DrawDropdownForListElements = false)]
    [HideReferenceObjectPicker]
    public ModifyTriggerConfig[] ModifyTriggers = new ModifyTriggerConfig[0];

    public bool HasPlugTag(ShipPlugTag tag)
    {
        return PlugTags.HasFlag(tag);
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

                        ///图标TODO
                        var descRow = LocalizationManager.Instance.GetTextValue(EffectDesc);
                        var damageText = GetDamamgeValueDescText(damage.DamageModifyFrom, finalDamage, damage.Damage);
                        return LocalizationManager.Instance.ReplaceTextBySpecialValue(descRow, damageText);
                    }
                    else if (effect.EffectType == ModifyTriggerEffectType.CreateExplode)
                    {
                        MTEC_CreateExplode explde = effect as MTEC_CreateExplode;
                        var finalDamage = GameHelper.CalculatePlayerDamageWithModify(explde.ExplodeDamageBase, explde.DamageModifyFrom, out bool critical);

                        ///图标TODO
                        var descRow = LocalizationManager.Instance.GetTextValue(EffectDesc);
                        var damageText = GetDamamgeValueDescText(explde.DamageModifyFrom, finalDamage, explde.ExplodeDamageBase);
                        return LocalizationManager.Instance.ReplaceTextBySpecialValue(descRow, damageText);
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

    private string GetDamamgeValueDescText(List<UnitPropertyModifyFrom> modifyFroms, int damage, int rowDamage)
    {
        StringBuilder sb = new StringBuilder();
        string color = GameHelper.GetColorCode(damage, rowDamage, false);

        ///伤害图文混排
        sb.Append(string.Format("<color={0}>{1}</color> ", color, damage));

        if (modifyFroms != null && modifyFroms.Count > 0)
        {
            sb.Append("[");
            for (int i = 0; i < modifyFroms.Count; i++)
            {
                var modifyFrom = modifyFroms[i];
                var proeprtyDisplayCfg = DataManager.Instance.battleCfg.GetPropertyDisplayConfig(modifyFrom.PropertyKey);
                if (proeprtyDisplayCfg != null)
                {
                    if (modifyFrom.Ratio == 1)
                    {
                        sb.Append(string.Format("{0}%<sprite={1}>", modifyFrom.Ratio * 100, proeprtyDisplayCfg.TextSpriteIndex));
                    }
                    else
                    {
                        sb.Append(string.Format("<sprite={0}>", proeprtyDisplayCfg.TextSpriteIndex));
                    }
                }
            }
            sb.Append("]");
        }

        return sb.ToString();
    }


    private ValueDropdownList<ModifyTriggerConfig> GetTriggerLst()
    {
        return ModifyTriggerConfig.GetModifyTriggerList();
    }


    private PropertyModifyConfig AddPropertyMidifyConfig()
    {
        return new PropertyModifyConfig();
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
