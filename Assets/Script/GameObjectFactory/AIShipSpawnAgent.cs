using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;



public class ExtraSpawnInfo
{
    public int ID;
    public WaveEnemySpawnConfig Cfg;
    public AISkillShip ownerShip;
    public AttachPointConfig PointCfg;
}


public class AIShipSpawnAgent : ShipSpawnAgent, IPoolable
{
    private List<AIShip> _shiplist = new List<AIShip>();
    private Vector2 _spawnreferencedir;

    public override void Initialization()
    {

    }

    public override void StartSpawn(ShipSpawnInfo m_spawninfo)
    {
        spawnInfo = m_spawninfo;
        AIShipSpawnInfo aispawninfo = m_spawninfo as AIShipSpawnInfo;
        Spawn(aispawninfo);
    }

    private async void Spawn(AIShipSpawnInfo m_aispawninfo)
    {
        _spawnreferencedir = GetSpawnReferenceDir();

        List<Vector2> shapeposlist = new List<Vector2>();
        shapeposlist = m_aispawninfo.shapeSetting.CalculateShapePosList(m_aispawninfo.referencePos, _spawnreferencedir, m_aispawninfo.shipConfig.GetMapSize());

        List<UniTask> tasklist = new List<UniTask>();
        for (int i = 0; i < shapeposlist.Count; i++)
        {
            //实例化所有的配置敌人ＡＩ
            CancellationTokenSource cts = new CancellationTokenSource();
            ctkLst.Add(cts);
            await UniTask.Delay((int)m_aispawninfo.shapeSetting.spawnIntervalTime * 1000, cancellationToken : cts.Token);
            tasklist.Add(CreateEntity(m_aispawninfo, shapeposlist[i]));
        }

        await UniTask.WhenAll(tasklist);

        CancellationTokenSource cts2 = new CancellationTokenSource();
        ctkLst.Add(cts2);
        await UniTask.Delay(1000, cancellationToken: cts2.Token);
        Debug.Log(string.Format("Create Enemy Success! UnitID = {0} , Count = {1}", m_aispawninfo.ID, _shiplist.Count));
        PoolableDestroy();
    }

    public void StopSpawn(UnityAction<List<AIShip>> callback)
    {
        callback?.Invoke(_shiplist);
        PoolableDestroy();
    }

    public Vector2 GetSpawnReferenceDir()
    {
        _spawnreferencedir = this.transform.position.DirectionToXY(RogueManager.Instance.currentShip.transform.position).ToVector2();
        return _spawnreferencedir;
    }

    public override void PoolableReset()
    {
        base.PoolableReset();
        _shiplist.Clear();
    }

    public override void PoolableDestroy()
    {
        base.PoolableDestroy();
    }

    public override void PoolableSetActive(bool isactive = true)
    {
        base.PoolableSetActive(isactive);
    }

    private async UniTask CreateEntity(AIShipSpawnInfo aispawninfo, Vector2 pos)
    {
        ///创建特效
        EffectManager.Instance.CreateEffect(EntitySpawnEffect, pos);
        SoundManager.Instance.PlayBattleSound("Ship/Ship_SpawnEffect", transform, 0.5f);
        await UniTask.Delay(1000);

        if (!RogueManager.Instance.IsLevelSpawnVaild())
            return;

        PoolManager.Instance.GetObjectSync(GameGlobalConfig.AIShipPath + aispawninfo.shipConfig.GetPrefabName(), true, (obj) =>
        {
            obj.transform.position = pos;
            if(aispawninfo.referenceTarget != null)
            {
                obj.transform.rotation = Quaternion.LookRotation(Vector3.forward, obj.transform.position.DirectionToXY(aispawninfo.referenceTarget.position));
            }
            var tempship = obj.GetComponent<AIShip>();
            tempship.OverrideHardLevelID = aispawninfo.hardLevelID;
            tempship.Initialization();
            _shiplist.Add(tempship);
            aispawninfo.action?.Invoke(tempship);
        }, LevelManager.AIPool);
    }
}
