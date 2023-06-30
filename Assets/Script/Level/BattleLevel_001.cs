using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleLevel_001 : LevelEntity
{





    public override void Initialization()
    {
        startPoint = GameObject.Find("StartPoint").transform.position;
        cameraBoard = GameObject.Find("CameraBoard").GetComponent<PolygonCollider2D>();
    }

    public override void Unload()
    {

    }
}
