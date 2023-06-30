using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harbor : LevelEntity
{
    public ShipBuilder shipbuilder;
    public override void Initialization()
    {
        base.Initialization();
        shipbuilder.Initialization();
    }

    public override void Unload()
    {
        base.Unload();
        if(shipbuilder.editorShip != null)
        {
            GameObject.Destroy(shipbuilder.editorShip.container.gameObject);
            shipbuilder.editorShip = null;
        }
    }
}
