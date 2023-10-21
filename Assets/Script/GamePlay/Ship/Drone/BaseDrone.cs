using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum DroneState
{
    Ready,
    Patrol,
    Launched,
    Battle,
    Return,
    Destroy,
    Restore,
}

public class BaseDrone : BaseShip, IPoolable
{
    protected DroneConfig _droneCfg;
    public DroneAttribute droneAttribute;
    public StateMachine<DroneState> state;
    public int DroneID;
    public OwnerType ownerType
    {
        get;
        private set;
    }

    public override void Initialization()
    {
        base.Initialization();
        state = new StateMachine<DroneState>(this.gameObject,false,false);
        CreateShip();
    }

    protected override void Death(UnitDeathInfo info)
    {
        base.Death(info);
        //DestroyAIShipBillBoard();
        ResetAllAnimation();
        LevelManager.Instance.pickupList.AddRange(Drop());
        if (!string.IsNullOrEmpty(_droneCfg.DieAudio))
        {
            SoundManager.Instance.PlayBattleSound(_droneCfg.DieAudio, transform);
        }
        ECSManager.Instance.UnRegisterJobData(OwnerType.AI, this);
        //AIManager.Instance.RemoveAI(this);
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + deathVFXName, true, (vfx) =>
        {
            vfx.transform.position = this.transform.position;
            vfx.GetComponent<ParticleController>().PoolableSetActive(true);
            vfx.GetComponent<ParticleController>().PlayVFX();
            PoolableDestroy();
        });
    }


    public override void CreateShip()
    {
        base.CreateShip();
        //初始化
        _chunkMap = new Chunk[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
        _unitList = new List<Unit>();

        DroneConfig droneCfg = DataManager.Instance.GetDroneConfig(DroneID);
        _droneCfg = droneCfg;
        baseDroneCfg = droneCfg;
        ownerType = droneCfg.Owner;
        Vector2Int pos;
        InitAttribute();

        for (int row = 0; row < droneCfg.Map.GetLength(0); row++)
        {
            for (int colume = 0; colume < droneCfg.Map.GetLength(1); colume++)
            {
                if (droneCfg.Map[row, colume] == 0)
                {
                    continue;
                }
                pos = GameHelper.CoordinateArrayToMap(new Vector2Int(row, colume), GameGlobalConfig.ShipMapSize);
                _chunkMap[row, colume] = new Chunk();
                _chunkMap[row, colume].shipCoord = pos;
                _chunkMap[row, colume].state = DamagableState.Normal;
                _chunkMap[row, colume].isBuildingPiovt = false;
                _chunkMap[row, colume].isOccupied = false;
            }
        }
        //处理unit
        _unitList = buildingsParent.GetComponentsInChildren<Unit>(true).ToList<Unit>();
        BaseUnitConfig unitconfig;
        for (int i = 0; i < _unitList.Count; i++)
        {
            var unit = _unitList[i];
            unitconfig = DataManager.Instance.GetUnitConfig(_unitList[i].UnitID);
            unit.gameObject.SetActive(true);

            unit.Initialization(this, unitconfig);
            unit.SetUnitProcess(true);
            if (unit.IsCoreUnit)
            {
                CoreUnits.Add(unit);
            }
        }

        DoSpawnEffect();
    }

    public void PoolableReset()
    {

    }

    public void PoolableDestroy()
    {

        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }

    private void InitAttribute()
    {
        droneAttribute.InitProeprty(this, _droneCfg);
    }

    protected const string AnimTrigger_Spawn = "Spawn";
    private async void DoSpawnEffect()
    {
        _spriteMat.EnableKeyword(Mat_Shader_PropertyKey_HOLOGRAM_ON);
        SetAnimatorTrigger(AnimTrigger_Spawn);
        ///UnitSpawn
        for (int i = 0; i < _unitList.Count; i++)
        {
            if (_unitList[i].isActiveAndEnabled)
            {
                _unitList[i].DoSpawnEffect();
            }
        }

        var length = GameHelper.GetAnimatorClipLength(_spriteAnimator, "EnemyShip_Spawn");
        await UniTask.Delay((int)(length * 1000));
        _spriteMat.DisableKeyword(Mat_Shader_PropertyKey_HOLOGRAM_ON);
    }

    private void ResetAllAnimation()
    {
        _spriteMat.DisableKeyword(Mat_Shader_PropertyKey_HOLOGRAM_ON);
        _spriteAnimator.ResetTrigger(AnimTrigger_Spawn);
    }
}
