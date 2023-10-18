using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 周期性给范围内目标加效果的building
 */
public class EffectBuilding : Building
{

    private EffectBuildingConfig _effectCfg;

    private float _effectTimer;
    private float _triggerInterval;

    public override void InitBuildingAttribute(OwnerShipType type)
    {
        base.InitBuildingAttribute(type);
        _effectTimer = 0;
    }

    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        _effectCfg = m_unitconfig as EffectBuildingConfig;
        _triggerInterval = _effectCfg.TriggerInterval;

        base.Initialization(m_owner, m_unitconfig);
    }

    public override bool OnUpdateBattle()
    {
        if (base.OnUpdateBattle())
            return false;

        UpdateEffectTrigger();

        return true;
    }

    private void UpdateEffectTrigger()
    {
        _effectTimer += Time.deltaTime;
        if(_effectTimer >= _triggerInterval)
        {
            TriggerEffect();
            _effectTimer = 0;
        }
    }

    /// <summary>
    /// 触发加成效果
    /// </summary>
    private void TriggerEffect()
    {
        var allaiPos = AIManager.Instance.position;
        var targetInfos = GameHelper.FindTargetsByPoint(transform.position, _effectCfg.EffectRadius, allaiPos);
        var allUnits = AIManager.Instance.GetActiveUnitReferenceByTargetInfo(targetInfos);



    }

}
