using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum OwnerType
{
    None,
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
public class BaseShip : MonoBehaviour, IPauseable,IGame
{
    /// <summary>
    /// ID
    /// </summary>
    public int ShipID
    {
        get { return baseShipCfg.ID; }
    }

    public BaseController controller;
    protected GameObject buildingsParent;
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
    protected Material _sharedMat;
    protected static Material _appearMat;
    protected SpriteRenderer _render;

    public virtual void Initialization()
    {
        GameManager.Instance.RegisterPauseable(this);
        controller = this.GetComponent<BaseController>();
        if (controller == null)
        {
            Debug.LogError(this.gameObject.name + " missing controller component");
        }
        else
        {
            controller.Initialization();
        }

    }

    public BaseShip GetFirstTarget()
    {
        if (_unitList.Count == 0) { return null; }
        if (_unitList[0].targetList.Count == 0) { return null; }

        BaseShip baseship;
        baseship = _unitList[0].targetList[0]?.target.GetComponent<Unit>()._owner;

        return baseship;
    }

    public UnitTargetInfo GetFirstTargetInfo()
    {
        if (_unitList.Count == 0) { return null; }
        if (_unitList[0].targetList.Count == 0) { return null; }

        UnitTargetInfo info;
        info = _unitList[0].targetList[0];

        return info;
    }


    public void SetFirstTargetInfo(UnitTargetInfo unitinfo)
    {
        for (int i = 0; i < _unitList.Count; i++)
        {
         
           if( _unitList[i].targetList.Count == 0)
            {
                _unitList[i].targetList.Add(unitinfo);
            }
            if (_unitList[i].targetList[0] == unitinfo)
            {
                continue;
            }   
            else
            {
                _unitList[i].targetList[0] = unitinfo;
            }

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
        _spriteAnimator = transform.Find("Sprite").SafeGetComponent<Animator>();
        _render = transform.Find("Sprite").SafeGetComponent<SpriteRenderer>();
        if(_render != null)
        {
            _sharedMat = _render.sharedMaterial;
        }


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

    public virtual bool CheckDeath(Unit coreUnit, UnitDeathInfo info)
    {
        CoreUnits.Remove(coreUnit);
        if(CoreUnits.Count <= 0)
        {
            Death(info);
            return true;
        }
        return false;
    }

    public virtual void Death(UnitDeathInfo info)
    {
        GameManager.Instance.UnRegisterPauseable(this);
        conditionState.ChangeState(ShipConditionState.Death);
        ShipStateEvent.Trigger(this, info, movementState.CurrentState, conditionState.CurrentState, this is PlayerShip);
    }

    /// <summary>
    /// 强制死亡
    /// </summary>
    public virtual void ForceKill()
    {
        UnitDeathInfo info = new UnitDeathInfo
        {
            isCriticalKill = false
        };
        GameManager.Instance.UnRegisterPauseable(this);
        conditionState.ChangeState(ShipConditionState.Death);
        ShipStateEvent.Trigger(this, info, movementState.CurrentState, conditionState.CurrentState, this is PlayerShip);
    }


    public virtual void InitProperty()
    {

    }

    public virtual void GameOver()
    {

    }
    public virtual void PauseGame()
    {
      
    }

    public virtual void UnPauseGame()
    {

    }

    #region Anim

    protected const string Mat_Shader_PropertyKey_HOLOGRAM_ON = "_HologramBlend";
    protected const string Mat_Shader_ProeprtyKey_OUTBASE_ON = "_OutlineAlpha";

    protected virtual void ResetAllAnimation()
    {

    }

    public void SetAnimatorTrigger(string trigger)
    {
        if(_spriteAnimator == null) { return; }
        _spriteAnimator.SetTrigger(trigger);
    }

    public void ResetAnimatorTrigger(string trigger)
    {
        if (_spriteAnimator == null) { return; }
        _spriteAnimator.ResetTrigger(trigger);
    }

    #endregion
}
