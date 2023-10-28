using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDroneFactory : DroneFactory
{
    public override void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        base.Initialization(m_owner, m_unitconfig);
    }

    /// <summary>
    /// 初始化核心信息
    /// </summary>
    public void InitCoreData()
    {
        HpComponent.BindHPChangeAction(OnPlayerCoreHPChange, false, 0, 0);
    }

    private void OnPlayerCoreHPChange(int oldValue, int newValue)
    {
        ShipPropertyEvent.Trigger(ShipPropertyEventType.CoreHPChange);
        var percent = HpComponent.HPPercent;
        LevelManager.Instance.OnPlayerShipCoreHPPercentChange(percent, oldValue, newValue);
    }
}
