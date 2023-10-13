using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * MainLevel
 */
public class BattleLevel : LevelEntity
{
    private LevelTimer _timer;

    public GameObject IndicatorPool;

    protected override void Update()
    {
        base.Update();
        _timer.OnUpdate();
    }


    public override void Initialization()
    {
        IndicatorPool = new GameObject("IndicatorPool");
        startPoint = GameObject.Find("StartPoint").transform.position;

        cameraBoard = GameObject.Find("CameraBoard").GetComponent<PolygonCollider2D>();
        _timer = RogueManager.Instance.Timer;
    }

    public override void Unload()
    {
        _timer.Pause();
        RogueManager.Instance.OnMainLevelUnload();
    }

}
