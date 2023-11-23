using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


public struct UnitTargetJobData : IComparable<UnitTargetJobData>
{
    public int targetIndex;
    public float distanceToTarget;
    public float3 targetPos;
    public float3 targetDirection;

    public int CompareTo(UnitTargetJobData other)
    {
        return distanceToTarget.CompareTo(other.distanceToTarget);
    }
}
public class Unit : MonoBehaviour, IDamageble, IPropertyModify, IPauseable, IOtherTarget,IGame
{
    public int UnitID;
    /// <summary>
    /// UniqueID
    /// </summary>
    public uint UID { get; set; }

    public Vector2 Position
    {
        get { return transform.position; }
    }

    public PropertyModifyCategory Category { get { return PropertyModifyCategory.ShipUnit; } }

    public string GetName
    {
        get { return LocalizationManager.Instance.GetTextValue(_baseUnitConfig.GeneralConfig.Name); }
    }


    [SerializeField]
    public DamagableState damageState
    {
        get;
        private set;
    }

    /// <summary>
    /// 不可见unit,优化性能
    /// </summary>
    public bool IsInvisiableUnit = false;

    public bool IsCoreUnit = false;
    public string deathVFXName = "ExplodeVFX";
    public List<UnitTargetInfo> targetList = new List<UnitTargetInfo>();
    public int maxTargetCount = 3;

    public SpriteRenderer unitSprite;
    public Transform rotationRoot;
    public bool redirection = true;
    public int direction = 0;
    public Vector2Int pivot;
    public List<Vector2Int> occupiedCoords;
    public List<Vector2Int> effectSlotCoords;

    public UnitLocalPropertyData LocalPropetyData = new UnitLocalPropertyData();

    private List<ModifyTriggerData> _modifyTriggerDatas = new List<ModifyTriggerData>();
    public List<ModifyTriggerData> AllTriggerDatas
    {
        get { return _modifyTriggerDatas; }
    }

    private List<PropertyModifySpecialData> _modifySpecialDatas = new List<PropertyModifySpecialData>();

    public OwnerType GetOwnerType
    {
        get
        {
            if(_owner is AIShip)
            {
                return OwnerType.AI;
            }
            if(_owner is PlayerShip)
            {
                return OwnerType.Player;
            }
            return OwnerType.None;
        }
    }
    public BaseShip _owner
    {
        get;
        protected set;
    }

    public bool IsProcess { get { return _isProcess; } }
    protected bool _isProcess = true;

    /// <summary>
    /// 正在瘫痪恢复
    /// </summary>
    private bool _isParalysising = false;
    private float _paralysisTimer = 0;

    public UnitBaseAttribute baseAttribute;

    public BaseUnitConfig _baseUnitConfig
    {
        get;
        protected set;
    }
    /// <summary>
    /// 血量管理组件
    /// </summary>
    public GeneralHPComponet HpComponent;

    protected Material _sharedMat;
    protected static Material _appearMat;
    protected static Material _playerHighlightMat;
    protected Animator _animator;
    protected Transform _uiCanvas;
    private UnitSceneHPSlider _sceneHPSlider;

    public virtual void Awake()
    {
        if(unitSprite != null && !IsInvisiableUnit)
        {
            _animator = unitSprite.transform.SafeGetComponent<Animator>();
        }
        _uiCanvas = transform.Find("UICanvas");
        _uiCanvas.SafeSetActive(false);
    }

    /// <summary>
    /// 更改State
    /// </summary>
    /// <param name="target"></param>
    public virtual void ChangeUnitState(DamagableState target)
    {
        if (damageState == target)
            return;

        if(damageState == DamagableState.Paralysis && target == DamagableState.Normal)
        {
            if(_owner is PlayerShip || _owner is BaseDrone)
            {
                ///瘫痪恢复
                ECSManager.Instance.RegisterJobData(OwnerType.Player, this);
                //(RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.AddActiveUnit(this);
                LevelManager.Instance.PlayerUnitParalysis(UID, false); 
            }
        }

        damageState = target;
        if (target == DamagableState.Paralysis)
        {
            OnEnterParalysisState();
            if (_owner is PlayerShip)
            {
                LevelManager.Instance.PlayerUnitParalysis(UID, true);
                ///移除目标选中
                ECSManager.Instance.UnRegisterJobData(OwnerType.Player, this);
                //if (_baseUnitConfig.unitType == UnitType.Weapons || _baseUnitConfig.unitType == UnitType.Buildings)
                //{
                //    (RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.RemoveActiveUnit(this);
                //}
            }
        }
    }

