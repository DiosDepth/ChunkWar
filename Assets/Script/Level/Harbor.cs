using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harbor : LevelEntity
{
    public ShipBuilder shipbuilder;
    public override void Initialization()
    {
        base.Initialization();
        //Ïú»Ù¾ÉµÄShip
        if (RogueManager.Instance.currentShip != null)
        {
            GameObject.Destroy(RogueManager.Instance.currentShip.container.gameObject);
            RogueManager.Instance.currentShip = null;
        }
        shipbuilder.Initialization();
    }

    public override void Unload()
    {
        base.Unload();
        if (shipbuilder.editorShip != null)
        {
            GameObject.Destroy(shipbuilder.editorShip.container.gameObject);
            shipbuilder.editorShip = null;
        }
    }
}
