using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEntity 
{
    public StateMachine<EGameState> gamestate = new StateMachine<EGameState>(null, true,true);
    public int score = 0;
    public SaveData saveData;
    public RuntimeData runtimeData;

    public int brickCount = 100;
    public int brickConsumePerChunk = 5;

    public InventoryItem currentShipSelection;
    public Ship currentShip;

    public Inventory chunkPartInventory;
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
        chunkPartInventory = new Inventory();
        buildingInventory = new Inventory();
    }

    public void Reset()
    {

    }

    
    
}
