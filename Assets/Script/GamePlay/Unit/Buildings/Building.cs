using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class BuildingAttribute : UnitBaseAttribute
{

    public override void InitProeprty(Unit parentUnit, BaseUnitConfig cfg, bool isPlayerShip)
    {
        base.InitProeprty(parentUnit, cfg, isPlayerShip);
    }

    public override void Destroy()
    {
        base.Destroy();
    }
}

public class Building : Unit
{

    public struct BuildingTargetInfo : IComparable<BuildingTargetInfo>
    {
        public int targetIndex;
        public float distanceToTarget;
        public float3 targetPos;
        public float3 targetDirection;

        public int CompareTo(BuildingTargetInfo other)
        {
            return distanceToTarget.CompareTo(other.distanceToTarget);
        }
    }

    private List<BaseBuildingComponent> _buildingComponents = new List<BaseBuildingComponent>();

    protected BuildingConfig _buildingConfig;

    public BuildingAttribute buildingAttribute;

    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
    
        base.Update();
   
    }



    public virtual void BuildingUpdate()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        UpdateBuildingComponents();
    }

    protected override void OnDestroy()
    {
        buildingAttribute?.Destroy();
        base.OnDestroy();
    }

    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        _baseUnitConfig = m_unitconfig;
        _buildingConfig = m_unitconfig as BuildingConfig;
        _owner = m_owner;
        InitBuildingAttribute(m_owner is PlayerShip);
        base.Initialization(m_owner, m_unitconfig);
 
        InitBuildingComponent();
    }

    public virtual void InitBuildingAttribute(bool isPlayerShip)
    {
        buildingAttribute.InitProeprty(this, _buildingConfig, isPlayerShip);
        baseAttribute = buildingAttribute;
    }

    public override bool TakeDamage(DamageResultInfo info)
    {
        return base.TakeDamage(info);
    }

    public override void Death(UnitDeathInfo info)
    {
        base.Death(info);
        RemoveAllBuildingComponents();
    }

    #region Building Components

    /// <summary>
    /// 添加建筑组件
    /// </summary>
    /// <param name="cmpt"></param>
    public void AddBuildingComponent(BaseBuildingComponent cmpt)
    {
        ///Unique
        if (_buildingComponents.Contains(cmpt))
            return;

        cmpt.OnInit(_owner, this);
        _buildingComponents.Add(cmpt);
    }

    /// <summary>
    /// 获取建筑组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetBuildingComponent<T>() where T : BaseBuildingComponent
    {
        for (int i = 0; i < _buildingComponents.Count; i++)
        {
            if (_buildingComponents[i] is T)
                return _buildingComponents[i] as T;
        }
        return null;
    }

    private void InitBuildingComponent()
    {
        GenerateShipShield();
    }

    private void UpdateBuildingComponents()
    {
        if (_buildingComponents.Count <= 0)
            return;

        for(int i = 0; i < _buildingComponents.Count; i++)
        {
            _buildingComponents[i].OnUpdate();
        }
    }

    private void RemoveAllBuildingComponents()
    {
        if (_buildingComponents.Count <= 0)
            return;

        for (int i = 0; i < _buildingComponents.Count; i++)
        {
            _buildingComponents[i].OnRemove();
        }
    }

    #endregion

    private void GenerateShipShield()
    {
        var shieldCfg = _buildingConfig.ShieldConfig;
        if (shieldCfg.GenerateShield)
        {
            ///Generate Shield
            GeneralShieldHPComponet shieldCmpt = new GeneralShieldHPComponet(shieldCfg);
            AddBuildingComponent(shieldCmpt);

            var shieldPath = GameGlobalConfig.ShieldPath + shieldCfg.ShieldPrefabName;
            PoolManager.Instance.GetObjectAsync(shieldPath, true, (obj) =>
            {
                var cmpt = obj.GetComponent<MonoShield>();
                cmpt.InitShield(shieldCmpt, _owner);
                shieldCmpt.monoShield = cmpt;
            }, transform);
        }
    }


    public override void PauseGame()
    {
        base.PauseGame();
    }
    public override void UnPauseGame()
    {
        base.UnPauseGame();
    }

}
