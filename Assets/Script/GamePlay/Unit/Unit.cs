using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour, IDamageble, IPropertyModify, IPauseable
{
    public int UnitID;
    /// <summary>
    /// UniqueID
    /// </summary>
    public uint UID { get; set; }

    public PropertyModifyCategory Category { get { return PropertyModifyCategory.ShipUnit; } }

    public string GetName
    {
        get { return LocalizationManager.Instance.GetTextValue(_baseUnitConfig.GeneralConfig.Name); }
    }


    [SerializeField]
    public DamagableState state
    {
        get;
        private set;
    }

    public bool IsCoreUnit = false;
    public string deathVFXName = "ExplodeVFX";
    public List<WeaponTargetInfo> targetList = new List<WeaponTargetInfo>();
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

    protected Material _spriteMat;
    protected Animator _animator;
    protected Transform _uiCanvas;
    private UnitSceneHPSlider _sceneHPSlider;

    public virtual void Awake()
    {
        if(unitSprite != null)
        {
            _spriteMat = unitSprite.material;
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
        if (state == target)
            return;

        if(state == DamagableState.Paralysis && target == DamagableState.Normal)
        {
            if(_owner is PlayerShip)
            {
                ///瘫痪恢复
                ECSManager.Instance.RegisterJobData(OwnerType.Player, this);
                //(RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.AddActiveUnit(this);
                LevelManager.Instance.PlayerUnitParalysis(UID, false); 
            }
        }

        state = target;
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

        if (IsCoreUnit)
        {
            _owner.CheckDeath(this, info);
            //destroy owner
        }
        else
        {
            if (_owner is AIShip)
            {
                ECSManager.Instance.UnRegisterJobData(OwnerType.AI, this);
            }

            if (_owner is PlayerShip)
            {

                RogueManager.Instance.MainPropertyData.UnBindPropertyChangeAction(PropertyModifyKey.HP, OnMaxHPChangeAction);
                ECSManager.Instance.UnRegisterJobData(OwnerType.Player, this);
                //if (_baseUnitConfig.unitType == UnitType.Weapons || _baseUnitConfig.unitType == UnitType.Buildings)
                //{
                //    (RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.RemoveActiveUnit(this);
                //}
            }
        }
        this.gameObject.SetActive(false);
        ///Remove Owner
        _owner.RemoveUnit(this);
        SetDisable();
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + deathVFXName, true, (vfx) => 
        {
            
            vfx.transform.position = this.transform.position;
            vfx.GetComponent<ParticleController>().PoolableSetActive(true);
            vfx.GetComponent<ParticleController>().PlayVFX();
        });
        
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
            //AIManager.Instance.AddSingleUnit(this);
        }
        if (_owner is PlayerShip)
        {
            ECSManager.Instance.RegisterJobData(OwnerType.Player, this);
            //AIManager.Instance.AddTargetUnit(this);
            //(RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.AddActiveUnit(this);
    
        }
        SetUnitProcess(true);
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
        HpComponent = new GeneralHPComponet(baseAttribute.HPMax, baseAttribute.HPMax);
        RogueManager.Instance.MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.HP, OnMaxHPChangeAction);

        if (_owner is AIShip)
        {
            ECSManager.Instance.RegisterJobData(OwnerType.AI, this);

        }
        if (_owner is PlayerShip)
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

        if (state == DamagableState.Normal)
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
        _modifyTriggerDatas.ForEach(x => x.OnTriggerRemove());
        _modifySpecialDatas.ForEach(x => x.OnRemove());
    }

    public void OutLineHighlight(bool highlight)
    {
        if (_spriteMat == null)
            return;

        if (highlight)
        {
            var color = GameHelper.GetRarityColor(_baseUnitConfig.GeneralConfig.Rarity);
            _spriteMat.EnableKeyword("OUTBASE_ON");
            _spriteMat.SetColor("_OutlineColor", color);
        }
        else
        {
            _spriteMat.DisableKeyword("OUTBASE_ON");
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

        if (state == DamagableState.Destroyed || state == DamagableState.Paralysis)
            return;

        HpComponent.ChangeHP(value);
    }

    public virtual bool TakeDamage(DamageResultInfo info)
    {
        if (HpComponent == null)
            return false;
        //已经死亡或者瘫痪的不会收到更多伤害
        if(state == DamagableState.Destroyed || state == DamagableState.Paralysis)
        {
            return false;
        }

        var rowScreenPos = UIManager.Instance.GetUIposBWorldPosition(transform.position);
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
                if(enemyClass == EnemyClassType.Elite || enemyClass == EnemyClassType.Boss)
                {
                    var damageAddition = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.EliteBossDamage);
                    var newDamage = info.Damage * (1 + damageAddition / 100f);
                    newDamage = Mathf.Clamp(newDamage, 0, float.MaxValue);
                    info.Damage = Mathf.RoundToInt(newDamage);
                }
            }
            LevelManager.Instance.UnitBeforeHit(info);
            UIManager.Instance.CreatePoolerUI<FloatingText>("FloatingText", true, E_UI_Layer.Top, this.gameObject, (panel) =>
            {
                panel.Initialization();
                panel.SetText(Mathf.Abs(Damage), critical, rowScreenPos);
                panel.Show();

            });
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
                UIManager.Instance.CreatePoolerUI<FloatingText>("FloatingText", true, E_UI_Layer.Top, this.gameObject, (panel) =>
                {
                    panel.Initialization();
                    panel.SetPlayerTakeDamageText(Mathf.Abs(info.Damage), rowScreenPos);
                    panel.Show();
                });
                LevelManager.Instance.PlayerUnitTakeDamage(info);
            }
        }

        LevelManager.Instance.UnitHitFinish(info);
        if (info.IsHit)
        {
            bool isDie = false;
            if (state != DamagableState.Immortal)
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
                if(CheckEffectSlotMainConditions(effect.MainConditions, targetUnits[j]))
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

    private bool CheckEffectSlotMainConditions(UnitSlotEffectConditionConfig[] cons, Unit targetUnit)
    {
        if (targetUnit == null)
            return false;

        if (cons == null || cons.Length <= 0)
            return true;

        bool result = true;
        for(int i = 0; i < cons.Length; i++)
        {
            result &= cons[i].GetResult(targetUnit);
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

    public async void DoSpawnEffect()
    {
        _spriteMat.EnableKeyword(Mat_Shader_PropertyKey_HOLOGRAM_ON);
        SetAnimatorTrigger(AnimTrigger_Spawn);
        var length = GameHelper.GetAnimatorClipLength(_animator, "EnemyShip_Spawn");
        await UniTask.Delay((int)(length * 1000));
        _spriteMat.DisableKeyword(Mat_Shader_PropertyKey_HOLOGRAM_ON);
    }

    public void DoDeSpawnEffect()
    {
        _spriteMat.EnableKeyword(Mat_Shader_PropertyKey_HOLOGRAM_ON);
        SetAnimatorTrigger(AnimTrigger_DeSpawn);
    }

    private void ResetAllAnimation()
    {
        _spriteMat.DisableKeyword(Mat_Shader_PropertyKey_HOLOGRAM_ON);
        _animator.ResetTrigger(AnimTrigger_Spawn);
        _animator.ResetTrigger(AnimTrigger_DeSpawn);
    }

    protected const string Mat_Shader_PropertyKey_HOLOGRAM_ON = "HOLOGRAM_ON";

    private void SetAnimatorTrigger(string trigger)
    {
        _animator.SetTrigger(trigger);
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

    #endregion
}

