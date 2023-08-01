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

    public GeneralHPComponet(int maxHP, int currentHP)
    {
        _currentHP = new ChangeValue<int>(currentHP, 0, maxHP);
        MaxHP = maxHP;
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
    }

    public void BindHPChangeAction(Action callback, bool trigger)
    {
        _currentHP.BindChangeAction(callback);
        if (trigger)
        {
            callback?.Invoke();
        }
    }

    public void UnBindHPChangeAction(Action callback)
    {
        _currentHP.UnBindChangeAction(callback);
    }
}
