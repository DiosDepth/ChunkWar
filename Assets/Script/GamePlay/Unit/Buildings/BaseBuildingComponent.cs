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
    /// �Ƴ�ʹ�ã����Ϊ���۵Ȳ�����������Ϊֱ������
    /// </summary>
    public virtual void OnRemove() { }
}
