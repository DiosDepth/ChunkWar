using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    public bool isDevMode = false;

    private void Awake()
    {
        var mgr = GameManager.Instance;
        GameEvent.Trigger(EGameState.EGameState_WelcomScreen);
    }

}
