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

    private int BaseHP;
    private int BaseEnergyCost;
    private int BaseEnergyGenerate;

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
        if(_parentUnit._baseUnitConfig.HasUnitTag(ItemTag.Weapon))
        {
            rate = mainProperty.GetPropertyFinal(PropertyModifyKey.WeaponEnergyCostPercent);
        }
        else if (_parentUnit._baseUnitConfig.HasUnitTag(ItemTag.Shield))
        {
            rate = mainProperty.GetPropertyFinal(PropertyModifyKey.ShieldEnergyCostPercent);
        }
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

public class Unit : MonoBehaviour, IDamageble, IPropertyModify
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

    private List<ModifyTriggerData> _modifyTriggerDatas = new List<ModifyTriggerData>();
    public List<ModifyTriggerData> AllTriggerDatas
    {
        get { return _modifyTriggerDatas; }
    }

    private List<PropertyModifySpecialData> _modifySpecialDatas = new List<PropertyModifySpecialData>();

    /// <summary>
    /// 当前升级点数
    /// </summary>
    public byte currentEvolvePoints;

    public BaseShip _owner
    {
        get;
        private set;
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

    public virtual void Death()
    {
        state = DamagableState.Destroyed;

        if(_owner is AIShip)
        {
            AIManager.Instance.RemoveSingleUnit(this);
        }

        if(_owner is PlayerShip)
        {
            AIManager.Instance.RemoveTargetUnit(this);
        }
        this.gameObject.SetActive(false);
        SetUnitProcess(false);
        
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + deathVFXName, true, (vfx) => 
        {
            
            vfx.transform.position = this.transform.position;
            vfx.GetComponent<ParticleController>().PoolableSetActive(true);
            vfx.GetComponent<ParticleController>().PlayVFX();
            unitSprite.color = Color.black;

            if (IsCoreUnit)
            {
                _owner.CheckDeath(this);
                //destroy owner
            }

        });
    
    }

    public virtual void Restore()
    {
        if( _owner is AIShip)
        {
            AIManager.Instance.AddSingleUnit(this);
        }
        if(_owner is PlayerShip)
        {
            AIManager.Instance.AddTargetUnit(this);
        }
       
        SetUnitProcess(true);
        unitSprite.color = Color.white;
    }

    protected virtual void OnDestroy()
    {
        RogueManager.Instance.MainPropertyData.UnBindPropertyChangeAction(PropertyModifyKey.HP, OnMaxHPChangeAction);
    }

    public virtual void Initialization(BaseShip m_owner, BaseUnitConfig m_unitconfig)
    {
        _owner = m_owner;
        HpComponent = new GeneralHPComponet(baseAttribute.HPMax, baseAttribute.HPMax);
        RogueManager.Instance.MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.HP, OnMaxHPChangeAction);
        if (_owner is PlayerShip)
        {
            IsRestoreable = true;
            Restore();
        }
        else
        {
            IsRestoreable = false;
        }
  
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
    /// 处理OnAdd 属性 & 机制
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
    /// 处理OnRemove 属性 & 机制
    /// </summary>
    public void OnRemove()
    {
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

    public virtual bool TakeDamage(ref DamageResultInfo info)
    {
        if (HpComponent == null)
            return false;
        //已经死亡或者瘫痪的不会收到更多伤害
        if(state == DamagableState.Destroyed || state == DamagableState.Paralysis)
        {
            return false;
        }
        if(_owner is AIShip)
        {
            int Damage = info.Damage;
            bool critical = info.IsCritical;
            ///只有敌人才显示伤害数字
            //这里需要显示对应的漂浮文字
            UIManager.Instance.CreatePoolerUI<FloatingText>("FloatingText", true, E_UI_Layer.Top, this.gameObject, (panel) =>
            {
                panel.transform.position = CameraManager.Instance.mainCamera.WorldToScreenPoint(transform.position);

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
            Death();
        }
        return isDie;
    }
    

    public virtual void  SetUnitProcess(bool isprocess)
    {
        _isProcess = isprocess;
    }
    /// <summary>
    /// 最大血量变化
    /// </summary>
    private void OnMaxHPChangeAction()
    {
        HpComponent.SetMaxHP(baseAttribute.HPMax);
    }
}

