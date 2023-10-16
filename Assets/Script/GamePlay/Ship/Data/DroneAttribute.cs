using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 无人机属性
 */
[System.Serializable]
public class DroneAttribute 
{

    public float MaxVelocity
    {
        get;
        protected set;
    }

    public float MaxAcceleration
    {
        get;
        protected set;
    }

    private float MaxVelocityBase;
    private float MaxAccelerationBase;

    private BaseDrone owner;
    private UnitPropertyData _mainProperty;

    public void InitProeprty(BaseDrone _owner, DroneConfig cfg)
    {
        this.owner = _owner;
        _mainProperty = RogueManager.Instance.MainPropertyData;
        MaxVelocityBase = cfg.MaxVelocity;
        MaxAccelerationBase = cfg.MaxAcceleration;

        if(owner.ownerType == OwnerType.Player)
        {
            _mainProperty.BindPropertyChangeAction(PropertyModifyKey.Aircraft_Speed, CalculateSpeed);
            CalculateSpeed();
        }
        else
        {
            MaxVelocity = MaxVelocityBase;
            MaxAcceleration = MaxAccelerationBase;
        }
    }

    public void Destroy()
    {
        if (owner.ownerType == OwnerType.Player)
        {
            _mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.Aircraft_Speed, CalculateSpeed);
        }
    }

    private void CalculateSpeed()
    {
        var speedRatio = _mainProperty.GetPropertyFinal(PropertyModifyKey.Aircraft_Speed);
        speedRatio = Mathf.Max(-100, speedRatio);
        MaxVelocity = MaxVelocityBase * (1 + speedRatio / 100f);
        MaxAcceleration = MaxAccelerationBase * (1 + speedRatio / 100f);
    }
}
