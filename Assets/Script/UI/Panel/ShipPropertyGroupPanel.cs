using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPropertyGroupPanel : MonoBehaviour
{
    public enum GroupType
    {
        Main,
        Sub
    }

    private static List<PropertyModifyKey> MainPropertyList = new List<PropertyModifyKey>
    {
         PropertyModifyKey.HP,
         PropertyModifyKey.DamagePercent,
         PropertyModifyKey.ShipSpeed,
         PropertyModifyKey.AttackSpeed,
         PropertyModifyKey.ShipArmor,
         PropertyModifyKey.Critical,
         PropertyModifyKey.PhysicsDamage,
         PropertyModifyKey.EnergyDamage,
         PropertyModifyKey.Command,
         PropertyModifyKey.WeaponRange,
         PropertyModifyKey.ShieldHP,
         PropertyModifyKey.ShieldArmor,
         PropertyModifyKey.Luck,
         PropertyModifyKey.ShipParry

    };

    private static List<PropertyModifyKey> SubPropertyList = new List<PropertyModifyKey>
    {
         PropertyModifyKey.Explode_Damage,
         PropertyModifyKey.Explode_Range

    };

    /// <summary>
    /// PropertyCmpt
    /// </summary>
    private ShipPropertyItemCmpt[] propertyCmpts;
    private ShipPropertyItemCmpt[] propertySubCmpts;

    private CanvasGroup _mainPropertyCanvas;
    private CanvasGroup _subPropertyCanvas;

    public GroupType CurrentGroupType = GroupType.Sub;

    private const string ShipPropertyCmpt_PrefabPath = "Prefab/GUIPrefab/CmptItems/ShipPropertyItem";


    public void Awake()
    {
        _mainPropertyCanvas = transform.Find("PropertyPanel").SafeGetComponent<CanvasGroup>();
        _subPropertyCanvas = transform.Find("PropertySubPanel").SafeGetComponent<CanvasGroup>();
        propertyCmpts = new ShipPropertyItemCmpt[MainPropertyList.Count];
        propertySubCmpts = new ShipPropertyItemCmpt[SubPropertyList.Count];
    }

    public void RefreshPropertyByKey(PropertyModifyKey key)
    {
        var cmpt = GetCmptByKey(key);
        if(cmpt != null)
        {
            cmpt.Refresh();
        }
    }

    public void SwitchGroupType()
    {
        InitPropertyCmpts();
        _mainPropertyCanvas.ActiveCanvasGroup(CurrentGroupType == GroupType.Main);
        _subPropertyCanvas.ActiveCanvasGroup(CurrentGroupType == GroupType.Sub);
    }

    private void InitPropertyCmpts()
    {
        for (int i = 0; i < MainPropertyList.Count; i++) 
        {
            PoolManager.Instance.GetObjectSync(ShipPropertyCmpt_PrefabPath, true, (obj) =>
            {
                var cmpt = obj.transform.SafeGetComponent<ShipPropertyItemCmpt>();
                cmpt.SetUp(MainPropertyList[i]);
                propertyCmpts[i] = cmpt;
            }, _mainPropertyCanvas.transform);
        }

        for (int i = 0; i < SubPropertyList.Count; i++)
        {
            PoolManager.Instance.GetObjectSync(ShipPropertyCmpt_PrefabPath, true, (obj) =>
            {
                var cmpt = obj.transform.SafeGetComponent<ShipPropertyItemCmpt>();
                cmpt.SetUp(SubPropertyList[i]);
                propertySubCmpts[i] = cmpt;
            }, _subPropertyCanvas.transform);
        }
    }

    private ShipPropertyItemCmpt GetCmptByKey(PropertyModifyKey key)
    {
        for (int i = 0; i < propertyCmpts.Length; i++) 
        {
            if (propertyCmpts[i].PropertyKey == key)
                return propertyCmpts[i];
        }

        for (int i = 0; i < propertySubCmpts.Length; i++)
        {
            if (propertySubCmpts[i].PropertyKey == key)
                return propertySubCmpts[i];
        }
        return null;
    }
}
