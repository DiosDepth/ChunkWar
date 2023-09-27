using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponLongRangeIonBlasters : ShipWeapon
{
    public List<WeaponTargetInfo> restordeTargetList = new List<WeaponTargetInfo>();
    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        base.Initialization(m_owner, m_unitconfig);
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
        base.OnDestroy();
    }


    public override void HandleShipAutonomyMainWeapon()
    {
        //base.HandleShipAutonomyMainWeapon();
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
        //如果没有在开火或者在开火间歇中，则重新刷写weapon.targetlist
        if (weaponstate.CurrentState != WeaponState.Firing &&
            weaponstate.CurrentState != WeaponState.BetweenDelay)
        {
            int iterateIndex = Mathf.Min(slice.Length, maxTargetCount);
            if (iterateIndex != 0)
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
        //Check if targetList is valid 
        if (targetList != null && targetList.Count != 0)
        {
            WeaponOn();
        }
        else
        {
            WeaponOff();
        }

        ProcessWeapon();
        //rv_weaponTargetsInfoQue.Dispose();
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

    public override void WeaponCharging()
    {
        base.WeaponCharging();
    }

    public override void WeaponCharged()
    {
        base.WeaponCharged();
    }

    public override void WeaponFiring()
    {
        base.WeaponFiring();
    }

    public override void WeaponBetweenDelay()
    {
        base.WeaponBetweenDelay();
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

    public override bool TakeDamage(ref DamageResultInfo info)
    {
        return base.TakeDamage(ref info);
    }


}
