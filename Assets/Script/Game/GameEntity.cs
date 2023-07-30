using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEntity 
{
    
    public int score = 0;
  
    public RuntimeData runtimeData;




    public Inventory buildingInventory;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Initialization()
    {
    
        buildingInventory = new Inventory();
    }

    public void Reset()
    {

    }

    
    
}
