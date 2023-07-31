using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum DamagableState
{
    None,
    Normal,
    Destroyed,
}
public class Chunk : IDamageble
{


    public string chunkName;
    public float hp = 100;
    public DamagableState state = DamagableState.None;

    public Vector2Int shipCoord = Vector2Int.zero;
    public bool isOccupied = false;
    public bool isBuildingPiovt = false;


    public Unit unit;




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage()
    {

    }

}
