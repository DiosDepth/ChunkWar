using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStarter : MonoBehaviour
{
    public bool isDevMode = false;

    private void Awake()
    {
        if (GameManager.Instance.isInitialCompleted)
        {
            GameEvent.Trigger(GameState.WelcomScreen);

        }
    }

}
