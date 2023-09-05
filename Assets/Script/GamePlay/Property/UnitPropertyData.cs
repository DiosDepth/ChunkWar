using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PropertyModifyType
{
    Row,
    Modify,
    ModifyPercent,
    TempModify
}

public class UnitPropertyData 
{
    private Dictionary<PropertyModifyKey, UnitPropertyPool> PropertyPool = new Dictionary<PropertyModifyKey, UnitPropertyPool>();

    public void Clear()
    {
        PropertyPool.Clear();
    }

    /// <summary>
    /// ��ChangeAction
    /// </summary>
    /// <param name="key"></param>
    /// <param name="changeAction"></param>
    public void BindPropertyChangeAction(PropertyModifyKey key, Action changeAction)
    {
        var pool = GetOrCreatePropertyPool(key);
        pool.BindChangeAction(changeAction);
    }

    public void UnBindPropertyChangeAction(PropertyModifyKey key, Action changeAction)
    {
        var pool = GetOrCreatePropertyPool(key);
        pool.UnBindChangeAction(changeAction);
    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="propertyKey"></param>
    /// <param name="fromUID"></param>
    /// <param name="value"></param>
    public void AddPropertyModifyValue(PropertyModifyKey propertyKey, PropertyModifyType type, uint fromUID, float value)
    {
        var pool = GetOrCreatePropertyPool(propertyKey);
        if (pool == null)
            return;

        pool.AddToPool(type, fromUID, value);
    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="propertyKey"></param>
    /// <param name="fromUID"></param>
    /// <param name="value"></param>
    public void SetPropertyModifyValue(PropertyModifyKey propertyKey, PropertyModifyType type, uint fromUID, float value)
    {
        var pool = GetOrCreatePropertyPool(propertyKey);
        if (pool == null)
            return;

        pool.SetValue(type, fromUID, value);
        ShipPropertyEvent.Trigger(ShipPropertyEventType.MainPropertyValueChange, propertyKey);
    }

    /// <summary>
    /// �Ƴ�����
    /// </summary>
    /// <param name="propertyKey"></param>
    /// <param name="fromUID"></param>
    public void RemovePropertyModifyValue(PropertyModifyKey propertyKey, uint fromUID)
    {
        var pool = GetOrCreatePropertyPool(propertyKey);
        if (pool == null)
            return;

        pool.RemoveFromPool_Modify(fromUID);
        ShipPropertyEvent.Trigger(ShipPropertyEventType.MainPropertyValueChange, propertyKey);
    }

    /// <summary>
    /// ��ȡ��������ֵ
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public float GetPropertyFinal(PropertyModifyKey key)
    {
        var pool = GetOrCreatePropertyPool(key);
        return pool.GetFinialValue();
    }

    private UnitPropertyPool GetOrCreatePropertyPool(PropertyModifyKey propertyKey)
    {
        if (propertyKey == PropertyModifyKey.NONE)
            return null;

        if (PropertyPool.ContainsKey(propertyKey))
            return PropertyPool[propertyKey];

        UnitPropertyPool pool = new UnitPropertyPool(propertyKey);
        PropertyPool.Add(propertyKey, pool);
        return pool;
    }
}

public class UnitPropertyPool
{
    public PropertyModifyKey PropertyKey;

    private Hashtable _propertyTable_Modify;

    private Action propertychangeAction;

    /// <summary>
    /// ��ʼ����
    /// </summary>
    private ChangeValue<float> _propertyRow;

    /// <summary>
    /// ��ʱ����
    /// </summary>
    private ChangeValue<float> _propertyTemp;

    /// <summary>
    /// ����������
    /// E.G : �˺��޸�+100%
    /// </summary>
    private Hashtable _propertyTable_ModifyPercent;

    public UnitPropertyPool(PropertyModifyKey propertyKey)
    {
        this.PropertyKey = propertyKey;
        _propertyTable_Modify = new Hashtable();
        _propertyTable_ModifyPercent = new Hashtable();
        _propertyRow = new ChangeValue<float>(0, float.MinValue, float.MaxValue);
        _propertyTemp = new ChangeValue<float>(0, float.MinValue, float.MaxValue);
    }

    public void BindChangeAction(Action action)
    {
        propertychangeAction += action;
    }

    public void UnBindChangeAction(Action action)
    {
        propertychangeAction -= action;
    }

    public bool AddToPool(PropertyModifyType type, uint key, float value)
    {
        bool succ = false;
        switch (type)
        {
            case PropertyModifyType.Modify:
                succ = AddPropertyModifyValue(key, value);
                break;
            case PropertyModifyType.ModifyPercent:
                succ = AddPropertyModifyPercentValue(key, value);
                break;
            case PropertyModifyType.Row:
                succ = AddPropertyRowValue(value);
                break;
            case PropertyModifyType.TempModify:
                succ = AddPropertyTempValue(value);
                break;
        }
        if (succ)
        {
            propertychangeAction?.Invoke();
            ShipPropertyEvent.Trigger(ShipPropertyEventType.MainPropertyValueChange, PropertyKey);
        }
        
        return true;
    }

    /// <summary>
    /// ����ֵ
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetValue(PropertyModifyType type, uint key, float value)
    {
        switch (type)
        {
            case PropertyModifyType.Modify:
                if (_propertyTable_Modify.ContainsKey(key))
                {
                    _propertyTable_Modify[key] = value;
                }
                else
                {
                    _propertyTable_Modify.Add(key, value);
                }
                break;
            case PropertyModifyType.ModifyPercent:
                if (_propertyTable_ModifyPercent.ContainsKey(key))
                {
                    _propertyTable_ModifyPercent[key] = value;
                }
                else
                {
                    _propertyTable_ModifyPercent.Add(key, value);
                }
                break;
        }
        propertychangeAction?.Invoke();
        ShipPropertyEvent.Trigger(ShipPropertyEventType.MainPropertyValueChange, PropertyKey);
    }

    public void RemoveFromPool_Modify(uint key)
    {
        _propertyTable_Modify.Remove(key);
        propertychangeAction?.Invoke();
        ShipPropertyEvent.Trigger(ShipPropertyEventType.MainPropertyValueChange, PropertyKey);
    }

    public void RemoveFromPool_ModifyPercent(uint fromUID)
    {
        _propertyTable_ModifyPercent.Remove(fromUID);
        propertychangeAction?.Invoke();
        ShipPropertyEvent.Trigger(ShipPropertyEventType.MainPropertyValueChange, PropertyKey);
    }

    /// <summary>
    /// �Ƴ���ʱ��������
    /// </summary>
    public void ClearTempValue()
    {
        _propertyTemp.Set(0);
    }

    /// <summary>
    /// ��ȡ��������
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public float GetFinialValue()
    {
        ///Modify
        float result = 0;
        foreach (var value in _propertyTable_Modify.Values)
        {
            result += (float)value;
        }
        ///ModifyPercent
        var modifyPercent = GetPropertyModifyPercentValue();
        modifyPercent = Mathf.Clamp(modifyPercent, 0, float.MaxValue);

        return (_propertyRow.Value + result + _propertyTemp.Value) * (1 + modifyPercent / 100f);
    }

    /// <summary>
    /// ������������
    /// </summary>
    /// <param name="value"></param>
    private bool AddPropertyRowValue(float value)
    {
        var newValue = _propertyRow.Value + value;
        _propertyRow.Set(newValue);
        return true;
    }

    private bool AddPropertyTempValue(float value)
    {
        var newValue = _propertyTemp.Value + value;
        _propertyTemp.Set(newValue);
        return true;
    }

    private bool AddPropertyModifyValue(uint key, float value)
    {
        if (_propertyTable_Modify.ContainsKey(key))
            return false;

        _propertyTable_Modify.Add(key, value);
        return true;
    }

    private bool AddPropertyModifyPercentValue(uint key, float value)
    {
        if (_propertyTable_ModifyPercent.ContainsKey(key))
            return false;

        _propertyTable_ModifyPercent.Add(key, value);
        return true;
    }

    private float GetPropertyModifyPercentValue()
    {
        float result = 0;
        foreach (var value in _propertyTable_ModifyPercent.Values)
        {
            result += (float)value;
        }
        return result;
    }
}