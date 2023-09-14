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
        UIManager.Instance.ShowUI<WellcomScreen>("WellcomScreen", E_UI_Layer.Top, GameManager.Instance, (panel) =>
        {
            MonoManager.Instance.StartCoroutine(DataManager.Instance.LoadAllData(() =>
            {
                MonoManager.Instance.StartDelay(1.75f, () =>
                {
                    LeanTween.alpha(panel.uiGroup.gameObject, 0, 0.25f).setOnComplete(() =>
                    {
                        UIManager.Instance.HiddenUI("WellcomScreen");
                        GameEvent.Trigger(EGameState.EGameState_MainMenu);
                    });

                });
                GameManager.Instance.InitData();
                SaveLoadManager.Instance.Initialization();
                AchievementManager.Instance.Initialization();
            }));
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
             panel.uiGroup.alpha = 0;
             LeanTween.value(0, 1, 0.25f).setOnUpdate((value) =>
             {
                 panel.uiGroup.alpha = value;

                 LoadingScreen loadingscreen = UIManager.Instance.GetGUIFromDic("LoadingScreen") as LoadingScreen;
                 if (loadingscreen != null)
                 {
                     loadingscreen.OpenLoadingDoor(()=> 
                     {
                         UIManager.Instance.HiddenUI("LoadingScreen");
                     });
                 }
             });
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
        RogueManager.Instance.InitShipSelection();
        Debug.Log("GameState = ShipSelection");
        UIManager.Instance.HiddenUIALLBut(new List<string> { "LoadingScreen" });
        MonoManager.Instance.StartCoroutine(LevelManager.Instance.LoadScene(1, (ac) =>
        {
          

            MonoManager.Instance.StartCoroutine(LevelManager.Instance.LoadLevel("ShipSelectionLevel", (level) =>
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
            }));
        }));

        ///Test
        
    }

    public override void OnUpdate()
    {


        base.OnUpdate();
    }

    public override void OnExit()
    {
        base.OnExit();
        LevelManager.Instance.UnloadCurrentLevel();
        GameManager.Instance.InitialRuntimeData();
        RogueManager.Instance.InitRogueBattle();

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

        if (LevelManager.Instance.needServicing)
        {
            MonoManager.Instance.StartCoroutine(LevelManager.Instance.LoadLevel("Harbor", (level) =>
            {
                GameEvent.Trigger(EGameState.EGameState_GameStart);
            }));
        }
        else
        {
            MonoManager.Instance.StartCoroutine(LevelManager.Instance.LoadLevel("BattleLevel_001", (level) =>
            {
               
                RogueManager.Instance.currentShip =  LevelManager.Instance.SpawnShipAtPos(RogueManager.Instance.currentShipSelection.itemconfig.Prefab, level.startPoint,Quaternion.identity,false);
                RogueManager.Instance.currentShip.LoadRuntimeData(RogueManager.Instance.ShipMapData);
                RogueManager.Instance.currentShip.gameObject.SetActive(true);
                //在初始化Ship之前先准备好Aimanager，会把对应的信息放入
                AIManager.Instance.Initialization();
                RogueManager.Instance.currentShip.Initialization();
                RogueManager.Instance.currentShip.CreateShip();
                RogueManager.Instance.currentShip.ActiveShipUnit();

                //初始化摄影机
                //CameraManager.Instance.ChangeVCameraLookAtTarget(GameManager.Instance.gameEntity.currentShip.transform);
                CameraManager.Instance.ChangeVCameraFollowTarget(RogueManager.Instance.currentShip.transform);
                CameraManager.Instance.vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.x = 0;
                CameraManager.Instance.vcam.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.x = 0;
                CameraManager.Instance.SetVCameraBoard(level.cameraBoard);
                CameraManager.Instance.vcam.m_Lens.OrthographicSize = 40;

                ///start Timer
                

                GameEvent.Trigger(EGameState.EGameState_GameStart);
            }));

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

        

        if (!LevelManager.Instance.needServicing)
        {
            InputDispatcher.Instance.ChangeInputMode("Player");
            InputDispatcher.Instance.Action_GamePlay_Pause += HandlePause;
            InputDispatcher.Instance.Action_UI_UnPause += HandleUnPause;
        }
        else
        {
            InputDispatcher.Instance.ChangeInputMode("UI");
        }
        
        UIManager.Instance.HiddenUIALLBut(new List<string> { "LoadingScreen" }, false);
        LoadingScreen loadingscreen = UIManager.Instance.GetGUIFromDic("LoadingScreen") as LoadingScreen;
        if (loadingscreen != null)
        {
            if (LevelManager.Instance.needServicing)
            {
                RogueManager.Instance.OnEnterHarborInit();

                UIManager.Instance.ShowUI<HarborHUD>("HarborHUD", E_UI_Layer.Mid, GameManager.Instance, (panel) =>
                {
                    panel.Initialization();

                    loadingscreen.OpenLoadingDoor(() =>
                    {
                        UIManager.Instance.HiddenUI("LoadingScreen");
                        
                    });
                });
            }
            else
            {
                UIManager.Instance.ShowUI<ShipHUD>("ShipHUD", E_UI_Layer.Mid, GameManager.Instance, (panel) =>
                {
                    panel.Initialization();

                    loadingscreen.OpenLoadingDoor(() =>
                    {
                        UIManager.Instance.HiddenUI("LoadingScreen");
                        RogueManager.Instance.currentShip.controller.IsUpdate = true;
                        RogueManager.Instance.Timer.StartTimer();
                        LevelManager.Instance.LevelActive();
                     
                        //LeanTween.delayedCall(10, () =>
                        //{
                        //    GameStateTransitionEvent.Trigger(EGameState.EGameState_GameCompleted);
                        //});
                    });
                });
            }
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
            InputDispatcher.Instance.ChangeInputMode("UI");


            UIManager.Instance.ShowUI<Pause>("Pause", E_UI_Layer.Mid, GameManager.Instance, (panel) =>
            {
                panel.Initialization();

                LeanTween.value(0, 9, 0.25f).setOnUpdate((value) => 
                {
                    CameraManager.Instance.vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.x = value;
                    CameraManager.Instance.vcam.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.x = value;

                });
                LeanTween.value(512, 0, 0.25f).setOnUpdate((value)=> 
                {
                    panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(value, 0);
                });

            });
        }


    }
    
    public void HandleUnPause(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
        {
         
            GUIBasePanel pause = UIManager.Instance.GetGUIFromDic("Pause");



            LeanTween.value(9,0, 0.25f).setOnUpdate((value) =>
            {
                CameraManager.Instance.vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.x = value;
                CameraManager.Instance.vcam.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.x = value;

            });
            LeanTween.value(0,512, 0.25f).setOnUpdate((value) =>
            {
                pause.GetComponent<RectTransform>().anchoredPosition = new Vector2(value, 0);
            }).setOnComplete(()=> 
            {
                InputDispatcher.Instance.ChangeInputMode("Player");
                UIManager.Instance.HiddenUI("Pause");
            });

        }
       
    }

}
/// <summary>
///  EGameState_GamePause
/// </summary>

