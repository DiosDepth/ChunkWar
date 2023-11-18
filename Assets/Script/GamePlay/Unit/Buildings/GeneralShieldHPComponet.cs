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
    /// 多久不受击恢复
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
    private BuildingShieldConfig _shieldCfg;


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
    /// 护盾半径
    /// </summary>
    public float ShieldRatio
    {
        get;
        private set;
    }

    private float _shieldRatioBase = 0;
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

    public GeneralShieldHPComponet(BuildingShieldConfig cfg)
    {
        _shieldCfg = cfg;
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
            CalculateMaxShieldHP();
            CalculateShieldRecoverValue();
            CalculateShieldRecoverTime();
            InitShieldRatio();
        }
        else if( owner is AIShip || owner is AIDrone)
        {
            EnemyHardLevelItem hardLevel = null;

            if(owner is AIShip)
            {
                var parentAI = OwnerShip as AIShip;
                hardLevel = GameHelper.GetEnemyHardLevelItem(parentAI.AIShipCfg.HardLevelGroupID);
            }
            else
            {
                var parentDrone = OwnerShip as AIDrone;
                hardLevel = GameHelper.GetEnemyHardLevelItem(parentDrone.droneCfg.HardLevelGroupID);
            }

            if(hardLevel != null)
            {
                MaxShieldHP = Mathf.RoundToInt(_shieldHPBase * (1 + hardLevel.ShieldHPPercentAdd / 100f));
            }
            else
            {
                MaxShieldHP = _shieldHPBase;
            }

            ///Enemy
            ShieldRatio = _shieldRatioBase;
            ShieldRecoverTime = _shieldRecoverCDBase;
            ShieldRecoverValue = _shieldRecoverValueBase;
            _currentShieldHP.SetMaxValue(MaxShieldHP);
        }
        
        _currentShieldHP.Set(MaxShieldHP);
    }

    public override void OnUpdate()
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

        if (IsShieldBroken)
        {
            if(monoShield != null)
            {
                monoShield.SetShieldActive(false);
            }
            LevelManager.Instance.ShieldBroken(ParentUnit.UID);
            SoundManager.Instance.PlayBattleSound(_shieldCfg.ShieldBreakAudio, monoShield.transform);
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

    public override void DeSpawn()
    {
        base.DeSpawn();
        if (monoShield != null)
        {
            monoShield.OnRemove();
            monoShield = null;
        }
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

    private void InitShieldRatio()
    {
        ShieldRatio = _shieldRatioBase;
        ///Refresh MonoShield
        if (monoShield != null)
        {
            monoShield.UpdateShieldRatio();
        }
    }

    /// <summary>
    /// 护盾最大HP变更
    /// </summary>
    private void CalculateMaxShieldHP()
    {
        MaxShieldHP = GameHelper.CalculateShieldHP(_shieldHPBase);
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
    /// 设置最大血量
    /// </summary>
    /// <param name="value"></param>
    private void SetShieldMaxHP(int value)
    {
        _currentShieldHP.SetMaxValue(value);
    }
}
