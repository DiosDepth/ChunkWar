using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;




public class ShipWeapon : Weapon
{

    public NativeList<float> activeWeaponAttackRangeList;
    public NativeList<float3> activeWeaponPosList;
    public NativeList<int> activeWeaponTargetCountList;
    public NativeArray<Weapon.RV_WeaponTargetInfo> rv_weaponTargetsInfo;
    public NativeArray<int> rv_validTargetCount;


    private List<Weapon.RV_WeaponTargetInfo> targetListcandidator = new List<RV_WeaponTargetInfo>();
    public virtual void HandleShipManualWeapon(InputAction.CallbackContext context)
    {
        if(weaponmode  == WeaponControlType.Autonomy)
        {
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

    public void HandleShipAutonomyMainWeapon()
    {
        // searching for targets

        rv_weaponTargetsInfo = new NativeArray<Weapon.RV_WeaponTargetInfo>(AIManager.Instance.aiActiveUnitPos.Length , Allocator.TempJob);
        rv_validTargetCount = new NativeArray<int>(1, Allocator.TempJob);

        FindMainWeaponTargetsInRangeJob findMainWeaponTargetsInRangeJob = new FindMainWeaponTargetsInRangeJob
        {
            job_attackRange = weaponAttribute.WeaponRange,
            job_selfPos = transform.position,
            job_targetsPos = AIManager.Instance.aiActiveUnitPos,
            rv_validIndex = rv_validTargetCount,
            rv_targetsInfo = rv_weaponTargetsInfo,

        };

        JobHandle jobHandle = findMainWeaponTargetsInRangeJob.ScheduleBatch(AIManager.Instance.aiActiveUnitPos.Length, 2);
        jobHandle.Complete();

        targetList.Clear();
        targetListcandidator.Clear();

        //slice the searching targets result
        Weapon.RV_WeaponTargetInfo[] slice = rv_weaponTargetsInfo.Slice(0, rv_validTargetCount[0]).ToArray();
        slice.Sort();


        //Allocate targetinfo to targetList
        int iterateIndex = Mathf.Min(slice.Length, maxTargetCount);
        if (iterateIndex != 0)
        {
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
        }
       

        rv_validTargetCount.Dispose();
        rv_weaponTargetsInfo.Dispose();
    }


    public struct FindMainWeaponTargetsInRangeJob : IJobParallelForBatch
    {
        [ReadOnly] public float job_attackRange;
        [ReadOnly] public float3 job_selfPos;
        [ReadOnly] public NativeArray<float3> job_targetsPos;
        //这里返回的时对应的target在list中的index

        public NativeArray<int> rv_validIndex;
        public NativeArray<RV_WeaponTargetInfo> rv_targetsInfo;
        RV_WeaponTargetInfo tempinfo;
        int index;
        public void Execute(int startIndex, int count)
        {
            index = 0;
            for (int i = startIndex; i < startIndex + count; i++)
            {
                if(math.distance(job_selfPos, job_targetsPos[i]) <= job_attackRange)
                {
                    tempinfo.targetPos = job_targetsPos[i];
                    tempinfo.distanceToTarget = math.distance(job_selfPos, job_targetsPos[i]);
                    tempinfo.targetDirection = math.normalize(job_targetsPos[i] - job_selfPos);
                    tempinfo.targetIndex = i;
                    rv_targetsInfo[index] = tempinfo;
                    index++;
                    rv_validIndex[0] = index;
                }
            }
        }

        public void Execute(int index)
        {
            throw new System.NotImplementedException();
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
        base.OnDestroy();
    }

    public override void Death()
    {
        if (activeWeaponPosList.IsCreated) { activeWeaponPosList.Dispose(); }
        if (activeWeaponAttackRangeList.IsCreated) { activeWeaponAttackRangeList.Dispose(); }
        if (activeWeaponTargetCountList.IsCreated) { activeWeaponTargetCountList.Dispose(); }
        base.Death();
    }


    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        base.Initialization(m_owner, m_unitconfig);
        activeWeaponPosList = new NativeList<float3>(Allocator.Persistent);
        activeWeaponAttackRangeList = new NativeList<float>(Allocator.Persistent);
        activeWeaponTargetCountList = new NativeList<int>(Allocator.Persistent);

    }

    public override void ProcessWeapon()
    {
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

    public override bool TakeDamage(ref DamageResultInfo info)
    {
        return base.TakeDamage(ref info);
    }
}
