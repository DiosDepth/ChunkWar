using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum UnitType
{
    Buildings,
    Weapons,
    MainWeapons,
}

[System.Serializable]
public class UnitBaseAttribute
{
    /// <summary>
    /// 最大血量
    /// </summary>
    public int HPMax
    {
        get;
        protected set;
    }

    /// <summary>
    /// 能量消耗
    /// </summary>
    public int EnergyCost
    {
        get;
        protected set;
    }

    /// <summary>
    /// 能源产生
    /// </summary>
    public int EnergyGenerate
    {
        get;
        protected set;
    }

    /// <summary>
    /// 瘫痪恢复时长
    /// </summary>
    public float UnitParalysisRecoverTime
    {
        get;
        protected set;
    }

    private int BaseHP;
    private int BaseEnergyCost;
    private int BaseEnergyGenerate;
    private float BaseParalysisRecoverTime;

    /// <summary>
    /// 武器射程
    /// </summary>
    public float WeaponRange { get; protected set; }

    /// <summary>
    /// 是否玩家舰船，影响伤害计算
    /// </summary>
    protected bool isPlayerShip;


    protected UnitPropertyData mainProperty;
    protected Unit _parentUnit;

    protected EnemyHardLevelItem _hardLevelItem;

    public virtual void InitProeprty(Unit parentUnit, BaseUnitConfig cfg, bool isPlayerShip)
    {
        this.isPlayerShip = isPlayerShip;
        this._parentUnit = parentUnit;

        mainProperty = RogueManager.Instance.MainPropertyData;
        BaseHP = cfg.BaseHP;
        BaseEnergyCost = cfg.BaseEnergyCost;
        BaseEnergyGenerate = cfg.BaseEnergyGenerate;
        BaseParalysisRecoverTime = cfg.ParalysisResumeTime;

        if (isPlayerShip)
        {
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.HP, CalculateHP);

            if(BaseEnergyCost != 0)
            {
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.WeaponEnergyCostPercent, CalculateEnergyCost);
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.ShieldEnergyCostPercent, CalculateEnergyCost);
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.UnitParalysisRecoverTimeRatio, CalculateUnitParalysis);
            }
            
            if(BaseEnergyGenerate != 0)
            {
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.UnitEnergyGenerate, CalculateEnergyGenerate);
            }
            
            CalculateHP();
            CalculateEnergyCost();
            CalculateEnergyGenerate();
            CalculateUnitParalysis();
        }
        else
        {
            //unit with no owner ,that means this is a dispersed unit;
            if (_parentUnit._owner == null)
            {
                _hardLevelItem = GameHelper.GetEnemyHardLevelItem(1);
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.EnemyHPPercent, CalculateEnemyHP);
                CalculateEnemyHP();
            }
            else
            {
                var enemyShip = _parentUnit._owner as AIShip;

                _hardLevelItem = GameHelper.GetEnemyHardLevelItem(enemyShip.AIShipCfg.HardLevelGroupID);
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.EnemyHPPercent, CalculateEnemyHP);
                CalculateEnemyHP();
            }
        }
    }

    public virtual void Destroy()
    {
        if (isPlayerShip)
        {
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.HP, CalculateHP);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.WeaponEnergyCostPercent, CalculateEnergyCost);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.UnitEnergyGenerate, CalculateEnergyGenerate);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.ShieldEnergyCostPercent, CalculateEnergyCost);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.UnitParalysisRecoverTimeRatio, CalculateUnitParalysis);
        }
        else
        {
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.EnemyHPPercent, CalculateEnemyHP);
        }
    }

    private void CalculateHP()
    {
        var hp = mainProperty.GetPropertyFinal(PropertyModifyKey.HP);
        HPMax = BaseHP + Mathf.RoundToInt(hp);
    }

    private void CalculateEnergyCost()
    {
        float rate = 0;

        var baseCfg = _parentUnit._baseUnitConfig;

        if (baseCfg.HasUnitTag(ItemTag.Weapon) || baseCfg.HasUnitTag(ItemTag.MainWeapon) || baseCfg.HasUnitTag(ItemTag.Building)) 
        {
            rate = mainProperty.GetPropertyFinal(PropertyModifyKey.WeaponEnergyCostPercent);
        }
        else if (baseCfg.HasUnitTag(ItemTag.Shield))
        {
            rate = mainProperty.GetPropertyFinal(PropertyModifyKey.ShieldEnergyCostPercent);
        }

        var totalUnitCostPercent = mainProperty.GetPropertyFinal(PropertyModifyKey.UnitEnergyCostPercent);
        rate += totalUnitCostPercent;
        ///Calculate Local Property
        var localEnergyCostPercent = _parentUnit.LocalPropetyData.GetPropertyFinal(UnitPropertyModifyKey.UnitEnergyCostPercent);
        rate += localEnergyCostPercent;
        rate = Mathf.Clamp(rate, -100, float.MaxValue);

        EnergyCost = Mathf.RoundToInt(BaseEnergyCost * (100 + rate) / 100f);

        RefreshShipEnergy();
    }

    private void CalculateEnergyGenerate()
    {
        var percent = mainProperty.GetPropertyFinal(PropertyModifyKey.UnitEnergyGenerate);
        ///Self GenerateValue
        var delta = _parentUnit.LocalPropetyData.GetPropertyFinal(UnitPropertyModifyKey.UnitEnergyGenerateValue);
        var newValue = BaseEnergyGenerate + delta;

        newValue *= (1 + percent / 100f);

        EnergyGenerate = (int)Mathf.Clamp(newValue, 0, float.MaxValue);
        RefreshShipEnergy();
    }

    private void CalculateUnitParalysis()
    {
        var percent = mainProperty.GetPropertyFinal(PropertyModifyKey.UnitParalysisRecoverTimeRatio);
        percent = Mathf.Clamp(percent, -100, float.MaxValue);
        UnitParalysisRecoverTime = (1 + percent / 100f) * BaseParalysisRecoverTime;
    }

    private void CalculateEnemyHP()
    {
        var hpPercent = mainProperty.GetPropertyFinal(PropertyModifyKey.EnemyHPPercent);
        int hardLevelHPAdd = _hardLevelItem != null ? _hardLevelItem.HPAdd : 0;
        HPMax = Mathf.RoundToInt((BaseHP + hardLevelHPAdd) * (1 + hpPercent));
    }

    protected void RefreshShipEnergy()
    {
        var playerShip = RogueManager.Instance.currentShip;
        if (playerShip != null)
        {
            playerShip.RefreshShipEnergy();
        }
    }
}

