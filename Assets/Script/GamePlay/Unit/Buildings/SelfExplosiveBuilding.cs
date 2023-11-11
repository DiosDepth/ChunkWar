using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

[System.Serializable]
public class SelfExplosiveBuildingAttribute : BuildingAttribute
{
    public float DamageRange
    {
        get;
        protected set;
    }

    public int DamageValue
    {
        get;
        protected set;
    }

    public float ExplodeDelay
    {
        get;
        protected set;
    }


    public override void InitProeprty(Unit parentUnit, BaseUnitConfig cfg, OwnerShipType ownerType)
    {
        base.InitProeprty(parentUnit, cfg, ownerType);
        var _Cfg = cfg as SelfExplosiveBuildingConfig;

        DamageRange = _Cfg.DamageRange;
        DamageValue = _Cfg.DamageValue;
        ExplodeDelay = _Cfg.ExplodeDelay;
    }
}



public class SelfExplosiveBuilding : Building
{

    public SelfExplosiveBuildingAttribute selfExplosiveBuildingAttribute;
    protected SelfExplosiveBuildingConfig _selfExplosiveBuildingCfg;


    public List<IDamageble> prepareDamageTargetList = new List<IDamageble>();
    private bool _inExplodeProcess;


    NativeArray<int> rv_findedTargetIndex_Unit;
    NativeArray<int> rv_findedTargetIndex_Drone;

