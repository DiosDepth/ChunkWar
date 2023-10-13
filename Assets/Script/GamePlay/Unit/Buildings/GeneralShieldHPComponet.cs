using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralShieldHPComponet : BaseBuildingComponent
{
    public int ShieldRecoverValue
    {
        get;
        protected set;
    }

    /// <summary>
    /// ��ò��ܻ��ָ�
    /// </summary>
    public float ShieldRecoverTime
    {
        get;
        protected set;
    }

    public MonoShield monoShield;

    private int _shieldRecoverValueBase;
    private int _shieldHPBase;
    private float _shieldRecoverCDBase;
    private UnitPropertyData mainProperty;


    private ChangeValue<int> _currentShieldHP;

    /// <summary>
    /// �����Ƿ���
    /// </summary>
    public bool IsShieldBroken
    {
        get
        {
            return GetCurrentShieldHP <= 0;
        }
    }

    /// <summary>
    /// ��ǰHP
    /// </summary>
    public int GetCurrentShieldHP
    {
        get { return _currentShieldHP.Value; }
    }

    /// <summary>
    /// HP����
    /// </summary>
    public float ShieldHPPercent
    {
        get { return GetCurrentShieldHP / (float)MaxShieldHP; }
    }

    /// <summary>
    /// ���Ѫ��
    /// </summary>
    public int MaxShieldHP
    {
        get;
        private set;
    }

    /// <summary>
    /// ���ܰ뾶
    /// </summary>
    public float ShieldRatio
    {
        get;
        private set;
    }

    private float _shieldRatioBase = 0;
    /// <summary>
    /// �����ܻ��ظ�Timer
    /// </summary>
    private float _shieldRecoverTimer = 0;
    /// <summary>
    /// �Ƿ����ڻָ�����
    /// </summary>
    private bool _shieldRecovering = false;
    /// <summary>
    /// �ָ�CD
    /// </summary>
    private float _recoverDeltaTimer = 0;
    private float _recoverDeltaTime;

    public GeneralShieldHPComponet(BuildingShieldConfig cfg)
    {
        _shieldRecoverValueBase = cfg.ShieldRecoverValue;
        _shieldRecoverCDBase = cfg.ShieldRecoverTime;
        _shieldHPBase = cfg.ShieldHP;
        _shieldRatioBase = cfg.ShieldBaseRatio;
        _recoverDeltaTime = DataManager.Instance.battleCfg.ShieldRecoverCD;
        mainProperty = RogueManager.Instance.MainPropertyData;
    }

    public override void OnInit(BaseShip owner, Unit parentUnit)
    {
        base.OnInit(owner, parentUnit);
        _currentShieldHP = new ChangeValue<int>(0, 0, 0);
        if (owner is PlayerShip)
        {
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.ShieldHP, CalculateMaxShieldHP);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.ShieldRecoverValue, CalculateShieldRecoverValue);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.ShieldRecoverTimeReduce, CalculateShieldRecoverTime);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.ShieldRatioAdd, CalculateShieldRatio);
            CalculateMaxShieldHP();
            CalculateShieldRecoverValue();
            CalculateShieldRecoverTime();
            CalculateShieldRatio();
        }
        else
        {
            ///Enemy
            MaxShieldHP = _shieldHPBase;
            ShieldRatio = _shieldRatioBase;
            ShieldRecoverTime = _shieldRecoverCDBase;
            ShieldRecoverValue = _shieldRecoverValueBase;
            _currentShieldHP.SetMaxValue(MaxShieldHP);
        }
        
        _currentShieldHP.Set(MaxShieldHP);
    }

    public override void OnUpdate()
    {
        ///�������򲻻ָ�
        if (GetCurrentShieldHP >= MaxShieldHP)
            return;

        if (!_shieldRecovering)
        {
            _shieldRecoverTimer += Time.deltaTime;
            if (_shieldRecoverTimer >= ShieldRecoverTime)
            {
                _shieldRecovering = true;

                if(ParentUnit._owner is PlayerShip)
                {
                    LevelManager.Instance.ShieldRecoverStart(ParentUnit.UID);
                }
                
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

    public override void OnRemove()
    {
        base.OnRemove();
        if(OwnerShip is PlayerShip)
        {
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.ShieldHP, CalculateMaxShieldHP);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.ShieldRecoverValue, CalculateShieldRecoverValue);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.ShieldRecoverTimeReduce, CalculateShieldRecoverTime);
        }

        if(monoShield != null)
        {
            monoShield.OnRemove();
            monoShield = null;
        }
    }
    /// <summary>
    /// �ı�Ѫ��
    /// </summary>
    /// <param name="damageValue">�Ƿ�����</param>
    /// <returns></returns>
    public bool TakeDamage(int damageValue)
    {
        var newValue = GetCurrentShieldHP + damageValue;
        _currentShieldHP.Set(newValue);
        ///Reset Timer
        _shieldRecoverTimer = 0;

        if (IsShieldBroken)
        {
            if(monoShield != null)
            {
                monoShield.SetShieldActive(false);
            }
            LevelManager.Instance.ShieldBroken(ParentUnit.UID);
        }

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
    /// ���ܻظ�
    /// </summary>
    private void ShieldRecover()
    {
        var newValue = GetCurrentShieldHP + ShieldRecoverValue;
        _currentShieldHP.Set(newValue);
        if(newValue >= MaxShieldHP)
        {
            if (monoShield != null)
            {
                monoShield.SetShieldActive(true);
            }
            ///RecoverFinish
            _shieldRecovering = false;

            if(ParentUnit._owner is PlayerShip)
            {
                LevelManager.Instance.ShieldRecoverEnd(ParentUnit.UID);
            }

            _recoverDeltaTimer = 0;
            _shieldRecoverTimer = 0;
        }
    }

    private void CalculateShieldRatio()
    {
        var shieldMinRatio = DataManager.Instance.battleCfg.ShieldRatioMin;
        var ratioValue = mainProperty.GetPropertyFinal(PropertyModifyKey.ShieldRatioAdd);
        ShieldRatio = Mathf.Clamp(ratioValue + _shieldRatioBase, shieldMinRatio, int.MaxValue);
    }

    /// <summary>
    /// �������HP���
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
        var newValue = Mathf.Clamp((1 + recoverAdd / 100f) * _shieldRecoverValueBase, 0, int.MaxValue);
        ShieldRecoverValue = Mathf.CeilToInt(newValue);
    }

    private void CalculateShieldRecoverTime()
    {
        var timePercent = mainProperty.GetPropertyFinal(PropertyModifyKey.ShieldRecoverTimeReduce);
        var newValue = Mathf.Clamp(-100, timePercent, float.MaxValue);
        ShieldRecoverTime = ((100 + newValue) / 100f) * _shieldRecoverCDBase;
    }

    /// <summary>
    /// �������Ѫ��
    /// </summary>
    /// <param name="value"></param>
    private void SetShieldMaxHP(int value)
    {
        _currentShieldHP.SetMaxValue(value);
    }
}