    public virtual void Death(UnitDeathInfo info)
    {
        GameManager.Instance.UnRegisterPauseable(this);
        ChangeUnitState(DamagableState.Destroyed);

        bool ownerDeath = false;


        if (_owner is PlayerShip)
        {
            ///玩家死亡不销毁，主要处理mainBuilding
            RogueManager.Instance.MainPropertyData.UnBindPropertyChangeAction(PropertyModifyKey.HP, OnMaxHPChangeAction);
            ECSManager.Instance.UnRegisterJobData(OwnerType.Player, this);
            return;
        }
        if (_owner is AIShip)
        {
            ECSManager.Instance.UnRegisterJobData(OwnerType.AI, this);
        }


        this.gameObject.SetActive(false);
        ///Remove Owner
        _owner.RemoveUnit(this);
        SetDisable();

        if (IsCoreUnit)
        {
            ownerDeath = _owner.CheckDeath(this, info);
        }

        if (!ownerDeath)
        {
            EffectManager.Instance.CreateEffect(deathVFXName, transform.position);
        }

    }

    /// <summary>
    /// 整船死亡时调用
    /// </summary>
    public virtual void SetDisable()
    {
        RemoveHPBar();
        SetUnitProcess(false);
        HpComponent.Clear();
        OnRemove();
    }

    public virtual void Restore()
    {
        if (_owner is AIShip)
        {
            ECSManager.Instance.RegisterJobData(OwnerType.AI, this);
            SetUnitProcess(true);
            //AIManager.Instance.AddSingleUnit(this);
        }
        if (_owner is PlayerShip)
        {
            ECSManager.Instance.RegisterJobData(OwnerType.Player, this);
            SetUnitProcess(true);
            //AIManager.Instance.AddTargetUnit(this);
            //(RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.AddActiveUnit(this);
        }

        ChangeUnitState(DamagableState.Normal);
        HpComponent.RecoverHPToMax();
     
    }

    protected virtual void OnDestroy()
    {
        if(_owner is PlayerShip)
        {
            RogueManager.Instance.MainPropertyData.UnBindPropertyChangeAction(PropertyModifyKey.HP, OnMaxHPChangeAction);
        }
        GameManager.Instance.UnRegisterPauseable(this);
    }

