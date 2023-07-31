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

    public virtual void InitProeprty(BaseUnitConfig cfg)
    {
        BaseHP = cfg.BaseHP;

        var mainProperty = RogueManager.Instance.MainPropertyData;
        mainProperty.BindPropertyChangeAction(PropertyModifyKey.HP, CalculateHP);
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

    public virtual void OnDestroy()
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
