using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum UnitState
{
    None,
    Normal,
    Destroyed,
}
public class Chunk : Unit,IDamageble
{



    public Vector2Int shipCoord = Vector2Int.zero;
    public bool isOccupied = false;
    public bool isBuildingPiovt = false;

    public GameObject modle;
    public Building building;




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
