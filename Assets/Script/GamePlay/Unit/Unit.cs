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
    /// ×î´óÑªÁ¿
    /// </summary>
    public int HPMax
    {
        get;
        protected set;
    }

    private int BaseHP;


    protected UnitPropertyData mainProperty;
    public virtual void InitProeprty(BaseUnitConfig cfg)
    {

        mainProperty = RogueManager.Instance.MainPropertyData;
        BaseHP = cfg.BaseHP;


        mainProperty.BindPropertyChangeAction(PropertyModifyKey.HP, CalculateHP);
    }

    public virtual void Destroy()
    {
        RogueManager.Instance.MainPropertyData. UnBindPropertyChangeAction(PropertyModifyKey.HP, CalculateHP);

    }

    private void CalculateHP()
    {
        var hp = RogueManager.Instance.MainPropertyData.GetPropertyFinal(PropertyModifyKey.HP);
        HPMax = BaseHP + Mathf.RoundToInt(hp);
    }
}

public class Unit : MonoBehaviour
{
    public int UnitID;
    public DamagableState state = DamagableState.None;
    public bool IsTarget { get { return _isTarget; } }
    [SerializeField]
    private bool _isTarget;
    public SpriteRenderer unitSprite;
    public bool redirection = true;
    public int direction = 0;
    public Vector2Int pivot;
    public List<Vector2Int> occupiedCoords;
    private BaseShip _owner;

    protected UnitBaseAttribute baseAttribute;

    public virtual void Start()
    {

    }

    // Update is called once per frame
    public virtual void Update()
    {

    }

    protected virtual void OnDestroy()
    {
        
    }

    public virtual void Initialization(BaseShip m_owner)
    {
        _owner = m_owner;
    }

    public virtual void TakeDamage()
    {

    }


}
