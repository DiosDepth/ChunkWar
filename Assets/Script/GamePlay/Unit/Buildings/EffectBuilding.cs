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
    private float _firstSpawnInterval;

    private int _triggerCount = 0;

    public override void InitBuildingAttribute(OwnerShipType type)
    {
        base.InitBuildingAttribute(type);
        _effectTimer = 0;
    }

    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        _effectCfg = m_unitconfig as EffectBuildingConfig;
        _triggerInterval = _effectCfg.TriggerInterval;
        _firstSpawnInterval = _effectCfg.FirstSpawnCD;

        base.Initialization(m_owner, m_unitconfig);
    }

    public override bool OnUpdateBattle()
    {
        if (!base.OnUpdateBattle())
            return false;

        UpdateEffectTrigger();

        return true;
    }

    public override void OnRemove()
    {
        base.OnRemove();
        _triggerCount = 0;
        _effectTimer = 0;
    }

    private void UpdateEffectTrigger()
    {
        _effectTimer += Time.deltaTime;

        if(_triggerCount == 0)
        {
            ///CheckFirstSpawn
            if(_effectTimer >= _firstSpawnInterval)
            {
                TriggerEffect();
                _triggerCount++;
                _effectTimer = 0;
            }
        }
        else
        {
            if (_effectTimer >= _triggerInterval)
            {
                TriggerEffect();
                _triggerCount++;
                _effectTimer = 0;
            }
        }
    }

    /// <summary>
    /// 触发加成效果
    /// </summary>
    private void TriggerEffect()
    {
        List<AIShip> aiShips;
        var allaiPos = ECSManager.Instance.activeAIUnitData.unitPos;
        var targetInfos = GameHelper.FindTargetsByPoint(transform.position, _effectCfg.EffectRadius, allaiPos);
        var allUnits = ECSManager.Instance.GetAIShipAllActiveUnitByTargetsInfo(targetInfos, out aiShips);

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
                AddUnitTimerModifier(newData, _effectCfg.Modifiers, unit);
            }
            else
            {
                AddUnitTimerModifier(existData, _effectCfg.Modifiers, unit);
            }
        }

        ///HighLight
        for(int i = 0; i < aiShips.Count; i++)
        {
            var targetShip = aiShips[i];
            if(targetShip != null && targetShip != _owner)
            {
                targetShip.ShowEnhanceOutline();
            }
        }
    }

    private void AddUnitTimerModifier(ModifyTriggerData data, MTEC_AddUnitTimerModifier[] cfgs, Unit targetUnit)
    {
        if (cfgs == null || cfgs.Length <= 0)
            return;

        for(int i = 0; i < cfgs.Length; i++)
        {
            data.AddTimerModifier_Unit(cfgs[i], targetUnit, OnUnitEffectRemove);
        }
    }

    private void OnUnitEffectRemove(Unit targetUnit)
    {
        if (targetUnit == null && targetUnit._owner is AIShip)
            return;

        ///待观察，可能不同步
        (targetUnit._owner as AIShip).HideEnhanceOutLine();
    }

}
