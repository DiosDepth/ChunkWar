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
        var allaiPos = ECSManager.Instance.activeAIUnitData.unitPos;
        var targetInfos = GameHelper.FindTargetsByPoint(transform.position, _effectCfg.EffectRadius, allaiPos);
        var allUnits = ECSManager.Instance.GetAIShipAllActiveUnitByTargetsInfo(targetInfos);

        if (allUnits.Count <= 0)
            return;

        for (int i = 0; i < allUnits.Count; i++) 
        {
            ///移除自身
            var unit = allUnits[i];
            if (_owner.UnitList.Contains(unit))
                continue;

            ///Add Time Modifier
            var existData = unit.GetModifierTriggerByUID(UID);
            if(existData == null)
            {
                ///Create New
                MTC_OnAdd newCfg = new MTC_OnAdd(ModifyTriggerType.OnAdd);
                var newData = newCfg.Create(newCfg, UID);
                unit.AddNewModifyEffectData(newData);
                AddUnitTimerModifier(newData, _effectCfg.Modifiers);
            }
            else
            {
                AddUnitTimerModifier(existData, _effectCfg.Modifiers);
            }
        }
    }

    private void AddUnitTimerModifier(ModifyTriggerData data, MTEC_AddUnitTimerModifier[] cfgs)
    {
        if (cfgs == null || cfgs.Length <= 0)
            return;

        for(int i = 0; i < cfgs.Length; i++)
        {
            data.AddTimerModifier_Unit(cfgs[i], UID);
        }
    }

}
