using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;
using static Weapon;

public class ShipUnitManager 
{
    public BaseShip targetShip;

    public List<ShipWeapon> activeWeaponList;

    public List<Building> activeBuildingList;


    public NativeList<float> activeWeaponAttackRangeList;
    public NativeList<float3> activeWeaponPosList;
    public NativeList<int> activeWeaponTargetCountList;
    public NativeArray<Weapon.RV_WeaponTargetInfo> rv_weaponTargetsInfo;

    public ShipUnitManager()
    {
        activeWeaponList = new List<ShipWeapon>();
        activeBuildingList = new List<Building>();
        activeWeaponPosList = new NativeList<float3>(Allocator.Persistent);
        activeWeaponAttackRangeList = new NativeList<float>(Allocator.Persistent);
        activeWeaponTargetCountList = new NativeList<int>(Allocator.Persistent);
    }

    ~ ShipUnitManager()
    {
        if (activeWeaponPosList.IsCreated) { activeWeaponPosList.Dispose(); }
        if (activeWeaponAttackRangeList.IsCreated) { activeWeaponAttackRangeList.Dispose(); }
        if (activeWeaponTargetCountList.IsCreated) { activeWeaponTargetCountList.Dispose(); }
    }

    public virtual void Initialization(BaseShip target)
    {
        targetShip = target;
    }

    public virtual void Update()
    {
        UpdateActiveUnit();
        UpdateWeapon();
        UpdateBuilding();
    }



    //Only other weapon but main weapon
    protected virtual void UpdateWeapon()
    {
        if(activeWeaponList.Count <= 0)
        {
            return;
        }
        int targetstotalcount = 0;
        for (int i = 0; i < activeWeaponList.Count; i++)
        {
            targetstotalcount += activeWeaponList[i].maxTargetCount;
        }

        rv_weaponTargetsInfo = new NativeArray<Weapon.RV_WeaponTargetInfo>(targetstotalcount, Allocator.TempJob);

        // 如果Process 则开启索敌Job， 并且蒋索敌结果记录在targetsindex[]中
        //这个Job返回一个JRD_targetsInfo 这个是已经排序的数据， 包含 index， Pos， direction， distance 几部分数据


        Weapon.FindWeaponTargetsJob findWeaponTargetsJob = new Weapon.FindWeaponTargetsJob
        {
            job_attackRange = activeWeaponAttackRangeList,
            job_selfPos = activeWeaponPosList,
            job_targetsPos = AIManager.Instance.aiActiveUnitPos,
            job_maxTargetCount = activeWeaponTargetCountList,

            rv_targetsInfo = rv_weaponTargetsInfo,

        };

        JobHandle jobHandle = findWeaponTargetsJob.ScheduleBatch(activeWeaponList.Count, 2);
        jobHandle.Complete();


        int startindex;
        int targetindex;
        for (int i = 0; i < activeWeaponList.Count; i++)
        {
            //获取Flat Array中的startindex 为后续的拆分做准备
            //吧前面每一个unit的 maxtargetscount全部加起来就是FlatArray中的第一个index
            startindex = 0;
            for (int c = 0; c < i; c++)
            {
                startindex += activeWeaponTargetCountList[i];
            }


            ShipWeapon weapon = activeWeaponList[i];
            weapon.targetList.Clear();
            for (int n = 0; n < activeWeaponTargetCountList[i]; n++)
            {
                targetindex = rv_weaponTargetsInfo[startindex + n].targetIndex;
                if (targetindex == -1)
                {
                    weapon.WeaponOff();
                    break;
                }
                else
                {
                    if (AIManager.Instance.aiActiveUnitList == null || AIManager.Instance.aiActiveUnitList.Count == 0)
                    {
                        weapon.WeaponOff();
                        break;
                    }
                    weapon.targetList.Add(new WeaponTargetInfo
                        (
                            AIManager.Instance.aiActiveUnitList[targetindex].gameObject,
                            rv_weaponTargetsInfo[startindex + n].targetIndex,
                            rv_weaponTargetsInfo[startindex + n].distanceToTarget,
                            rv_weaponTargetsInfo[startindex + n].targetDirection
                        ));
                }
            }
            if (weapon.targetList != null && weapon.targetList.Count != 0)
            {
                weapon.WeaponOn();
            }
            weapon.ProcessWeapon();
        }


        rv_weaponTargetsInfo.Dispose();
    }

    protected virtual void UpdateBuilding()
    {

    }

    public virtual void AddActiveUnit(Unit unit)
    {
        if(unit is ShipWeapon)
        {
            ShipWeapon weapon = unit as ShipWeapon;
            if(weapon.WeaponCfg.unitType == UnitType.MainWeapons)
            {
                return;
            }
            if(activeWeaponList.Contains(weapon))
            {
                return;
            }
            activeWeaponList.Add(weapon);
            activeWeaponAttackRangeList.Add(weapon.baseAttribute.WeaponRange);
            activeWeaponPosList.Add(weapon.transform.position);
            activeWeaponTargetCountList.Add(weapon.maxTargetCount);
        }
        
        if(unit is Building)
        {
            Building building = unit as Building;
            if(activeBuildingList.Contains(building))
            {
                return;
            }
            activeBuildingList.Add(building);
        }
    }

    public virtual void RemoveActiveUnit(Unit unit)
    {
        if( unit is ShipWeapon)
        {
            ShipWeapon weapon = unit as ShipWeapon;
            int index = activeWeaponList.IndexOf(weapon);
            activeWeaponList.RemoveAt(index);
            activeWeaponPosList.RemoveAt(index);
            activeWeaponAttackRangeList.RemoveAt(index);
            activeWeaponTargetCountList.RemoveAt(index);
        }

        if(unit is Building)
        {
            Building building = unit as Building;
            int index = activeBuildingList.IndexOf(building);

            activeBuildingList.RemoveAt(index);
        }
      
    }
    public virtual void UpdateActiveUnit()
    {
        for (int i = 0; i < activeWeaponList.Count; i++)
        {
            activeWeaponPosList[i] = activeWeaponList[i].transform.position;
        }
    }
    protected virtual List<T> FindAllActiveUnit<T>(List<T> list) where T : Unit
    {
        return list.FindAll(x => x != null && x.isActiveAndEnabled).ConvertAll(x => x as T);
    }

}
