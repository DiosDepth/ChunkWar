using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEntity 
{
    public StateMachine<GameState> gamestate = new StateMachine<GameState>(null, true);
    public int score = 0;
    public SaveData saveData;
    public RuntimeData runtimeData;

    public int brickCount = 100;
    public int brickConsumePerChunk = 5;

    public Ship currentShip;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
