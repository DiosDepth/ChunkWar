using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameState : State
{

}

/// <summary>
///  EGameState_WelcomScreen
/// </summary>

public class EGameState_WelcomScreen : GameState
{
    public override void Initialization()
    {
        base.Initialization();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("GameState = WelcomScreen");
       
        CameraManager.Instance.Initialization();
        UIManager.Instance.ShowUI<BackGroundPanel>("BackGround", E_UI_Layer.Bot, GameManager.Instance, null);
        InputDispatcher.Instance.ChangeInputMode("UI");

        DataManager.Instance.LoadAllData(() =>
        {
            MonoManager.Instance.StartDelay(1.75f, () =>
            {
                GameEvent.Trigger(EGameState.EGameState_MainMenu);
            });
            GameManager.Instance.InitData();
            SaveLoadManager.Instance.Initialization();
            AchievementManager.Instance.Initialization();
        });

    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}

/// <summary>
///  EGameState_MainMenu
/// </summary>

public class EGameState_MainMenu : GameState
{

    public override void Initialization()
    {
        base.Initialization();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("GameState = MainMenu");
        UIManager.Instance.ShowUI<MainMenu>("MainMenu", E_UI_Layer.Mid, GameManager.Instance, (panel) =>
         {
             panel.Initialization();
         });
        SoundManager.Instance.PlayBGM("MainBGM");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}

/// <summary>
///  EGameState_ShipSelection
/// </summary>

public class EGameState_ShipSelection : GameState
{

    public override void Initialization()
    {
        base.Initialization();
       
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("GameState = ShipSelection");
        UIManager.Instance.HiddenUIALLBut(new List<string> { "LoadingScreen" });
        MonoManager.Instance.StartCoroutine(LevelManager.Instance.LoadScene(1, (ac) =>
        {
            LevelManager.Instance.LoadLevel("ShipSelectionLevel", (level) =>
            {
                level.Initialization();

                UIManager.Instance.ShowUI<ShipSelection>("ShipSelection", E_UI_Layer.Mid, (level as ShipSelectionLevel), (shipselection) =>
                {
                    shipselection.Initialization();

                    LoadingScreen loadingscreen = UIManager.Instance.GetGUIFromDic("LoadingScreen") as LoadingScreen;
                    if (loadingscreen != null)
                    {
                        loadingscreen.OpenLoadingDoor(() =>
                        {
                            UIManager.Instance.HiddenUI("LoadingScreen");
                        });
                    }
                });
            });
        }));
    }

    public override void OnUpdate()
    {


        base.OnUpdate();
    }

    public override void OnExit()
    {
        base.OnExit();
        LevelManager.Instance.UnloadCurrentLevel();
    }
}
/// <summary>
///  EGameState_GamePrepare
/// </summary>

public class EGameState_GamePrepare : GameState
{