    public virtual void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        _baseUnitConfig = m_unitconfig;
        _owner = m_owner;
        InitMaterial();
        HpComponent = new GeneralHPComponet(baseAttribute.HPMax, baseAttribute.HPMax);
        RogueManager.Instance.MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.HP, OnMaxHPChangeAction);

        if (_owner is AIShip)
        {
            ECSManager.Instance.RegisterJobData(OwnerType.AI, this);
        }
        if (_owner is PlayerShip || _owner is BaseDrone)
        {
            ECSManager.Instance.RegisterJobData(OwnerType.Player, this);
            //(RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.AddActiveUnit(this);
            SetUnitProcess(true);
        }
        GameManager.Instance.RegisterPauseable(this);
        ChangeUnitState(DamagableState.Normal);

    }

    /// <summary>
    /// Player Create Use
    /// </summary>
    /// <param name="ship"></param>
    /// <param name="m_unitconfig"></param>
    public virtual void InitializationEditorUnit(PlayerEditShip ship, BaseUnitConfig m_unitconfig)
    {
        _owner = ship;
        _baseUnitConfig = m_unitconfig;
        RogueManager.Instance.MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.HP, OnMaxHPChangeAction);
        InitMaterial();
    }

    /// <summary>
    /// Update Only Player Units
    /// </summary>
    public virtual bool OnUpdateBattle()
    {
        if (_isParalysising)
        {
            _paralysisTimer += Time.deltaTime;
            if(_paralysisTimer >= baseAttribute.UnitParalysisRecoverTime)
            {
                OnParalysisStateFinish();
            }
            return false;
        }

        if (damageState == DamagableState.Normal)
        {
            ///UpdateTrigger
            for (int i = 0; i < _modifyTriggerDatas.Count; i++)
            {
                _modifyTriggerDatas[i].OnUpdateBattle();
            }
        }
        return true;
    }

    /// <summary>
    /// 处理OnAdd 属性 & 机制
    /// </summary>
    public void OnAdded()
    {
        AddModifySpecials();
        AddModifyTriggers();
    }

    /// <summary>
    /// 处理OnRemove 属性 & 机制
    /// </summary>
    public virtual void OnRemove()
    {
        ResetAllAnimation();
        baseAttribute.Destroy();
        LocalPropetyData.Clear();
        _modifyTriggerDatas.ForEach(x => x.OnTriggerRemove());
        _modifySpecialDatas.ForEach(x => x.OnRemove());
        targetList.Clear();
    }

    public void OutLineHighlight(bool highlight)
    {
        if (highlight)
        {
            unitSprite.material = _playerHighlightMat;
            var color = GameHelper.GetRarityColor(_baseUnitConfig.GeneralConfig.Rarity);
            _playerHighlightMat.SetFloat(Mat_Shader_PropertyKey_OUTLINE, 1);
            _playerHighlightMat.SetColor("_OutlineColor", color);
        }
        else
        {
            _playerHighlightMat.SetFloat(Mat_Shader_PropertyKey_OUTLINE, 0);
            unitSprite.material = _sharedMat;
        }
        
    }

    /// <summary>
    /// 治疗，增加HP
    /// </summary>
    /// <param name="value"></param>
    public virtual void Heal(int value)
    {
        if (HpComponent == null)
            return;

        if (damageState == DamagableState.Destroyed || damageState == DamagableState.Paralysis)
            return;

        HpComponent.ChangeHP(value);
    }

    public virtual bool TakeDamage(DamageResultInfo info)
    {
        if (HpComponent == null)
            return false;
        //已经死亡或者瘫痪的不会收到更多伤害
        if(damageState == DamagableState.Destroyed || damageState == DamagableState.Paralysis)
        {
            return false;
        }

        ///Calculate DamageReduce
        var damageReduceRatio = LocalPropetyData.GetPropertyFinal(UnitPropertyModifyKey.UnitDamageTakeReducePercent);
        if (damageReduceRatio != 0)
        {
            damageReduceRatio = Mathf.Clamp(damageReduceRatio, float.MaxValue, 1);
            var newDamage = info.Damage * (1 - damageReduceRatio / 100f);
            info.Damage = Mathf.RoundToInt(newDamage);
        }

        if (_owner is AIShip)
        {
            int Damage = info.Damage;
            bool critical = info.IsCritical;

            ///Check Damage
            if (Damage == 0)
                return false;

            if (info.IsPlayerAttack)
            {
                var enemyShip = _owner as AIShip;
                var enemyClass = enemyShip.AIShipCfg.ClassLevel;
                float damageAddition = 0;
                if(enemyClass == EnemyClassType.Elite || enemyClass == EnemyClassType.Boss)
                {
                    ///对精英BOSS额外伤害
                    damageAddition = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.EliteBossDamage);
                }
                else if(enemyClass == EnemyClassType.Meteorite)
                {
                    ///对陨石额外伤害
                    damageAddition = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.Meteorite_To_Damage);
                }

                if(damageAddition != 0)
                {
                    var newDamage = info.Damage * (1 + damageAddition / 100f);
                    newDamage = Mathf.Clamp(newDamage, 0, float.MaxValue);
                    info.Damage = Mathf.RoundToInt(newDamage);
                }
               
            }
            LevelManager.Instance.UnitBeforeHit(info);
            ///DamageText
            LevelManager.Instance.DamageDisplayHandler.ShowDamage(info.attackerUnit, UID, info.Damage, info.HitPoint, critical);
        }
        else if (_owner is PlayerShip)
        {
            ///CalculatePlayerTakeDamage
            GameHelper.ResolvePlayerUnitDamage(info);
            LevelManager.Instance.UnitBeforeHit(info);
            if (!info.IsHit)
            {
                ///Parry
                LevelManager.Instance.PlayerShipParry(info.attackerUnit);
            }
            else
            {
                ///Show Player TakeDamage
                LevelManager.Instance.DamageDisplayHandler.ShowPlayerTakeDamage(info.Damage, info.HitPoint);
                LevelManager.Instance.PlayerUnitTakeDamage(info);
            }
        }

        LevelManager.Instance.UnitHitFinish(info);
        if (info.IsHit)
        {
            bool isDie = false;
            if (damageState != DamagableState.Immortal)
            {
                isDie = HpComponent.ChangeHP(-info.Damage);
                ///HP为0
                if (isDie)
                {
                    if (_baseUnitConfig.ParalysisResume)
                    {
                        ///瘫痪恢复
                        ChangeUnitState(DamagableState.Paralysis);
                    }
                    else
                    {
                        UnitDeathInfo deathInfo = new UnitDeathInfo
                        {
                            isCriticalKill = info.IsCritical
                        };
                        Death(deathInfo);
                    }
                }

                ///敌人血量变化检测
                if(IsCoreUnit && _owner is AIShip)
                {
                    var aiship = _owner as AIShip;
                    aiship.OnCoreHPChange();
                }
            }
            return isDie;
        }
        return false;
    }
    

    public virtual void  SetUnitProcess(bool isprocess)
    {
        _isProcess = isprocess;
    }
    /// <summary>
    /// 最大血量变化
    /// </summary>
    protected void OnMaxHPChangeAction()
    {
        HpComponent.SetMaxHP(baseAttribute.HPMax);
    }

    public virtual void GameOver()
    {
        SetUnitProcess(false);
    }

    public virtual void PauseGame()
    {
       
    }

    public virtual void  UnPauseGame()
    {
       
    }

    /// <summary>
    /// 进入瘫痪状态
    /// </summary>
    protected virtual void OnEnterParalysisState()
    {
        _isParalysising = true;
    }

    /// <summary>
    /// 瘫痪恢复结束
    /// </summary>
    protected virtual void OnParalysisStateFinish()
    {
        _isParalysising = false;
        _paralysisTimer = 0f;
        ChangeUnitState(DamagableState.Normal);
        HpComponent.RecoverHPToMax();
    }

    #region Proeprty & Effect

    public void AddNewModifyEffectDatas(List<ModifyTriggerData> datas)
    {
        for(int i = 0; i < datas.Count; i++)
        {
            AddNewModifyEffectData(datas[i]);
        }
    }

    public void AddNewModifyEffectData(ModifyTriggerData data)
    {
        data.OnTriggerAdd();
        _modifyTriggerDatas.Add(data);
    }

    public ModifyTriggerData GetModifierTriggerByUID(uint uid)
    {
        return _modifyTriggerDatas.Find(x => x.UID == uid);
    }

    /// <summary>
    /// 刷新格子效果
    /// </summary>
    public void RefreshEffectSlot()
    {
        RemoveAllSlotEffets();

        if (_baseUnitConfig.SlotEffects == null || _baseUnitConfig.SlotEffects.Length <= 0)
            return;

        if (effectSlotCoords == null || effectSlotCoords.Count <= 0)
            return;

        List<Unit> targetUnits = new List<Unit>();
        for (int i = 0; i < effectSlotCoords.Count; i++)
        {
            var unit = _owner.GetUnitBySlotPosition(effectSlotCoords[i]);
            if (unit != null && targetUnits.Contains(unit))
            {
                targetUnits.Add(unit);
            }
        }

        ///CheckMainCondition
        for (int i = 0; i < _baseUnitConfig.SlotEffects.Length; i++)
        {
            var effect = _baseUnitConfig.SlotEffects[i];

            for (int j = 0; j < targetUnits.Count; j++) 
            {
                if(CheckEffectSlotMainConditions(effect.MainConditions, targetUnits[j], targetUnits))
                {
                    if(effect.ApplyTarget == UnitSlotEffectApplyTarget.Target)
                    {
                        ApplySlotEffectToTarget(targetUnits[i], effect.ApplyEffects, effect.ModifyMap);
                    }
                    else if (effect.ApplyTarget == UnitSlotEffectApplyTarget.Self)
                    {
                        ///自身添加效果
                        ApplySlotEffectToTarget(this, effect.ApplyEffects, effect.ModifyMap);
                    }
                }
            }
        }
    }

    private void ApplySlotEffectToTarget(Unit targetUnit, ModifyTriggerConfig[] triggers, Dictionary<UnitPropertyModifyKey, float> property)
    {
        if (targetUnit == null)
            return;

        if(triggers != null && triggers.Length > 0)
        {
            for(int i = 0; i < triggers.Length; i++)
            {
                ModifyTriggerData data = triggers[i].Create(triggers[i], 0);
                var uid = ModifyUIDManager.Instance.GetUID(PropertyModifyCategory.ModifyTrigger, data);
                data.UID = uid;
                data.OwnerUID = targetUnit.UID;
                data.FromUnitSlotEffect = true;
                targetUnit.AddNewModifyEffectData(data);
            }
        }

        if(property != null && property.Count > 0)
        {
            foreach(var item in property)
            {
                targetUnit.LocalPropetyData.AddPropertyModifyValue(item.Key, LocalPropertyModifyType.EffectSlotModify, UID, item.Value);
            }
        }
    }

    /// <summary>
    /// 移除所有网格效果
    /// </summary>
    private void RemoveAllSlotEffets()
    {
        for (int i = _modifyTriggerDatas.Count - 1; i >= 0; i--) 
        {
            var trigger = _modifyTriggerDatas[i];
            if (trigger.FromUnitSlotEffect)
            {
                trigger.OnTriggerRemove();
                _modifyTriggerDatas.RemoveAt(i);
            }
        }

        LocalPropetyData.ClearAllSlotEffectPropertyModify();
    }

    private void AddModifySpecials()
    {
        var propertyModify = _baseUnitConfig.PropertyModify;
        if (propertyModify != null && propertyModify.Length > 0)
        {
            for (int i = 0; i < propertyModify.Length; i++)
            {
                var modify = propertyModify[i];
                if (!modify.BySpecialValue)
                {
                    RogueManager.Instance.MainPropertyData.AddPropertyModifyValue(modify.ModifyKey, PropertyModifyType.Modify, UID, modify.Value);
                }
                else
                {
                    PropertyModifySpecialData specialData = new PropertyModifySpecialData(modify, UID);
                    _modifySpecialDatas.Add(specialData);
                }
            }
        }
        ///Handle SpecialDatas
        for (int i = 0; i < _modifySpecialDatas.Count; i++)
        {
            _modifySpecialDatas[i].HandlePropetyModifyBySpecialValue();
        }
    }

    private void AddModifyTriggers()
    {
        var triggers = _baseUnitConfig.ModifyTriggers;
        if (triggers != null && triggers.Length > 0)
        {
            for (int i = 0; i < triggers.Length; i++)
            {
                var triggerData = triggers[i].Create(triggers[i], 0);
                if (triggerData != null)
                {
                    var uid = ModifyUIDManager.Instance.GetUID(PropertyModifyCategory.ModifyTrigger, triggerData);
                    triggerData.UID = uid;
                    triggerData.OwnerUID = UID;
                    triggerData.OnTriggerAdd();
                    _modifyTriggerDatas.Add(triggerData);
                }
            }
        }
    }

    private bool CheckEffectSlotMainConditions(UnitSlotEffectConditionConfig[] cons, Unit targetUnit, List<Unit> allUnits)
    {
        if (targetUnit == null)
            return false;

        if (cons == null || cons.Length <= 0)
            return true;

        bool result = true;
        for(int i = 0; i < cons.Length; i++)
        {
            result &= cons[i].GetResult(targetUnit, allUnits);
        }

        return result;
    }

    #endregion

    public virtual UnitLogData GetUnitLogData()
    {
        return null;
    }

    #region Anim

    protected const string AnimTrigger_Spawn = "Spawn";
    protected const string AnimTrigger_DeSpawn = "DeSpawn";
    protected const string AnimTrigger_SelfExplode = "SelfExplode";

    public void DoSpawnEffect()
    {
        if (IsInvisiableUnit)
            return;

        _DoSpawnEffect();
    }

    protected async void _DoSpawnEffect()
    {
        unitSprite.material = _appearMat;
        SetAnimatorTrigger(AnimTrigger_Spawn);
        var length = GameHelper.GetAnimatorClipLength(_animator, "EnemyShip_Spawn");
        await UniTask.Delay((int)(length * 1000));
        _appearMat.SetFloat(Mat_Shader_PropertyKey_HOLOGRAM_ON, 0);
        unitSprite.material = _sharedMat;
    }

    public void DoDeSpawnEffect()
    {
        if (IsInvisiableUnit)
            return;

        _DoDeSpawnEffect();
    }

    protected virtual void _DoDeSpawnEffect()
    {
        unitSprite.material = _appearMat;
        SetAnimatorTrigger(AnimTrigger_DeSpawn);
    }

    protected virtual void ResetAllAnimation()
    {
        if (IsInvisiableUnit)
            return;
        if(_appearMat != null)
        {
            _appearMat.SetFloat(Mat_Shader_PropertyKey_HOLOGRAM_ON, 0);
            _appearMat.SetFloat(Mat_Shader_PropertyKey_OUTLINE, 0);
        }

        if (_animator != null)
        {
            _animator.ResetTrigger(AnimTrigger_Spawn);
            _animator.ResetTrigger(AnimTrigger_DeSpawn);
        }
    }

    protected const string Mat_Shader_PropertyKey_HOLOGRAM_ON = "_HologramBlend";
    protected const string Mat_Shader_PropertyKey_OUTLINE = "_OutlineAlpha";

    protected virtual void SetAnimatorTrigger(string trigger)
    {
        if(_animator == null) { return; }
        _animator.SetTrigger(trigger);
    }

    private void InitMaterial()
    {
        if (IsInvisiableUnit)
            return;

        _sharedMat = unitSprite.sharedMaterial;
        unitSprite.material = _sharedMat;
        var ownerType = GetOwnerType;
        if (ownerType == OwnerType.AI)
        {
            _appearMat = Instantiate(_sharedMat);
            
        }
        else if(ownerType == OwnerType.Player)
        {
            _playerHighlightMat = Instantiate(_sharedMat);
        }
    }

    #endregion

    #region UIDisplay

    public void RegisterHPBar()
    {
        if (_uiCanvas == null)
            return;

        _uiCanvas.SafeSetActive(true);

        UIManager.Instance.CreatePoolerUI<UnitSceneHPSlider>("UnitHPSlider", _uiCanvas, (panel) =>
        {
            panel.transform.localPosition = new Vector3(0, _baseUnitConfig.HPBarOffsetY, 0);
            var rect = panel.transform.SafeGetComponent<RectTransform>();
            rect.SetRectWidthAndHeight(_baseUnitConfig.HPBarWidth, _baseUnitConfig.HPBarHeight);
            panel.SetUpSlider(this);
            _sceneHPSlider = panel;
        });
    }

    private void RemoveHPBar()
    {
        if (_sceneHPSlider == null)
            return;

        _sceneHPSlider.PoolableDestroy();
        _sceneHPSlider = null;
    }

    public bool GetActiveAndEnabled()
    {
        return isActiveAndEnabled;
    }

    public float3 GetPosition()
    {
        return transform.position;
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }

    void IOtherTarget.OnUpdateBattle()
    {
        OnUpdateBattle();
    }

    #endregion
}

