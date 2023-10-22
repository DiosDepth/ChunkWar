using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseBuildingComponent 
{
    public BaseShip OwnerShip;
    public Unit ParentUnit;

    public virtual void OnInit(BaseShip owner, Unit parentUnit)
    {
        this.OwnerShip = owner;
        this.ParentUnit = parentUnit;
    }
    
    public virtual void OnUpdate()
    {

    }

    /// <summary>
    /// 移除使用，玩家为出售等操作、敌人则为直接销毁
    /// </summary>
    public virtual void OnRemove() { }

    /// <summary>
    /// 移除效果
    /// </summary>
    public virtual void DeSpawn() { }
}
