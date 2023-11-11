using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class SelfExplosiveBuildingAttribute : BuildingAttribute
{
    public float DamageRange
    {
        get;
        protected set;
    }

    public float DamageValue
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
        if (_inExplodeProcess) { return; }
        base.BuildingStart();


        LeanTween.value(0, 1, _selfExplosiveBuildingCfg.ExplodeDelay).setOnStart(() => 
        {
            SetAnimatorTrigger(AnimTrigger_SelfExplode);
        }).setOnComplete(() => 
        {
            SetAnimatorTrigger(AnimTrigger_SelfExplode);


            buildingState.ChangeState(BuildingState.Active);
        });
    }
    public override void BuildingActive()
    {
        base.BuildingActive();

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

        FindExplosiveBuildingDamageTargetJob findExplosiveBuildingDamageTargetJob_Drone= new FindExplosiveBuildingDamageTargetJob
        {
            job_selfPos = this.transform.position,
            job_searchingRange = selfExplosiveBuildingAttribute.DamageRange,
            job_targetPos = _droneData.unitPos,

            rv_findedTargetIndex = rv_findedTargetIndex_Drone,
        };
        jobHandle_Drone = findExplosiveBuildingDamageTargetJob_Unit.Schedule(_droneData.unitList.Count, 32);
        jobHandle_Drone.Complete();


        rv_findedTargetIndex_Unit.Dispose();
        rv_findedTargetIndex_Drone.Dispose();



    }
    public override void BuildingEnd()
    {
        base.BuildingEnd();
    }
    public override void BuildingRecover()
    {
        base.BuildingRecover();
   
    }

    public override bool ApplyDamageAllTarget()
    {

        if(targetList)

        if (prepareDamageTargetList == null || prepareDamageTargetList.Count == 0) { return false; }


        if (damagePattern == DamagePattern.Target && initialTarget != null)
        {
            ApplyDamage(initialTarget.GetComponent<IDamageble>());
        }

        if (damagePattern == DamagePattern.PointRadius)
        {
            ///Explode Damage
            LevelManager.Instance.PlayerCreateExplode();
            for (int i = 0; i < prepareDamageTargetList.Count; i++)
            {
                ApplyDamage(prepareDamageTargetList[i]);
            }
        }

        if (damagePattern == DamagePattern.Touched)
        {
            ApplyDamage(prepareDamageTargetList[0]);
        }
        prepareDamageTargetList.Clear();
        return true;

        var succ = base.ApplyDamageAllTarget();
        if (succ)
        {
            SoundManager.Instance.PlayBattleSound(_projectileCfg.HitAudio, transform);
            if (!string.IsNullOrEmpty(_projectileCfg.HitEffect.EffectName))
            {
                var explodeRange = GameHelper.CalculateExplodeRange(DamageRadiusBase);
                EffectManager.Instance.CreateEffect(_projectileCfg.HitEffect, transform.position, explodeRange * 2);
            }
            OnHitEffect();
        }
        return succ;
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
