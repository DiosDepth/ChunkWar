using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum PropertyModifyCategory
{
    ShipPlug,
    ModifyTrigger,
    Wreckage,
    ShipUnit,
    AIShipSkill,
    AIShipUnit,
}

public class ModifyUIDManager : Singleton<ModifyUIDManager>
{
    private Dictionary<uint, IPropertyModify> _modifyDic;

    private const uint UID_Sep = 10000000;

    public ModifyUIDManager()
    {
        _modifyDic = new Dictionary<uint, IPropertyModify>();
    }

    public void ClearAll()
    {
        _modifyDic.Clear();
    }

    /// <summary>
    /// ��ȡUID
    /// </summary>
    /// <param name="cate"></param>
    /// <returns></returns>
    public uint GetUID(PropertyModifyCategory cate, IPropertyModify item)
    {
        var sep = GetSep(cate);
        uint uid = (uint)UnityEngine.Random.Range(sep[0], sep[1]);
        if (_modifyDic.ContainsKey(uid))
            return GetUID(cate, item);

        _modifyDic.Add(uid, item);

        return uid;
    }

    public IPropertyModify GetModifyInterface(uint uid)
    {
        if (_modifyDic.ContainsKey(uid))
            return _modifyDic[uid];

        return null;
    }

    public PropertyModifyCategory GetCategoryByUID(uint uid)
    {
        foreach(PropertyModifyCategory cate in System.Enum.GetValues(typeof(PropertyModifyCategory)))
        {
            var sep = GetSep(cate);
            if (uid >= sep[0] && uid < sep[1])
                return cate;
        }
        return PropertyModifyCategory.ModifyTrigger;
    }

    public void RemoveUID(uint uid)
    {
        _modifyDic.Remove(uid);
    }

    /// <summary>
    /// �غϽ������
    /// </summary>
    public void ClearBattle()
    {
        ClearCategory(PropertyModifyCategory.AIShipSkill);
        ClearCategory(PropertyModifyCategory.AIShipUnit);
    }

    public void ClearCategory(PropertyModifyCategory cate)
    {
        var sep = GetSep(cate);
        var allItems = _modifyDic.Keys.ToArray();
        for (int i = 0; i < allItems.Length; i++) 
        {
            var item = allItems[i];
            if (item >= sep[0] && item < sep[1])
                _modifyDic.Remove(item);
        }
    }

    private uint[] GetSep(PropertyModifyCategory cate)
    {
        uint[] result = new uint[2];
        result[0] = (uint)(cate + 1) * UID_Sep;
        result[1] = (uint)(cate + 2) * UID_Sep;
        return result;
    }
}

public interface IPropertyModify
{
    public uint UID { get; set; }
    public PropertyModifyCategory Category { get; }

    public string GetName { get;  }

}
