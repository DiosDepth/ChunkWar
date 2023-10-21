using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum OwnerType
{
    Player,
    AI,
}

public struct ShipStateEvent
{
    public BaseShip Ship;
    public ShipMovementState movementState;
    public ShipConditionState conditionState;
    public bool IsPlayer;
    public bool MovementChange;

    public UnitDeathInfo KillInfo;

    public ShipStateEvent(BaseShip ship, UnitDeathInfo info, ShipMovementState m_movmentstate , ShipConditionState m_conditionstate, bool isPlayer, bool movementChange = false)
    {
        Ship = ship;
        movementState = m_movmentstate;
        conditionState = m_conditionstate;
        IsPlayer = isPlayer;
        MovementChange = movementChange;
        KillInfo = info;
    }

    public static ShipStateEvent e;
    public static void Trigger(BaseShip ship, UnitDeathInfo info, ShipMovementState m_movmentstate, ShipConditionState m_conditionstate, bool isPlayer, bool movementChange = false)
    {
        e.Ship = ship;
        e.movementState = m_movmentstate;
        e.conditionState = m_conditionstate;
        e.IsPlayer = isPlayer;
        e.MovementChange = movementChange;
        e.KillInfo = info;
        EventCenter.Instance.TriggerEvent<ShipStateEvent>(e);
    }
}

[ShowOdinSerializedPropertiesInInspector]
public class BaseShip : MonoBehaviour, IPauseable
{

    public BaseController controller;
    protected GameObject buildingsParent;
    public string deathVFXName = "ExplodeVFX";
    

    public StateMachine<ShipMovementState> movementState;
    public StateMachine<ShipConditionState> conditionState;


    public Chunk[,] ChunkMap { set { _chunkMap = value; } get { return _chunkMap; } }

    protected Chunk[,] _chunkMap;

    /// <summary>
    /// 核心Units
    /// </summary>
    public List<Unit> CoreUnits;

    public List<Unit> UnitList { set { _unitList = value; } get { return _unitList; } }
    [ShowInInspector]
    [ListDrawerSettings(DraggableItems = true)]
    protected List<Unit> _unitList = new List<Unit>();

    protected BaseShipConfig baseShipCfg;

    protected Animator _spriteAnimator;
    protected Material _spriteMat;

    public virtual void Initialization()
    {
        GameManager.Instance.RegisterPauseable(this);
        controller = this.GetComponent<BaseController>();
        _spriteAnimator = transform.Find("Sprite").SafeGetComponent<Animator>();
        _spriteMat = transform.Find("Sprite").SafeGetComponent<SpriteRenderer>().material;
        if (controller == null)
        {
            Debug.LogError(this.gameObject.name + " missing controller component");
        }
        else
        {
            controller.Initialization();
        }

    }

    /// <summary>
    /// 获取所有武器
    /// </summary>
    /// <returns></returns>
    /// 

    public List<T> GetAllShipUnitByType<T>() where T : Unit
    {
        return _unitList.FindAll(x => x is T).ConvertAll(x => x as T);
    }

    /// <summary>
    /// 移除Unit
    /// </summary>
    /// <param name="unit"></param>
    public virtual void RemoveUnit(Unit unit)
    {
        _unitList.Remove(unit);
    }

    public Unit GetUnitBySlotPosition(Vector2Int pos)
    {
        for(int i = 0; i < _unitList.Count; i++)
        {
            var coor = _unitList[i].occupiedCoords;
            if (coor.Contains(pos))
                return _unitList[i];
        }
        return null;
    }

    protected virtual void Awake()
    {
        CoreUnits = new List<Unit>();
        buildingsParent = transform.Find("Buildings").gameObject;
        if (movementState == null)
        {
            movementState = new StateMachine<ShipMovementState>(this.gameObject, false, false);
        }
        if (conditionState == null)
        {
            conditionState = new StateMachine<ShipConditionState>(this.gameObject, false, false);
        }
    }

    protected virtual void OnDestroy()
    {
        GameManager.Instance.UnRegisterPauseable(this);
        for (int i = 0; i < _unitList.Count; i++)
        {
            Destroy(_unitList[i].gameObject);
        }
    }

    public virtual void CreateShip()
    {

    }

    public virtual void ResetShip()
    {

    }

    /// <summary>
    /// 设置所有部件状态
    /// </summary>
    public async void ForeceSetAllUnitState(DamagableState state, float time = -1, bool playimmortalEffect = false)
    {
        for(int i = 0; i < _unitList.Count; i++)
        {
            _unitList[i].ChangeUnitState(state);
        }

        if (time > 0) 
        {
            await UniTask.Delay((int)(time * 1000));
            if (gameObject == null)
                return;

            for (int i = 0; i < _unitList.Count; i++)
            {
                var unit = _unitList[i];
                if (unit == null)
                    continue;

                _unitList[i].ChangeUnitState(DamagableState.Normal);
            }
        }
    }

    public void CheckDeath(Unit coreUnit, UnitDeathInfo info)
    {
        CoreUnits.Remove(coreUnit);
        if(CoreUnits.Count <= 0)
        {
            Death(info);
        }
    }

    protected virtual void Death(UnitDeathInfo info)
    {
        GameManager.Instance.UnRegisterPauseable(this);
        conditionState.ChangeState(ShipConditionState.Death);
        ShipStateEvent.Trigger(this, info, movementState.CurrentState, conditionState.CurrentState, this is PlayerShip);
    }

    public virtual void InitProperty()
    {

    }

    public virtual void GameOver()
    {
        for (int i = 0; i < CoreUnits.Count; i++)
        {
            CoreUnits[i].GameOver();
        }

    }
    public virtual void PauseGame()
    {
      
    }

    public virtual void UnPauseGame()
    {

    }

    #region Anim

    protected const string Mat_Shader_PropertyKey_HOLOGRAM_ON = "HOLOGRAM_ON";
    protected const string Mat_Shader_ProeprtyKey_OUTBASE_ON = "OUTBASE_ON";

    public void SetAnimatorTrigger(string trigger)
    {
        _spriteAnimator.SetTrigger(trigger);
    }

    public void ResetAnimatorTrigger(string trigger)
    {
        _spriteAnimator.ResetTrigger(trigger);
    }

    #endregion
}
