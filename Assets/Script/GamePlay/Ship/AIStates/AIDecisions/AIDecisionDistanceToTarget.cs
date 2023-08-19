using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public enum CompareType
{
    Greater,
    GreaterEqula,
    Equla,
    LessEqula,
    Less,
    NonEqula,
}

public class AIDecisionDistanceToTarget : AIDecision
{

    public Transform target;
    public CompareType compare = CompareType.GreaterEqula;
    public float thresholdDistance;

    public override void Initialization()
    {
        base.Initialization();

    }

    public override void OnEnterDecision()
    {
        base.OnEnterDecision();



    }

    public override bool Decide()
    {
        return DistanceToTarget();

    }

    public override void OnExitDecision()
    {
        base.OnExitDecision();
    }

    public bool DistanceToTarget()
    {
        if (target == null)
        {
            GetTarget();
            if (target == null)
                return false;
        }
        bool result = false;
        switch (compare)
        {
            case CompareType.Greater:
                if (transform.position.SqrDistanceXY(target.position) > Mathf.Pow(thresholdDistance, 2))
                {
                    result = true;
                }
                break;
            case CompareType.GreaterEqula:
                if (transform.position.SqrDistanceXY(target.position) >= Mathf.Pow(thresholdDistance, 2))
                {
                    result = true;
                }
                break;
            case CompareType.Equla:
                if (transform.position.EqualXY(target.position))
                {
                    result = true;
                }
                break;
            case CompareType.LessEqula:
                if (transform.position.SqrDistanceXY(target.position) <= Mathf.Pow(thresholdDistance, 2))
                {
                    result = true;
                }
                break;
            case CompareType.Less:
                if (transform.position.SqrDistanceXY(target.position) < Mathf.Pow(thresholdDistance, 2))
                {
                    result = true;
                }
                break;
            case CompareType.NonEqula:
                if (transform.position.SqrDistanceXY(target.position) != Mathf.Pow(thresholdDistance, 2))
                {
                    result = true;
                }
                break;
        }

        return result;
    }

    public void GetTarget()
    {
        if(RogueManager.Instance.currentShip != null)
        {
            target = RogueManager.Instance.currentShip.transform;
        }
        else
        {
            target = null;
        }
   
    }

}
