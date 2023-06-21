using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Building : Unit,IDamageble
{


    public SpriteRenderer buildingSprite;
    public int direction = 0;
    public Vector2Int pivot;
    public List<Vector2Int> occupiedCoords;

    // Start is called before the first frame update
    void Start()
    {
        buildingSprite = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void TakeDamage()
    {
        
    }


}
