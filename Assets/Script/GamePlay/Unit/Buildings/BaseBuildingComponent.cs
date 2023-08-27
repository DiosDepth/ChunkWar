using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseBuildingComponent 
{
    public BaseShip OwnerShip;

    public virtual void OnInit(BaseShip owner)
    {
        this.OwnerShip = owner;
    }
    
    public virtual void OnUpdate()
    {

    }

    /// <summary>
    /// 移除使用，玩家为出售等操作、敌人则为直接销毁
    /// </summary>
    public virtual void OnRemove() { }
}
