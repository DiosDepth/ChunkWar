using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;




public class ShipMainWeapon : Weapon
{

    public NativeList<float> activeWeaponAttackRangeList;
    public NativeList<float3> activeWeaponPosList;
    public NativeList<int> activeWeaponTargetCountList;
    public NativeQueue<UnitTargetJobData> rv_weaponTargetsInfoQue;



    protected List<UnitTargetJobData> targetListcandidator = new List<UnitTargetJobData>();
    public virtual void HandleShipMainWeapon(InputAction.CallbackContext context)
    {
        if(weaponmode  == WeaponControlType.Autonomy)
        {
            //return here and controlled by HandleShipAutonomyMainWeapon()
            return;
        }
        if(weaponmode == WeaponControlType.Manual)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    weaponState.ChangeState(WeaponState.Charging);
                    break;

                case InputActionPhase.Canceled:

                    WeaponOn();
                    break;
            }
        }

        if (weaponmode == WeaponControlType.SemiAuto)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    WeaponOn();
                    break;

                case InputActionPhase.Canceled:
                    WeaponOff();
                    break;
            }
        }
    }

    public virtual void UpdateMainWeaponTargetList()
    {
        // searching for targets
        rv_weaponTargetsInfoQue.Clear();
        FindMainWeaponTargetsInRangeJob findMainWeaponTargetsInRangeJob = new FindMainWeaponTargetsInRangeJob
        {
            job_attackRange = weaponAttribute.BaseRange,
            job_selfPos = transform.position,
            job_targetsPos = ECSManager.Instance.activeAIUnitData.unitPos,
            rv_targetsInfo = rv_weaponTargetsInfoQue.AsParallelWriter(),

        };

        JobHandle jobHandle = findMainWeaponTargetsInRangeJob.ScheduleBatch(ECSManager.Instance.activeAIUnitData.unitPos.Length, 2);
        jobHandle.Complete();



        if (rv_weaponTargetsInfoQue.Count == 0)
        {
            targetList.Clear();
            return;
        }

        UnitTargetJobData[] slice = new UnitTargetJobData[rv_weaponTargetsInfoQue.Count];

        //slice the searching targets result
        int c = rv_weaponTargetsInfoQue.Count;
        for (int i = 0; i < c; i++)
        {
            slice[i] = rv_weaponTargetsInfoQue.Dequeue();
        }

        slice.Sort();

        //Allocate targetinfo to targetList
        int iterateIndex = Mathf.Min(slice.Length, maxTargetCount);

        if (iterateIndex != 0)
        {
            if (firemode == WeaponFireMode.Linked)
            {
                UnitTargetInfo info;
                while(targetList.Count < maxTargetCount)
                {
                    for (int i = 0; i < slice.Length; i++)
                    {
                        info = new UnitTargetInfo
                            (
                                ECSManager.Instance.activeAIUnitData.unitList[slice[i].targetIndex].gameObject,
                            
                                slice[i].targetIndex,
                                slice[i].distanceToTarget,
                                slice[i].targetDirection
                            );
                        if (!targetList.Contains(info))
                        {
                            targetList.Add(info);
                            break;
                        }
                    }

                }
            }
            else
            {
                targetList.Clear();
                targetListcandidator.Clear();
                int index = 0;
                for (int i = 0; i < maxTargetCount; i++)
                {

                    index = i % iterateIndex;
                    targetList.Add(new UnitTargetInfo
                    (
                        ECSManager.Instance.activeAIUnitData.unitList[slice[index].targetIndex].gameObject,
                        //AIManager.Instance.activeWeaponList[slice[index].targetIndex].gameObject,
                        slice[index].targetIndex,
                        slice[index].distanceToTarget,
                        slice[index].targetDirection
                    ));
                }
            }
        }
    }

    public virtual void HandleShipAutonomyMainWeapon()
    {
        //Check if targetList is valid 
        if(aimingtype == WeaponAimingType.TargetBased || aimingtype == WeaponAimingType.TargetDirectional)
        {
            if (targetList != null && targetList.Count != 0)
            {
                WeaponOn();
            }
            else
            {
                WeaponOff();
            }

        }
        else
        {
            WeaponOn();
        }

    }


    public struct FindMainWeaponTargetsInRangeJob : IJobParallelForBatch
    {
        [ReadOnly] public float job_attackRange;
        [ReadOnly] public float3 job_selfPos;
        [ReadOnly] public NativeArray<float3> job_targetsPos;
        //这里返回的时对应的target在list中的index


        public NativeQueue<UnitTargetJobData>.ParallelWriter rv_targetsInfo;
        //public NativeArray<int> rv_validIndex;
       // [NativeDisableParallelForRestriction]
       // public NativeArray<RV_WeaponTargetInfo> rv_targetsInfo;
        UnitTargetJobData tempinfo;
        int index;
        public void Execute(int startIndex, int count)
        {
           
            for (int i = startIndex; i < startIndex + count; i++)
            {
                if(math.distance(job_selfPos, job_targetsPos[i]) <= job_attackRange)
                {
                    tempinfo.targetPos = job_targetsPos[i];
                    tempinfo.distanceToTarget = math.distance(job_selfPos, job_targetsPos[i]);
                    tempinfo.targetDirection = math.normalize(job_targetsPos[i] - job_selfPos);
                    tempinfo.targetIndex = i;
                    rv_targetsInfo.Enqueue(tempinfo);
                }
            }
        }
    }
    protected override void OnDestroy()
    {
        if (activeWeaponPosList.IsCreated) { activeWeaponPosList.Dispose(); }
        if (activeWeaponAttackRangeList.IsCreated) { activeWeaponAttackRangeList.Dispose(); }
        if (activeWeaponTargetCountList.IsCreated) { activeWeaponTargetCountList.Dispose(); }
        if (rv_weaponTargetsInfoQue.IsCreated) { rv_weaponTargetsInfoQue.Dispose(); }
        base.OnDestroy();
    }

    public override void Death(UnitDeathInfo info)
    {
        if (activeWeaponPosList.IsCreated) { activeWeaponPosList.Dispose(); }
        if (activeWeaponAttackRangeList.IsCreated) { activeWeaponAttackRangeList.Dispose(); }
        if (activeWeaponTargetCountList.IsCreated) { activeWeaponTargetCountList.Dispose(); }
        if (rv_weaponTargetsInfoQue.IsCreated) { rv_weaponTargetsInfoQue.Dispose(); }
        base.Death(info);
    }


    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        base.Initialization(m_owner, m_unitconfig);
        activeWeaponPosList = new NativeList<float3>(Allocator.Persistent);
        activeWeaponAttackRangeList = new NativeList<float>(Allocator.Persistent);
        activeWeaponTargetCountList = new NativeList<int>(Allocator.Persistent);
        rv_weaponTargetsInfoQue = new NativeQueue<UnitTargetJobData>(Allocator.Persistent);
    }

    public override void InitWeaponAttribute(OwnerShipType type)
    {
        base.InitWeaponAttribute(type);
        if (weaponAttribute.MagazineBased)
        {
            //magazine = 0;
            magazine = weaponAttribute.MaxMagazineSize;
        }
        else
        {
            ///Fix Value
            magazine = 1;
        }

    }
    public override void ProcessWeapon()
    {
        if (aimingtype == WeaponAimingType.TargetBased || aimingtype == WeaponAimingType.TargetDirectional)
        {
            UpdateMainWeaponTargetList();
        }

        if (weaponmode == WeaponControlType.Autonomy)
        {
            HandleShipAutonomyMainWeapon();
        }

        base.ProcessWeapon();

    }

    public override void WeaponOn()
    {
        base.WeaponOn();
    }

    public override void WeaponOff()
    {
        base.WeaponOff();
    }

    public override void WeaponReady()
    {


        base.WeaponReady();
    }

    public override void WeaponStart()
    {


        base.WeaponStart();
    }

    public override void WeaponBeforeDelay()
    {
        base.WeaponBeforeDelay();
    }

    public override void FireRequest()
    {
        base.FireRequest();
    }

    public override void WeaponFiring()
    {
        base.WeaponFiring();
    }

    public override void DoFire()
    {
        base.DoFire();
    }

    public override void WeaponBetweenDelay()
    {
        base.WeaponBetweenDelay();
    }

    public override void WeaponCharging()
    {
        base.WeaponCharging();
    }

    public override void WeaponCharged()
    {
        base.WeaponCharged();
    }

    public override void WeaponFired()
    {
        base.WeaponFired();
    }

    public override void WeaponAfterDelay()
    {
        base.WeaponAfterDelay();
    }

    public override void WeaponEnd()
    {
        base.WeaponEnd();
    }

    public override void WeaponReload()
    {
        base.WeaponReload();
    }

    public override void WeaponRecover()
    {
        base.WeaponRecover();
    }

    public override bool TakeDamage(DamageResultInfo info)
    {
        return base.TakeDamage(info);
    }
}
