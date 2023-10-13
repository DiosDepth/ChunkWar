using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAttack : AIAction
{

    private Vector3 _moveDirection;

    public float attackRange;
    public float maxRotateSpeed = 15;

    private Vector2 _aimdirection;
    public override void Initialization()
    {
        base.Initialization();

 
    }


    public override void OnEnterAction()
    {
        base.OnEnterAction();
        Attack();
    }

    public override void UpdateAction()
    {
        _aimdirection = (RogueManager.Instance.currentShip.transform.position - this.transform.position).normalized;
        //UpdateShipRotation();
        UpdateWeaponRotation();



    }
    public override void FixedUpdateAction()
    {
        
    }

    public override void OnExitAction()
    {
        base.OnExitAction();
        StopAttack();
        //_brain.seeingDic.Clear();
    }



    public void Attack()
    {
        var lst = _controller.controlledTarget.UnitList;
        for (int i = 0; i < lst.Count; i++)
        {
            var unit = lst[i];
            if (unit is Weapon)
            {
                (unit as Weapon).SetUnitProcess(true);
            }
        }
    }

    public void UpdateShipRotation()
    {
            _controller.transform.rotation = MathExtensionTools.CalculateRotation(_controller.transform.up, _aimdirection, maxRotateSpeed);
    }

    public void UpdateWeaponRotation()
    {
        Weapon tempweapon;
        var lst = _controller.controlledTarget.UnitList;
        for (int i = 0; i < lst.Count; i++)
        {
            var unit = lst[i];
            if (unit is Weapon)
            {
                tempweapon = unit as Weapon;
                if (tempweapon.rotationRoot != null)
                {
                    tempweapon.rotationRoot.rotation = MathExtensionTools.CalculateRotation(tempweapon.transform.up, _aimdirection, tempweapon.roatateSpeed);
                }
            }
        }
    }


    public void Drfting()
    {

    }

    public void StopAttack()
    {
        var lst = _controller.controlledTarget.UnitList;
        for (int i = 0; i < lst.Count; i++)
        {
            var unit = lst[i];
            if (unit is Weapon)
            {
                (unit as Weapon).WeaponOff();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!isDebug)
        {
            return;
        }
    }
}
