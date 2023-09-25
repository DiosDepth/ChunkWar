using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Building : Unit
{

    private List<BaseBuildingComponent> _buildingComponents = new List<BaseBuildingComponent>();

    protected BuildingConfig _buildingConfig;

    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        UpdateBuildingComponents();
    }

    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        base.Initialization(m_owner, m_unitconfig);
        _buildingConfig = m_unitconfig as BuildingConfig;
        InitBuildingComponent();
    }

    public override bool TakeDamage(ref DamageResultInfo info)
    {
        return base.TakeDamage(ref info);
    }

    public override void Death()
    {
        base.Death();
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

            var shieldItem = ResManager.Instance.Load<GameObject>("Prefab/Unit/Items/PlayerShieldItem_001");
            shieldItem.transform.SetParent(transform);
            shieldItem.transform.localPosition = Vector3.zero;
            MonoShield shield = shieldItem.GetComponent<MonoShield>();
            shield.InitShield(shieldCmpt, _owner);
            shieldCmpt.monoShield = shield;
        }
    }

  
}