public class Unit : MonoBehaviour, IDamageble, IPropertyModify, IPauseable
{
    public int UnitID;
    /// <summary>
    /// UniqueID
    /// </summary>
    public uint UID { get; set; }

    public PropertyModifyCategory Category { get { return PropertyModifyCategory.ShipUnit; } }

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

    private Material _spriteMat;

    public virtual void Awake()
    {
        if(unitSprite != null)
        {
            _spriteMat = unitSprite.material;
        }
    }

    public virtual void Start() { }

    public virtual void Update() { }

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
                AIManager.Instance.AddTargetUnit(this);
                (RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.AddActiveUnit(this);
            }
        }

        state = target;
        if (target == DamagableState.Paralysis)
        {
            OnEnterParalysisState();
            if (_owner is PlayerShip)
            {
                ///移除目标选中
                AIManager.Instance.RemoveTargetUnit(this);
                if (_baseUnitConfig.unitType == UnitType.Weapons || _baseUnitConfig.unitType == UnitType.Buildings)
                {
                    (RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.RemoveActiveUnit(this);
                }
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
                AIManager.Instance.RemoveSingleUnit(this);
            }

            if (_owner is PlayerShip)
            {
                AIManager.Instance.RemoveTargetUnit(this);
                RogueManager.Instance.MainPropertyData.UnBindPropertyChangeAction(PropertyModifyKey.HP, OnMaxHPChangeAction);
                if (_baseUnitConfig.unitType == UnitType.Weapons || _baseUnitConfig.unitType == UnitType.Buildings)
                {
                    (RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.RemoveActiveUnit(this);
                }
            }
        }
        this.gameObject.SetActive(false);
        SetUnitProcess(false);
        
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + deathVFXName, true, (vfx) => 
        {
            
            vfx.transform.position = this.transform.position;
            vfx.GetComponent<ParticleController>().PoolableSetActive(true);
            vfx.GetComponent<ParticleController>().PlayVFX();
            unitSprite.color = Color.black;
        });
    
    }

    public virtual void Restore()
    {
        if (_owner is AIShip)
        {
            AIManager.Instance.AddSingleUnit(this);
        }
        if (_owner is PlayerShip)
        {
            AIManager.Instance.AddTargetUnit(this);
            (RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.AddActiveUnit(this);
    
        }
        SetUnitProcess(true);
        unitSprite.color = Color.white;
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
            AIManager.Instance.AddSingleUnit(this);

        }
        if (_owner is PlayerShip)
        {
            AIManager.Instance.AddTargetUnit(this);
            (RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.AddActiveUnit(this);

            SetUnitProcess(true);
            unitSprite.color = Color.white;
        }
        GameManager.Instance.RegisterPauseable(this);
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
    public void OnUpdateBattle()
    {
        if (_isParalysising)
        {
            _paralysisTimer += Time.deltaTime;
            if(_paralysisTimer >= baseAttribute.UnitParalysisRecoverTime)
            {
                OnParalysisStateFinish();
            }
            return;
        }

        if (state == DamagableState.Normal)
        {
            ///UpdateTrigger
            for (int i = 0; i < _modifyTriggerDatas.Count; i++)
            {
                _modifyTriggerDatas[i].OnUpdateBattle();
            }
        }
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
    public void OnRemove()
    {
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

    public virtual bool TakeDamage(DamageResultInfo info)
    {
        if (HpComponent == null)
            return false;
        //已经死亡或者瘫痪的不会收到更多伤害
        if(state == DamagableState.Destroyed || state == DamagableState.Paralysis || state == DamagableState.Immortal)
        {
            return false;
        }

        var rowScreenPos = CameraManager.Instance.mainCamera.WorldToScreenPoint(transform.position);
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
                if (IsCoreUnit)
                {
                    LevelManager.Instance.PlayerCoreUnitTakeDamage(info);
                }
            }
        }

        LevelManager.Instance.UnitHitFinish(info);
        if (info.IsHit)
        {
            bool isDie = HpComponent.ChangeHP(-info.Damage);
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
                var triggerData = triggers[i].Create(triggers[i], UID);
                if (triggerData != null)
                {
                    var uid = ModifyUIDManager.Instance.GetUID(PropertyModifyCategory.ModifyTrigger, triggerData);
                    triggerData.UID = uid;
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
}

