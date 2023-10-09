using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 * ͨ��Ѫ������
 */
public class GeneralHPComponet 
{
    private ChangeValue<int> _currentHP;

    /// <summary>
    /// ��ǰHP
    /// </summary>
    public int GetCurrentHP
    {
        get { return _currentHP.Value; }
    }

    /// <summary>
    /// HP����
    /// </summary>
    public float HPPercent
    {
        get { return GetCurrentHP / (float)MaxHP; }
    }

    /// <summary>
    /// ���Ѫ��
    /// </summary>
    public int MaxHP
    {
        get;
        private set;
    }

    /// <summary>
    /// Param 1 = CurrentHP
    /// Param 2 = MaxHP
    /// Param 3 = HP Percent
    /// </summary>
    public Action<int, int, float> OnHpChangeAction;

    public GeneralHPComponet(int maxHP, int currentHP)
    {
        _currentHP = new ChangeValue<int>(currentHP, 0, maxHP);
        MaxHP = maxHP;
    }

    public void RecoverHPToMax()
    {
        _currentHP.Set(MaxHP);
    }

    /// <summary>
    /// �ı�Ѫ��
    /// </summary>
    /// <param name="value">�Ƿ�����</param>
    /// <returns></returns>
    public bool ChangeHP(int value)
    {
        var newValue = GetCurrentHP + value;
        _currentHP.Set(newValue);

        return newValue <= 0;
    }

    /// <summary>
    /// �������Ѫ��
    /// </summary>
    /// <param name="value"></param>
    public void SetMaxHP(int value)
    {
        _currentHP.SetMaxValue(value);
        OnHpChangeAction?.Invoke(_currentHP.Value, MaxHP, HPPercent);
    }

    public void BindHPChangeAction(Action<int, int> callback, bool trigger, int oldValue, int newValue)
    {
        _currentHP.BindChangeAction(callback);
        if (trigger)
        {
            callback?.Invoke(oldValue, newValue);
        }
    }

    public void UnBindHPChangeAction(Action callback)
    {
        _currentHP.UnBindChangeAction(callback);
    }

    public void UnBindHPChangeAction(Action<int , int> callback)
    {
        _currentHP.UnBindChangeAction(callback);
    }
}
