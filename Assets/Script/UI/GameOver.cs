using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : GUIBasePanel
{
    
    public override void Initialization()
    {
        base.Initialization();
        GetGUIComponent<Button>("TryAgain").onClick.AddListener(OnTryAgainPressed);
        GetGUIComponent<Button>("Quit").onClick.AddListener(OnQuitPressed);
    }



    public void OnQuitPressed()
    {
        GameStateTransitionEvent.Trigger(EGameState.EGameState_GameEnd);
    }
    public void OnTryAgainPressed()
    {
        GameStateTransitionEvent.Trigger(EGameState.EGameState_GameReset);
    }
}
