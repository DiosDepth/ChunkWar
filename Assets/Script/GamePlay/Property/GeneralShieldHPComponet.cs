using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralShieldHPComponet
{
    public int ShieldRecoverValue
    {
        get;
        protected set;
    }

    /// <summary>
    /// 多久不受击恢复
    /// </summary>
    public float ShieldRecoverTime
    {
        get;
        protected set;
    }
    

    private int _shieldRecoverValueBase;
    private int _shieldHPBase;
    private float _shieldRecoverCDBase;
    private UnitPropertyData mainProperty;


    private ChangeValue<int> _currentShieldHP;

    /// <summary>
    /// 护盾是否损坏
    /// </summary>
    public bool IsShieldBroken
    {
        get
        {
            return GetCurrentShieldHP <= 0;
        }
    }

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

    /// <summary>
    /// 护盾受击回复Timer
    /// </summary>
    private float _shieldRecoverTimer = 0;
    /// <summary>
    /// 是否正在恢复护盾
    /// </summary>
    private bool _shieldRecovering = false;
    /// <summary>
    /// 恢复CD
    /// </summary>
    private float _recoverDeltaTimer = 0;
    private float _recoverDeltaTime;

    public GeneralShieldHPComponet(BuildingConfig cfg)
    {
        _shieldRecoverValueBase = cfg.ShieldRecoverValue;
        _shieldRecoverCDBase = cfg.ShieldRecoverTime;
        _shieldHPBase = cfg.ShieldHP;

        _recoverDeltaTime = DataManager.Instance.battleCfg.ShieldRecoverCD;
        mainProperty = RogueManager.Instance.MainPropertyData;

        mainProperty.BindPropertyChangeAction(PropertyModifyKey.ShieldHP, CalculateMaxShieldHP);
        mainProperty.BindPropertyChangeAction(PropertyModifyKey.ShieldRecoverValue, CalculateShieldRecoverValue);
        mainProperty.BindPropertyChangeAction(PropertyModifyKey.ShieldRecoverTimeReduce, CalculateShieldRecoverTime);

        CalculateMaxShieldHP();
        CalculateShieldRecoverValue();
        CalculateShieldRecoverTime();
        _currentShieldHP = new ChangeValue<int>(MaxShieldHP, 0, MaxShieldHP);
    }

    public void OnUpdate()
    {
        ///护盾满则不恢复
        if (GetCurrentShieldHP >= MaxShieldHP)
            return;

        if (!_shieldRecovering)
        {
            _shieldRecoverTimer += Time.deltaTime;
            if (_shieldRecoverTimer >= ShieldRecoverTime)
            {
                _shieldRecovering = true;
            }
        }

        if (_shieldRecovering)
        {
            ///RecoverTimer
            _recoverDeltaTimer += Time.deltaTime;
            if(_recoverDeltaTimer >= _recoverDeltaTime)
            {
                _recoverDeltaTimer = 0;
                ShieldRecover();
            }
        }
    }

    public void OnDestroy()
    {
        mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.ShieldHP, CalculateMaxShieldHP);
        mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.ShieldRecoverValue, CalculateShieldRecoverValue);
        mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.ShieldRecoverTimeReduce, CalculateShieldRecoverTime);
    }

    /// <summary>
    /// 改变血量
    /// </summary>
    /// <param name="damageValue">是否死亡</param>
    /// <returns></returns>
    public bool TakeDamage(int damageValue)
    {
        var newValue = GetCurrentShieldHP + damageValue;
        _currentShieldHP.Set(newValue);
        ///Reset Timer
        _shieldRecoverTimer = 0;
        return newValue <= 0;
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

    /// <summary>
    /// 护盾回复
    /// </summary>
    private void ShieldRecover()
    {
        var newValue = GetCurrentShieldHP + ShieldRecoverValue;
        _currentShieldHP.Set(newValue);
        if(newValue >= MaxShieldHP)
        {
            ///RecoverFinish
            _shieldRecovering = false;
            _recoverDeltaTimer = 0;
            _shieldRecoverTimer = 0;
        }
    }

    /// <summary>
    /// 护盾最大HP变更
    /// </summary>
    private void CalculateMaxShieldHP()
    {
        var shieldAdd = mainProperty.GetPropertyFinal(PropertyModifyKey.ShieldHP);
        var newValue = Mathf.Clamp(shieldAdd + _shieldHPBase, 0, int.MaxValue); 
        MaxShieldHP = Mathf.CeilToInt(newValue);
        SetShieldMaxHP(MaxShieldHP);
    }

    private void CalculateShieldRecoverValue()
    {
        var recoverAdd = mainProperty.GetPropertyFinal(PropertyModifyKey.ShieldRecoverValue);
        var newValue = Mathf.Clamp(recoverAdd + _shieldRecoverValueBase, 0, int.MaxValue);
        ShieldRecoverValue = Mathf.CeilToInt(newValue);
    }

    private void CalculateShieldRecoverTime()
    {
        var timePercent = mainProperty.GetPropertyFinal(PropertyModifyKey.ShieldRecoverTimeReduce);
        var newValue = Mathf.Clamp(-100, timePercent, float.MaxValue);
        ShieldRecoverTime = ((100 + newValue) / 100f) * _shieldRecoverCDBase;
    }

    /// <summary>
    /// 设置最大血量
    /// </summary>
    /// <param name="value"></param>
    private void SetShieldMaxHP(int value)
    {
        _currentShieldHP.SetMaxValue(value);
    }
}
