using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum DroneState
{
    Ready = 0,
    Patrol = 1,
    Launched = 2,
    Battle = 3,
    Return = 4,
    Landed = 5,
    Destroy = 6,
    Restore = 9,
}

public class BaseDrone : BaseShip, IPoolable
{

    public DroneConfig droneCfg;
    public DroneAttribute droneAttribute;
    public StateMachine<DroneState> baseDroneState;

    public int DroneID;

    public OwnerType ownerType
    {
        get;
        private set;
    }

    protected DroneFactory _ownerFactory;

    protected override void Awake()
    {
        base.Awake();
        if (_appearMat == null)
        {
            _appearMat = Instantiate(_sharedMat);
        }
    }

    public override void Initialization()
    {
        base.Initialization();
        baseDroneState = new StateMachine<DroneState>(this.gameObject,false,false);
        baseDroneState.ChangeState(DroneState.Ready);
        CreateShip();

    }

    public virtual void SetOwnerFactory(DroneFactory factory)
    {
        _ownerFactory = factory;
    }
    public override void Death(UnitDeathInfo info)
    {
        base.Death(info);
        OnDeath();
    }

    public override void ForceKill()
    {
        base.ForceKill();
        OnDeath();
    }

    private void OnDeath()
    {
        if (!string.IsNullOrEmpty(droneCfg.DieAudio))
        {
            SoundManager.Instance.PlayBattleSound(droneCfg.DieAudio, transform);
        }
        //ECSManager.Instance.UnRegisterJobData(OwnerType.AI, this);
        EffectManager.Instance.CreateEffect(droneCfg.DeathEffect, transform.position);
    }

    public override void CreateShip()
    {
        base.CreateShip();
        //初始化
        _chunkMap = new Chunk[GameGlobalConfig.ShipMaxSize, GameGlobalConfig.ShipMaxSize];
        _unitList = new List<Unit>();

        DroneConfig droneCfg = DataManager.Instance.GetDroneConfig(DroneID);
        this.droneCfg = droneCfg;
        baseShipCfg = droneCfg;
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

    public override void GameOver()
    {
        base.GameOver();
        PoolableDestroy();
    }
    public void PoolableReset()
    {
        for (int i = 0; i < _unitList.Count; i++)
        {
            _unitList[i].SetDisable();
        }
        _ownerFactory = null;
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }
    
    public virtual void Launch()
    {
        
        PoolableSetActive(true);
    }
    public virtual void Landing()
     {
        _ownerFactory.Landing(this);
        PoolableSetActive(false);
    }
    public virtual void Crash(UnitDeathInfo info)
    {
        Death(info);

        _ownerFactory.Crash(this);
        PoolableSetActive(false);


    }
    public virtual void Repair()
    {
        baseDroneState.ChangeState(DroneState.Ready);
        _unitList = buildingsParent.GetComponentsInChildren<Unit>(true).ToList<Unit>();
        for (int i = 0; i < _unitList.Count; i++)
        {
            var unit = _unitList[i];
            //unit.gameObject.SetActive(true);

            unit.Restore();
            //unit.SetUnitProcess(true);
            if (unit.IsCoreUnit)
            {
                CoreUnits.Add(unit);
            }
        }
        
        PoolableSetActive(false);
    }


    public override bool CheckDeath(Unit coreUnit, UnitDeathInfo info)
    {
        CoreUnits.Remove(coreUnit);
        if (CoreUnits.Count <= 0)
        {
            Crash(info);
            return true;
        }
        return false;
    }


    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }

    private void InitAttribute()
    {
        droneAttribute.InitProeprty(this, droneCfg);
    }

    protected const string AnimTrigger_Spawn = "Spawn";
    protected const string AnimTrigger_DeSpawn = "DeSpawn";

    public async void DoSpawnEffect()
    {
        _render.material = _appearMat;
        SetAnimatorTrigger(AnimTrigger_Spawn);
        var length = GameHelper.GetAnimatorClipLength(_spriteAnimator, "EnemyShip_Spawn");
        await UniTask.Delay((int)(length * 1000));
        _appearMat.SetFloat(Mat_Shader_PropertyKey_HOLOGRAM_ON, 0);
        ///Spawn Effect Finish
        _render.material = _sharedMat;
    }

    /// <summary>
    /// 消失动画
    /// </summary>
    public void DoDeSpawnEffect()
    {
        _render.material = _appearMat;
        SetAnimatorTrigger(AnimTrigger_DeSpawn);
    }

    protected override void ResetAllAnimation()
    {
        if(_appearMat != null)
        {
            _appearMat.DisableKeyword(Mat_Shader_PropertyKey_HOLOGRAM_ON);
        }
        if(_spriteAnimator != null)
        {
            _spriteAnimator.ResetTrigger(AnimTrigger_Spawn);
        }
   
    }
}
