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

    private int BaseHP;

    /// <summary>
    /// 是否玩家舰船，影响伤害计算
    /// </summary>
    protected bool isPlayerShip;


    protected UnitPropertyData mainProperty;
    public virtual void InitProeprty(BaseUnitConfig cfg, bool isPlayerShip)
    {
        this.isPlayerShip = isPlayerShip;

        mainProperty = RogueManager.Instance.MainPropertyData;
        BaseHP = cfg.BaseHP;

        if (isPlayerShip)
        {
            mainProperty.BindPropertyChangeAction(PropertyModifyKey.HP, CalculateHP);
            CalculateHP();
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
            RogueManager.Instance.MainPropertyData.UnBindPropertyChangeAction(PropertyModifyKey.HP, CalculateHP);
        }
        else
        {
            RogueManager.Instance.MainPropertyData.UnBindPropertyChangeAction(PropertyModifyKey.EnemyHPPercent, CalculateEnemyHP);
        }
    }

    private void CalculateHP()
    {
        var hp = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.HP);
        HPMax = BaseHP + Mathf.RoundToInt(hp);
    }

    private void CalculateEnemyHP()
    {
        var hpPercent = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.EnemyHPPercent);
        HPMax = Mathf.RoundToInt(BaseHP * (1 + hpPercent / 100f));
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

    protected BaseShip _owner;

    protected bool _isActive = true;

    public UnitBaseAttribute baseAttribute;

    protected BaseUnitConfig _baseUnitConfig;
    /// <summary>
    /// 血量管理组件
    /// </summary>
    public GeneralHPComponet HpComponent;

    public virtual void Start()
    {

    }

    // Update is called once per frame
    public virtual void Update()
    {

    }

    public virtual void Death()
    {
        SetUnitActive(false);

        PoolManager.Instance.GetObjectAsync(GameGlobalConfig.VFXPath + deathVFXName, true, (vfx) => 
        {
            vfx.transform.position = this.transform.position;
            vfx.GetComponent<ParticleController>().SetActive();
            vfx.GetComponent<ParticleController>().PlayVFX();

        });
        unitSprite.color = Color.black; 
        
        if ( IsCoreUnit)
        {
            _owner.Death();
            Destroy(this.gameObject);
            //destroy owner
 
        }

  

    }

    public virtual void Restore()
    {
        SetUnitActive(true);
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
    }

    public virtual bool TakeDamage(int value)
    {
        if (HpComponent == null)
            return false;

        bool isDie = HpComponent.ChangeHP(value);
        if(isDie)
        {
            Death();
        }
        return isDie;
    }
    

    public virtual void  SetUnitActive(bool isactive)
    {
        _isActive = isactive;
    }
    /// <summary>
    /// 最大血量变化
    /// </summary>
    private void OnMaxHPChangeAction()
    {
        HpComponent.SetMaxHP(baseAttribute.HPMax);
    }
}

