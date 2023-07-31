using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum UnitType
{
    Buildings,
    Weapons,
    MainWeapons,
}
public class Unit : MonoBehaviour
{
    public string unitName;
    public float HP = 100;
    public DamagableState state = DamagableState.None;

    public SpriteRenderer unitSprite;
    public bool redirection = true;
    public int direction = 0;
    public Vector2Int pivot;
    public List<Vector2Int> occupiedCoords;
    private PlayerShip _owner;


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

    public virtual void Initialization(PlayerShip m_owner)
    {
        _owner = m_owner;
    }

    public virtual void TakeDamage()
    {

    }


}
