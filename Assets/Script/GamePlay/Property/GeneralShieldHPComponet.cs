using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralShieldHPComponet 
{
    private ChangeValue<int> _currentShieldHP;

    /// <summary>
    /// 当前HP
    /// </summary>
    public int GetCurrentShieldHP
    {
        get { return _currentShieldHP.Value; }
    }

    /// <summary>
    /// HP比例
    /// </summary>
    public float ShieldHPPercent
    {
        get { return GetCurrentShieldHP / (float)MaxShieldHP; }
    }

    /// <summary>
    /// 最大血量
    /// </summary>
    public int MaxShieldHP
    {
        get;
        private set;
    }

    public GeneralShieldHPComponet(int maxHP, int currentHP)
    {
        _currentShieldHP = new ChangeValue<int>(currentHP, 0, maxHP);
        MaxShieldHP = maxHP;
    }

    public void OnUpdate()
    {

    }

    /// <summary>
    /// 改变血量
    /// </summary>
    /// <param name="value">是否死亡</param>
    /// <returns></returns>
    public void TakeDamage(WeaponDamageType damageType, ref int value)
    {
        var newValue = GetCurrentShieldHP + value;
        _currentShieldHP.Set(newValue);
    }

    /// <summary>
    /// 设置最大血量
    /// </summary>
    /// <param name="value"></param>
    public void SetMaxHP(int value)
    {
        _currentShieldHP.SetMaxValue(value);
    }

    public void BindHPChangeAction(Action callback, bool trigger)
    {
        _currentShieldHP.BindChangeAction(callback);
        if (trigger)
        {
            callback?.Invoke();
        }
    }

    public void UnBindHPChangeAction(Action callback)
    {
        _currentShieldHP.UnBindChangeAction(callback);
    }
}
