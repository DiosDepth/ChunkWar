using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarborTeleport : PickableItem
{


    // Start is called before the first frame update
    protected override void Start()
    {

    }

    public override void PickUp(GameObject picker)
    {
        base.PickUp(picker);
        AfterPickUp(picker);
    }

    protected override void AfterPickUp(GameObject picker)
    {
        GameStateTransitionEvent.Trigger(EGameState.EGameState_GameHarbor);
        base.AfterPickUp(picker);

    }

    protected override void OnTriggerStay2D(Collider2D collider)
    {
        base.OnTriggerStay2D(collider);

        if (collider.tag == "Player")
        {
            if (transform.position.EqualXY(collider.transform.position, 0.1f))
            {
                PickUp(collider.gameObject);
            }
        }
    }
}
