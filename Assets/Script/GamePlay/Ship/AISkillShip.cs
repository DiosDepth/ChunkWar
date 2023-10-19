using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 带有技能更新的AIship
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
