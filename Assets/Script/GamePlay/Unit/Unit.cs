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
    /// ���Ѫ��
    /// </summary>
    public int HPMax
    {
        get;
        protected set;
    }

    /// <summary>
    /// ��������
    /// </summary>
    public int EnergyCost
    {
        get;
        protected set;
    }

    /// <summary>
    /// ��Դ����
    /// </summary>
    public int EnergyGenerate
    {
        get;
        protected set;
    }

    private int BaseHP;
    private int BaseEnergyCost;
    private int BaseEnergyGenerate;

    /// <summary>
    /// �������
    /// </summary>
    public float WeaponRange { get; protected set; }

    /// <summary>
    /// �Ƿ���ҽ�����Ӱ���˺�����
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

        if (isPlayerShip)
        {
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.HP, CalculateHP);

            if(BaseEnergyCost != 0)
            {
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.WeaponEnergyCostPercent, CalculateEnergyCost);
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.ShieldEnergyCostPercent, CalculateEnergyCost);
            }
            
            if(BaseEnergyGenerate != 0)
            {
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.UnitEnergyGenerate, CalculateEnergyGenerate);
            }
            
            CalculateHP();
            CalculateEnergyCost();
            CalculateEnergyGenerate();
        }
        else
        {
            var enemyShip = _parentUnit._owner as AIShip;
            _hardLevelItem = GameHelper.GetEnemyHardLevelItem(enemyShip.AIShipCfg.HardLevelGroupID);
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.EnemyHPPercent, CalculateEnemyHP);
            CalculateEnemyHP();
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
        var delta = mainProperty.GetPropertyFinal(PropertyModifyKey.UnitEnergyGenerate);
        var newValue = BaseEnergyGenerate + delta;
        EnergyGenerate = (int)Mathf.Clamp(newValue, 0, float.MaxValue);
        RefreshShipEnergy();
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

    public DamagableState state = DamagableState.None;

    public bool IsTarget { get { return _isTarget; } }
    public bool IsRestoreable = false;
    public bool IsCoreUnit = false;
    public string deathVFXName = "ExplodeVFX";
    public List<WeaponTargetInfo> targetList = new List<WeaponTargetInfo>();
    public int maxTargetCount = 3;

    protected bool _isTarget;
    public SpriteRenderer unitSprite;
    public Transform rotationRoot;
    public bool redirection = true;
    public int direction = 0;
    public Vector2Int pivot;
    public List<Vector2Int> occupiedCoords;

    public UnitLocalPropertyData LocalPropetyData = new UnitLocalPropertyData();

    private List<ModifyTriggerData> _modifyTriggerDatas = new List<ModifyTriggerData>();
    public List<ModifyTriggerData> AllTriggerDatas
    {
        get { return _modifyTriggerDatas; }
    }

    private List<PropertyModifySpecialData> _modifySpecialDatas = new List<PropertyModifySpecialData>();

    /// <summary>
    /// ��ǰ��������
    /// </summary>
    public byte currentEvolvePoints;



    public BaseShip _owner
    {
        get;
        protected set;
    }

    public bool IsProcess { get { return _isProcess; } }
    protected bool _isProcess = true;

    public UnitBaseAttribute baseAttribute;

    public BaseUnitConfig _baseUnitConfig
    {
        get;
        protected set;
    }
    /// <summary>
    /// Ѫ���������
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

    public virtual void Death(UnitDeathInfo info)
    {
        GameManager.Instance.UnRegisterPauseable(this);
        state = DamagableState.Destroyed;

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

            IsRestoreable = false;
        }
        if (_owner is PlayerShip)
        {
            AIManager.Instance.AddTargetUnit(this);
            (RogueManager.Instance.currentShip.controller as ShipController).shipUnitManager.AddActiveUnit(this);
            IsRestoreable = true;

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
    /// ����OnAdd ���� & ����
    /// </summary>
    public void OnAdded()
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

        var triggers = _baseUnitConfig.ModifyTriggers;
        if (triggers != null && triggers.Length > 0)
        {
            for (int i = 0; i < triggers.Length; i++)
            {
                var triggerData = ModifyTriggerData.CreateTrigger(triggers[i], UID);
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

    /// <summary>
    /// ����OnRemove ���� & ����
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
        //�Ѿ���������̱���Ĳ����յ������˺�
        if(state == DamagableState.Destroyed || state == DamagableState.Paralysis)
        {
            return false;
        }

        ///Hit Events
        HitInfo hitInfo = new HitInfo
        {
            DamageType = info.DamageType,
            isPlayerAttack = info.IsPlayerAttack,
            isCritical = info.IsCritical
        };
        LevelManager.Instance.UnitHit(hitInfo, info);

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

            ///ֻ�е��˲���ʾ�˺�����
            ///������Ҫ��ʾ��Ӧ��Ư������
            UIManager.Instance.CreatePoolerUI<FloatingText>("FloatingText", true, E_UI_Layer.Top, this.gameObject, (panel) =>
            {
                panel.transform.position = CameraManager.Instance.mainCamera.WorldToScreenPoint(transform.position);
                panel.Initialization();
                panel.SetText(Mathf.Abs(Damage), critical);
                panel.Show();

            });
        }
        else if (_owner is PlayerShip)
        {
            ///CalculatePlayerDamage
            GameHelper.ResolvePlayerUnitDamage(ref info);
        }
        
        bool isDie = HpComponent.ChangeHP(-info.Damage);
        if(isDie)
        {
            UnitDeathInfo deathInfo = new UnitDeathInfo
            {
                isCriticalKill = info.IsCritical
            };
            Death(deathInfo);

        }

        return isDie;
    }
    

    public virtual void  SetUnitProcess(bool isprocess)
    {
        _isProcess = isprocess;
    }
    /// <summary>
    /// ���Ѫ���仯
    /// </summary>
    private void OnMaxHPChangeAction()
    {
        HpComponent.SetMaxHP(baseAttribute.HPMax);
    }


    public virtual void PauseGame()
    {
       
    }

    public virtual void  UnPauseGame()
    {
       
    }
}

