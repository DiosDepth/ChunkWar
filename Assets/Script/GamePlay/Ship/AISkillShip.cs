using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ���м��ܸ��µ�AIship
 */
public class AISkillShip : AIShip
{
    public override void Initialization()
    {
        base.Initialization();
        InitAIShipSkill();
    }

    private void Update()
    {
        if (GameManager.Instance.IsPauseGame())
            return;

    }

    private void InitAIShipSkill()
    {

    }
}