    protected UnitData _unitData;
    protected UnitData _droneData;
    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        _selfExplosiveBuildingCfg = m_unitconfig as SelfExplosiveBuildingConfig;
        base.Initialization(m_owner, m_unitconfig);
        SetUnitData();
        _inExplodeProcess = false;


    }
    public override void InitBuildingAttribute(OwnerShipType type)
    {
        base.InitBuildingAttribute(type);
        selfExplosiveBuildingAttribute.InitProeprty (this, _selfExplosiveBuildingCfg, type);
 
    }
    public virtual void SetUnitData()
    {
        if(GetOwnerType == OwnerType.AI)
        {
            _unitData = ECSManager.Instance.activePlayerUnitData;
            _droneData = ECSManager.Instance.activePlayerDroneUnitData;
        }
        if(GetOwnerType == OwnerType.Player)
        {
            _unitData = ECSManager.Instance.activeAIUnitData;
            _droneData = ECSManager.Instance.activeAIDroneUnitData;
        }

    }
    public override void BuildingOFF()
    {
        base.BuildingOFF();
    }

    public override void BuildingOn()
    {
        base.BuildingOn();
    }

    public override void BuildingReady()
    {
        base.BuildingReady();
    }
    public override void BuildingStart()
    {
        if (!_inExplodeProcess) { return; }
        base.BuildingStart();


        LeanTween.value(0, 1, _selfExplosiveBuildingCfg.ExplodeDelay).setOnStart(() => 
        {
            _inExplodeProcess = true;
            (_owner.controller as SteeringBehaviorController).steeringState = SteeringState.Immobilized;
            SetAnimatorTrigger(AnimTrigger_SelfExplode);
        }).setOnComplete(() => 
        {
            SetAnimatorTrigger(AnimTrigger_SelfExplode);
            ApplyDamageAllTarget();
            buildingState.ChangeState(BuildingState.Active);
        });
    }
    public override void BuildingActive()
    {
        base.BuildingActive();

        buildingState.ChangeState(BuildingState.End);
    }
    public override void BuildingEnd()
    {
        base.BuildingEnd();

        UnitDeathInfo deathInfo = new UnitDeathInfo
        {
            isCriticalKill = false,
        };

        Death(deathInfo);

    }
    public override void BuildingRecover()
    {
        base.BuildingRecover();
   
    }

    public virtual bool ApplyDamageAllTarget()
    {
        if(targetList==null || targetList.Count == 0) { return false; }
        rv_findedTargetIndex_Unit = new NativeArray<int>(_unitData.unitList.Count, Allocator.TempJob);
        rv_findedTargetIndex_Drone = new NativeArray<int>(_droneData.unitList.Count, Allocator.TempJob);

        JobHandle jobHandle_Unit;
        JobHandle jobHandle_Drone;

        FindExplosiveBuildingDamageTargetJob findExplosiveBuildingDamageTargetJob_Unit = new FindExplosiveBuildingDamageTargetJob
        {
            job_selfPos = this.transform.position,
            job_searchingRange = selfExplosiveBuildingAttribute.DamageRange,
            job_targetPos = _unitData.unitPos,

            rv_findedTargetIndex = rv_findedTargetIndex_Unit,
        };
        jobHandle_Unit = findExplosiveBuildingDamageTargetJob_Unit.Schedule(_unitData.unitList.Count, 32);
        jobHandle_Unit.Complete();

        FindExplosiveBuildingDamageTargetJob findExplosiveBuildingDamageTargetJob_Drone = new FindExplosiveBuildingDamageTargetJob
        {
            job_selfPos = this.transform.position,
            job_searchingRange = selfExplosiveBuildingAttribute.DamageRange,
            job_targetPos = _droneData.unitPos,

            rv_findedTargetIndex = rv_findedTargetIndex_Drone,
        };
        jobHandle_Drone = findExplosiveBuildingDamageTargetJob_Unit.Schedule(_droneData.unitList.Count, 32);
        jobHandle_Drone.Complete();



        for (int i = 0; i < rv_findedTargetIndex_Unit.Length; i++)
        {
            IDamageble damageble = _unitData.unitList[rv_findedTargetIndex_Unit[i]].GetComponent<IDamageble>();
            if (damageble != null)
            {
                prepareDamageTargetList.Add(damageble);
            }
            else
            {
                continue;
            }
        }


        for (int i = 0; i < rv_findedTargetIndex_Drone.Length; i++)
        {
            IDamageble damageble = _droneData.unitList[rv_findedTargetIndex_Unit[i]].GetComponent<IDamageble>();
            if (damageble != null)
            {
                prepareDamageTargetList.Add(damageble);
            }
            else
            {
                continue;
            }
        }


        for (int i = 0; i < prepareDamageTargetList.Count; i++)
        {
            DamageResultInfo info = new DamageResultInfo()
            {
                attackerUnit = this,
                Target = prepareDamageTargetList[i],
                Damage = (int)selfExplosiveBuildingAttribute.DamageValue,
                IsCritical = false,
                DamageType = WeaponDamageType.Physics,
                IsPlayerAttack = GetOwnerType == OwnerType.Player ? true : false,
            };
            prepareDamageTargetList[i].TakeDamage(info);
        }



        rv_findedTargetIndex_Unit.Dispose();
        rv_findedTargetIndex_Drone.Dispose();

        return true;
    }

    public override void Death(UnitDeathInfo info)
    {
        base.Death(info);
        
    }

    public override void GameOver()
    {
        base.GameOver();
    }

    public override UnitLogData GetUnitLogData()
    {
        return base.GetUnitLogData();
    }



    public override bool OnUpdateBattle()
    {
        return base.OnUpdateBattle();
    }

    public override void ProcessBuilding()
    {
        base.ProcessBuilding();
    }

    public override void Restore()
    {

        base.Restore();
    }

    public override bool TakeDamage(DamageResultInfo info)
    {
        return base.TakeDamage(info);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void SetAnimatorTrigger(string trigger)
    {
        base.SetAnimatorTrigger(trigger);
    }

    protected override void OnEnterParalysisState()
    {
        base.OnEnterParalysisState();
    }

    protected override void OnParalysisStateFinish()
    {
        base.OnParalysisStateFinish();
    }

    protected override void ResetAllAnimation()
    {
        base.ResetAllAnimation();
    }

    protected override void _DoDeSpawnEffect()
    {
        base._DoDeSpawnEffect();
    }


    public struct FindExplosiveBuildingDamageTargetJob : IJobParallelFor
    {
        [Unity.Collections.ReadOnly] public float3 job_selfPos;
        [Unity.Collections.ReadOnly] public NativeArray<float3> job_targetPos;
        [Unity.Collections.ReadOnly] public float job_searchingRange;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<int> rv_findedTargetIndex;
        int index;
        public void Execute(int i)
        {
            if (math.distance( job_targetPos[i], job_selfPos) <= job_searchingRange)
            {
                rv_findedTargetIndex[index] = i;
                index++;
            }
        }
    }

}