public class EGameState_GamePause : GameState
{

    public override void Initialization()
    {
        base.Initialization();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("GameState = EGameState_GamePause");

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
///  EGameState_GameUnPause
/// </summary>

public class EGameState_GameUnPause : GameState
{

    public override void Initialization()
    {
        base.Initialization();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("GameState = EGameState_GameUnPause");
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
        InputDispatcher.Instance.ChangeInputMode("UI");
        LevelManager.Instance.GameOver();
        AIManager.Instance.GameOver();
        RogueManager.Instance.RogueBattleOver();
        UIManager.Instance.ShowUI<GameOver>("GameOver", E_UI_Layer.Mid, GameManager.Instance, (panel) => 
        {
            panel.Initialization();
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
///  EGameState_GameCompleted
/// </summary>

public class EGameState_GameCompleted : GameState
{

    public override void Initialization()
    {
        base.Initialization();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("GameState = EGameState_GameCompleted");
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
        AIManager.Instance.GameOver();
        LevelManager.Instance.UnloadCurrentLevel();


        if (RogueManager.Instance.currentShip != null)
        {
            GameObject.Destroy(RogueManager.Instance.currentShip.container.gameObject);
            RogueManager.Instance.currentShip = null;
        }
        
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
/// <summary>
///  EGameState_GameEnd
/// </summary>

public class EGameState_GameEnd : GameState
{

    public override void Initialization()
    {
        base.Initialization();
    }

    public override void OnEnter()
    {
        base.OnEnter();

        GameManager.Instance.InitialRuntimeData();
        RogueManager.Instance.RogueBattleOver();

        //if (RogueManager.Instance.currentShip != null)
        //{
        //    GameObject.Destroy(RogueManager.Instance.currentShip.container.gameObject);
        //    RogueManager.Instance.currentShip = null;
        //}

        //GameEvent.Trigger(EGameState.EGameState_MainMenu);



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
///  EGameState_GameReset
/// </summary>

public class EGameState_GameReset : GameState
{

    public override void Initialization()
    {
        base.Initialization();
    }

    public override void OnEnter()
    {
        base.OnEnter();


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

