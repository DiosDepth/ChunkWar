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
    public NativeQueue<Weapon.RV_WeaponTargetInfo> rv_weaponTargetsInfoQue;



    protected List<Weapon.RV_WeaponTargetInfo> targetListcandidator = new List<RV_WeaponTargetInfo>();
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
                    weaponstate.ChangeState(WeaponState.Charging);
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
            job_attackRange = weaponAttribute.WeaponRange,
            job_selfPos = transform.position,
            job_targetsPos = AIManager.Instance.aiActiveUnitPos,
            rv_targetsInfo = rv_weaponTargetsInfoQue.AsParallelWriter(),

        };

        JobHandle jobHandle = findMainWeaponTargetsInRangeJob.ScheduleBatch(AIManager.Instance.aiActiveUnitPos.Length, 2);
        jobHandle.Complete();



        if (rv_weaponTargetsInfoQue.Count == 0)
        {
            return;
        }

        Weapon.RV_WeaponTargetInfo[] slice = new RV_WeaponTargetInfo[rv_weaponTargetsInfoQue.Count];

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
                WeaponTargetInfo info;
                while(targetList.Count < maxTargetCount)
                {
                    for (int i = 0; i < slice.Length; i++)
                    {
                        info = new WeaponTargetInfo
                            (
                                AIManager.Instance.aiActiveUnitList[slice[i].targetIndex].gameObject,
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
                    targetList.Add(new WeaponTargetInfo
                    (
                        AIManager.Instance.aiActiveUnitList[slice[index].targetIndex].gameObject,
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


        public NativeQueue<RV_WeaponTargetInfo>.ParallelWriter rv_targetsInfo;
        //public NativeArray<int> rv_validIndex;
       // [NativeDisableParallelForRestriction]
       // public NativeArray<RV_WeaponTargetInfo> rv_targetsInfo;
        RV_WeaponTargetInfo tempinfo;
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



    public override void Start()
    {
        base.Start();

    }

    public override void Update()
    {
        base.Update();

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
        rv_weaponTargetsInfoQue = new NativeQueue<RV_WeaponTargetInfo>(Allocator.Persistent);
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
