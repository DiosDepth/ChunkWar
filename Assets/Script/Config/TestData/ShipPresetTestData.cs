using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if GMDEBUG
[CreateAssetMenu(fileName = "TestData_Ship_", menuName = "Configs/TestData_Ship")]
public class ShipPresetTestData : SerializedScriptableObject
{
    public int ID;
    public string Name;
    public string Comment;

    public int ShipID;
    public int ShipMainWeaponID;

    public Dictionary<PropertyModifyKey, float> PropertyRowAddValue = new Dictionary<PropertyModifyKey, float>();

    [ShowInInspector]
    [ReadOnly]
    private string UnitName;

    [LabelText("装备从存档导出")]
    [LabelWidth(150)]
    public bool ExportFromSave = false;
    [LabelText("装备配置")]
    public List<ShipInitUnitConfig> OriginUnits = new List<ShipInitUnitConfig>();

#if UNITY_EDITOR
    [OnInspectorInit]
    private void Init()
    {
        LocalizationManager.Instance.SetLanguage(SystemLanguage.ChineseSimplified);
        DataManager.Instance.LoadAllUnitConfig_Editor();
        for(int i = 0; i < OriginUnits.Count; i++)
        {
            var cfg = DataManager.Instance.GetUnitConfig(OriginUnits[i].UnitID);
            if(cfg != null)
            {
                UnitName += string.Format("{0};", LocalizationManager.Instance.GetTextValue(cfg.GeneralConfig.Name));
            }
        }
    }
#endif
}
#endif