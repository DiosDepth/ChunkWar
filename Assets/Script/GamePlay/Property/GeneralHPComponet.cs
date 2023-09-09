using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 * 通用血量计算
 */
public class GeneralHPComponet 
{
    private ChangeValue<int> _currentHP;

    /// <summary>
    /// 当前HP
    /// </summary>
    public int GetCurrentHP
    {
        get { return _currentHP.Value; }
    }

    /// <summary>
    /// HP比例
    /// </summary>
    public float HPPercent
    {
        get { return GetCurrentHP / (float)MaxHP; }
    }

    /// <summary>
    /// 最大血量
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

    /// <summary>
    /// 改变血量
    /// </summary>
    /// <param name="value">是否死亡</param>
    /// <returns></returns>
    public bool ChangeHP(int value)
    {
        var newValue = GetCurrentHP + value;
        _currentHP.Set(newValue);

        return newValue <= 0;
    }

    /// <summary>
    /// 设置最大血量
    /// </summary>
    /// <param name="value"></param>
    public void SetMaxHP(int value)
    {
        _currentHP.SetMaxValue(value);
        OnHpChangeAction?.Invoke(_currentHP.Value, MaxHP, HPPercent);
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
