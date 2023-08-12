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
    /// 是否玩家舰船，影响伤害计算
    /// </summary>
    protected bool isPlayerShip;


    protected UnitPropertyData mainProperty;
    protected Unit _parentUnit;
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
                mainProperty.BindPropertyChangeAction(PropertyModifyKey.UnitEnergyCostPercent, CalculateEnergyCost);
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
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.EnemyHPPercent, CalculateEnemyHP);
            CalculateEnemyHP();
        }
    }

    public virtual void Destroy()
    {
        if (isPlayerShip)
        {
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.HP, CalculateHP);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.UnitEnergyCostPercent, CalculateEnergyCost);
            mainProperty.UnBindPropertyChangeAction(PropertyModifyKey.UnitEnergyGenerate, CalculateEnergyGenerate);
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
        var rate = mainProperty.GetPropertyFinal(PropertyModifyKey.UnitEnergyCostPercent);
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
        var enemyShip = _parentUnit._owner as AIShip;
        float hardLevelPercent = 0;
        if (enemyShip != null)
        {
            hardLevelPercent = GameHelper.GetEnemyHPByHardLevel(enemyShip.AIShipCfg.HardLevelCfg);
        }
        HPMax = Mathf.RoundToInt(BaseHP * (1 + hpPercent + hardLevelPercent / 100f));
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

public class Unit : MonoBehaviour,IDamageble
{
    public int UnitID;
    /// <summary>
    /// UniqueID
    /// </summary>
    public uint UID;
    public DamagableState state = DamagableState.None;
    public bool IsTarget { get { return _isTarget; } }
    public bool IsRestoreable = false;
    public bool IsCoreUnit = false;
    public string deathVFXName = "ExplodeVFX";
    [SerializeField]
    private bool _isTarget;
    public SpriteRenderer unitSprite;
    public bool redirection = true;
    public int direction = 0;
    public Vector2Int pivot;
    public List<Vector2Int> occupiedCoords;

    /// <summary>
    /// 当前升级点数
    /// </summary>
    public byte currentEvolvePoints;

    public BaseShip _owner
    {
        get;
        private set;
    }

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

    public virtual void Start() { }

    public virtual void Update() { }

    public virtual void Death()
    {
        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + deathVFXName, true, (vfx) => 
        {
            SetUnitProcess(false);
            vfx.transform.position = this.transform.position;
            vfx.GetComponent<ParticleController>().PoolableSetActive(true);
            vfx.GetComponent<ParticleController>().PlayVFX();
            unitSprite.color = Color.black;

            if (IsCoreUnit)
            {
    
                _owner.Death();
                

                state = DamagableState.Destroyed;
                this.gameObject.SetActive(false);
                //destroy owner
            }

        });
    
    }

    public virtual void Restore()
    {
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
        _baseUnitConfig = m_unitconfig;
        HpComponent = new GeneralHPComponet(baseAttribute.HPMax, baseAttribute.HPMax);
        RogueManager.Instance.MainPropertyData.BindPropertyChangeAction(PropertyModifyKey.HP, OnMaxHPChangeAction);
        if (_owner is PlayerShip)
        {
            IsRestoreable = true;
        }
        else
        {
            IsRestoreable = false;
        }
        Restore();
    }

    public virtual bool TakeDamage(int value)
    {
        if (HpComponent == null)
            return false;


        //这里需要显示对应的漂浮文字
        UIManager.Instance.CreatePoolerUI<FloatingText>("FloatingText", true, E_UI_Layer.Top, this.gameObject, (panel) =>
          {
              panel.transform.position = CameraManager.Instance.mainCamera.WorldToScreenPoint(transform.position);

              panel.SetText(value);
              panel.Show();

          });
        bool isDie = HpComponent.ChangeHP(value);
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

