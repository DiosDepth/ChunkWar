using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * MainLevel
 */
public class BattleLevel : LevelEntity
{
    private LevelTimer _timer;

    public GameObject BulletPool;
    public GameObject AIPool;

    protected override void Update()
    {
        base.Update();
        _timer.OnUpdate();
    }


    public override void Initialization()
    {
        BulletPool = new GameObject("BulletPool");
        AIPool = new GameObject("AIPool");
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
