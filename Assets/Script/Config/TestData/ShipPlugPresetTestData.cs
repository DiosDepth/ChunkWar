using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if GMDEBUG
[CreateAssetMenu(fileName = "TestData_Plug_", menuName = "Configs/TestData_Plug")]
public class ShipPlugPresetTestData : SerializedScriptableObject
{
    public int ID;
    public string Name;
    public string Comment;

    [ValueDropdown("GetPlugs", IsUniqueList = true)]
    public List<PlugTestItem> PlugItems = new List<PlugTestItem>();

    private ValueDropdownList<PlugTestItem> dropDownLst;

    public class PlugTestItem
    {
        [HorizontalGroup("AA", 200)]
        [LabelWidth(50)]
        [LabelText("ID")]
        public int PlugID;

        [HorizontalGroup("AA", 200)]
        [LabelWidth(50)]
        [LabelText("Ãû³Æ")]
        [ReadOnly]
        public string Name;

        [HorizontalGroup("AA", 200)]
        [LabelWidth(50)]
        [LabelText("ÊýÁ¿")]
        public int count;
    }

#if UNITY_EDITOR
    [OnInspectorInit]
    private void Init()
    {
        LocalizationManager.Instance.SetLanguage(SystemLanguage.ChineseSimplified);
        DataManager.Instance.LoadShipPlugConfig_Editor();
        InitPlugs();

        for(int i =0;i< PlugItems.Count; i++)
        {
            var cfg = DataManager.Instance.GetShipPlugItemConfig(PlugItems[i].PlugID);
            if(cfg != null)
            {
                PlugItems[i].Name = LocalizationManager.Instance.GetTextValue(cfg.GeneralConfig.Name);
            }
            else
            {
                PlugItems[i].Name = string.Empty;
            }
            
        }
    }

#endif

    private void InitPlugs()
    {
        dropDownLst = new ValueDropdownList<PlugTestItem>();
        var allPlugs = DataManager.Instance.GetAllPlugs();
        for (int i = 0; i < allPlugs.Count; i++)
        {
            var plug = allPlugs[i];
            var name = string.Format("[{0}]-{1}", plug.ID, LocalizationManager.Instance.GetTextValue(plug.GeneralConfig.Name));
            dropDownLst.Add(name, new PlugTestItem()
            {
                count = 1,
                PlugID = plug.ID
            });
        }
    }

    private ValueDropdownList<PlugTestItem> GetPlugs()
    {
        return dropDownLst;
    }
}
#endif