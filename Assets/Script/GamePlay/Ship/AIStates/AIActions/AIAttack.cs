using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAttack : AIAction
{

    private Vector3 _moveDirection;
  



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

            

    }

    public override void OnExitAction()
    {
        base.OnExitAction();
        StopAttack();
        //_brain.seeingDic.Clear();
    }



    public void Attack()
    {
        Debug.Log(this.gameObject +  " + Attack@@@@@");
        for (int i = 0; i < _controller.controlledTarget.UnitList.Count; i++)
        {
            if (_controller.controlledTarget.UnitList[i] is Weapon)
            {
                (_controller.controlledTarget.UnitList[i] as Weapon).WeaponOn();
            }
        }
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
