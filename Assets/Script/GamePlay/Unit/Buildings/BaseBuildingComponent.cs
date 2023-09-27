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
    /// �Ƴ�ʹ�ã����Ϊ���۵Ȳ�����������Ϊֱ������
    /// </summary>
    public virtual void OnRemove() { }
}
