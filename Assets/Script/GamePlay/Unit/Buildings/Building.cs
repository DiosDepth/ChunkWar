using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class BuildingAttribute : UnitBaseAttribute
{
    public override void InitProeprty(Unit parentUnit, BaseUnitConfig cfg, OwnerShipType shipType)
    {
        base.InitProeprty(parentUnit, cfg, shipType);
    }

    public override void Destroy()
    {
        base.Destroy();
    }
}

public enum BuildingState
{
    Ready,
    Start,
    Active,
    End,
    Recover,
}

public class Building : Unit
{
    public StateMachine<BuildingState> buildingState;
    private List<BaseBuildingComponent> _buildingComponents = new List<BaseBuildingComponent>();

    protected BuildingConfig _buildingConfig;

    public BuildingAttribute buildingAttribute;


    protected bool _isBuildingOn = false;   


    public override bool OnUpdateBattle()
    {

        if (!base.OnUpdateBattle())
            return false;

        UpdateBuildingComponents();
        return true;
    }


    protected override void OnDestroy()
    {
        buildingAttribute?.Destroy();
        base.OnDestroy();
    }

    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        _buildingConfig = m_unitconfig as BuildingConfig;
        ///TempFix
        _baseUnitConfig = m_unitconfig;
        _owner = m_owner;
        InitBuildingAttribute(GameHelper.GetOwnerShipType(_owner));
        buildingState = new StateMachine<BuildingState>(this.gameObject,false,false);
        buildingState.ChangeState(BuildingState.Ready);

        base.Initialization(m_owner, m_unitconfig);
        InitBuildingComponent();
    }

    public virtual void InitBuildingAttribute(OwnerShipType type)
    {
        buildingAttribute.InitProeprty(this, _buildingConfig, type);
        baseAttribute = buildingAttribute;
    }

    public virtual void BuildingOn()
    {
        _isBuildingOn = true;
    }


    public virtual void BuildingOFF()
    {
        _isBuildingOn = false;
    }


    public virtual void ProcessBuilding()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }
        if (!_isProcess) { return; }

        switch (buildingState.CurrentState)
        {
            case BuildingState.Ready:
                BuildingReady();
                break;
            case BuildingState.Start:
                BuildingStart();
                break;
            case BuildingState.Active:
                BuildingActive();
                break;
            case BuildingState.End:
                BuildingEnd();
                break;
            case BuildingState.Recover:
                BuildingRecover();
                break;
        }
    }

    public virtual void BuildingReady()
    {
        if (!_isBuildingOn) { return; }
        buildingState.ChangeState(BuildingState.Start);
    }

    public virtual void BuildingStart()
    {

    }

    public virtual void BuildingActive()
    {

    }

    public virtual void BuildingEnd()
    {

    }

    public virtual void BuildingRecover()
    {

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

    protected override void _DoDeSpawnEffect()
    {
        DeSpawnAllBuildingComponents();
    }

    #region Building Components

    /// <summary>
    /// ��ӽ������
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
    /// ��ȡ�������
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

    private void DeSpawnAllBuildingComponents()
    {
        if (_buildingComponents.Count <= 0)
            return;

        for (int i = 0; i < _buildingComponents.Count; i++)
        {
            _buildingComponents[i].DeSpawn();
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