    public override void Initialization()
    {
        base.Initialization();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("GameState = EGameState_GamePrepare");

        if (!RogueManager.Instance.InBattle)
        {
            RogueManager.Instance.InitRogueBattle();
            RogueManager.Instance.PlayCurrentHardLevelBGM();
        }
        
        if (LevelManager.Instance.needServicing)
        {
            LevelManager.Instance.LoadLevel("Harbor", (level) =>
            {
                GameEvent.Trigger(EGameState.EGameState_GameStart);
            });
        }
        else
        {
            LevelManager.Instance.LoadLevel("BattleLevel_001", (level) =>
            {

                RogueManager.Instance.currentShip = LevelManager.Instance.SpawnShipAtPos(RogueManager.Instance.currentShipSelection.itemconfig.Prefab, level.startPoint, Quaternion.identity, false);
                RogueManager.Instance.currentShip.LoadRuntimeData(RogueManager.Instance.ShipMapData);
                RogueManager.Instance.currentShip.gameObject.SetActive(true);
                //在初始化Ship之前先准备好Aimanager，会把对应的信息放入
                ECSManager.Instance.Initialization();
                RogueManager.Instance.currentShip.Initialization();

                //初始化摄影机
                CameraManager.Instance.SetFollowPlayerShip();
                CameraManager.Instance.SetVCameraBoard(level.cameraBoard);
                CameraManager.Instance.SetOrthographicSize(40);
                GameEvent.Trigger(EGameState.EGameState_GameStart);
            });
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}
/// <summary>
///  EGameState_GameStart
/// </summary>

public class EGameState_GameStart : GameState
{


    public override void Initialization()
    {
        base.Initialization();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("GameState = EGameState_GameStart");

        UIManager.Instance.HiddenAllUI();

        if (!LevelManager.Instance.needServicing)
        {
            InputDispatcher.Instance.ChangeInputMode("Player");
            InputDispatcher.Instance.Action_GamePlay_Pause += HandlePause;
            InputDispatcher.Instance.Action_UI_UnPause += HandleUnPause;
            RogueManager.Instance.OnExitHarbor();
            UIManager.Instance.ShowUI<ShipHUD>("ShipHUD", E_UI_Layer.Mid, GameManager.Instance, (panel) =>
            {
                panel.Initialization();
                RogueManager.Instance.OnNewWaveStart();
                RogueManager.Instance.currentShip.controller.IsUpdate = true;
                LevelManager.Instance.LevelActive();
            });
        }
        else
        {
            InputDispatcher.Instance.ChangeInputMode("UI");
            RogueManager.Instance.OnEnterHarborInit();

            UIManager.Instance.ShowUI<HarborHUD>("HarborHUD", E_UI_Layer.Mid, GameManager.Instance, (panel) =>
            {
                panel.Initialization();

            });
           
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (LevelManager.Instance.needServicing)
        {
            if (UIManager.Instance.IsMouseOverUI())
            {
                InputDispatcher.Instance.ChangeInputMode("UI");
            }
            else
            {
                InputDispatcher.Instance.ChangeInputMode("Player");
            }
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        InputDispatcher.Instance.Action_GamePlay_Pause -= HandlePause;
        InputDispatcher.Instance.Action_UI_UnPause -= HandleUnPause;
    }


    public void HandlePause(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
        {
            if (RogueManager.Instance.IsShowingShipLevelUp)
                return;

            InputDispatcher.Instance.ChangeInputMode("UI");
            UIManager.Instance.HiddenUI("ShipHUD");
            GameManager.Instance.PauseGame();
            UIManager.Instance.ShowUI<Pause>("Pause", E_UI_Layer.Mid, GameManager.Instance, (panel) =>
            {
                panel.Initialization();
            });
        }
    }
    
    public void HandleUnPause(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
        {
            if (RogueManager.Instance.IsShowingShipLevelUp)
                return;

            InputDispatcher.Instance.ChangeInputMode("Player");
            UIManager.Instance.HiddenUI("Pause");
            GameManager.Instance.UnPauseGame();
            UIManager.Instance.ShowUI<ShipHUD>("ShipHUD", E_UI_Layer.Mid, GameManager.Instance, (panel) =>
            {
                panel.Initialization();
            });
        }
    }

}

/// <summary>
///  EGameState_GameOver
/// </summary>

public class EGameState_GameOver : GameState
{

    public override void Initialization()
    {
        base.Initialization();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("GameState = EGameState_GameOver");

        UIManager.Instance.HiddenAllUI();

        InputDispatcher.Instance.ChangeInputMode("UI");
        LevelManager.Instance.GameOver();
        ECSManager.Instance.GameOver();
        RogueManager.Instance.RogueBattleOver();
        UIManager.Instance.ShowUI<GameOver>("GameOver", E_UI_Layer.Mid, GameManager.Instance, (panel) => 
        {
            panel.Initialization();
            LevelManager.Instance.UnloadCurrentLevel();
        });

    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}


/// <summary>
///  EGameState_Harbor
/// </summary>

public class EGameState_GameHarbor : GameState
{

    public override void Initialization()
    {
        base.Initialization();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        if(LevelManager.Instance.needServicing)
        {
            LevelManager.Instance.needServicing = false;
            (LevelManager.Instance.currentLevel as Harbor).shipbuilder.SaveShip();
        }
        else
        {
            LevelManager.Instance.needServicing = true;
            RogueManager.Instance.currentShip.SaveRuntimeData();
        }
        LevelManager.Instance.UnloadCurrentLevel();
        
        GameEvent.Trigger(EGameState.EGameState_GamePrepare);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}