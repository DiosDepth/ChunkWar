using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAttack : AIAction
{

    private Vector3 _moveDirection;



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
        UpdateShipRotation();
        UpdateWeaponRotation();



    }

    public override void OnExitAction()
    {
        base.OnExitAction();
        StopAttack();
        //_brain.seeingDic.Clear();
    }



    public void Attack()
    {
  
        for (int i = 0; i < _controller.controlledTarget.UnitList.Count; i++)
        {
            if (_controller.controlledTarget.UnitList[i] is Weapon)
            {
                (_controller.controlledTarget.UnitList[i] as Weapon).SetUnitProcess(true);
            }
        }
    }

    public void UpdateShipRotation()
    {
            _controller.transform.rotation = MathExtensionTools.CalculateRotation(_controller.transform.up, _aimdirection, _controller.maxRotateSpeed);
    }

    public void UpdateWeaponRotation()
    {
        Weapon tempweapon;
        for (int i = 0; i < _controller.controlledTarget.UnitList.Count; i++)
        {
            if (_controller.controlledTarget.UnitList[i] is Weapon)
            {
                tempweapon = _controller.controlledTarget.UnitList[i] as Weapon;
                if (tempweapon.rotationRoot != null)
                {
                    tempweapon.rotationRoot.rotation = MathExtensionTools.CalculateRotation(_controller.controlledTarget.UnitList[i].transform.up, _aimdirection, tempweapon.roatateSpeed);
                }
            }
        }
    }


    public void Drfting()
    {

    }

    public void StopAttack()
    {
        for (int i = 0; i < _controller.controlledTarget.UnitList.Count; i++)
        {
            if (_controller.controlledTarget.UnitList[i] is Weapon)
            {
                (_controller.controlledTarget.UnitList[i] as Weapon).WeaponOff();
            }
        }
    }



    // Start is called before the first frame update
    protected override void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (!isDebug)
        {
            return;
        }
    }
}
