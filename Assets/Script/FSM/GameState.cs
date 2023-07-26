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

        Debug.Log("GameState = ShipSelection");

        UIManager.Instance.HiddenUIALLBut(new List<string> { "LoadingScreen" });
        MonoManager.Instance.StartCoroutine(LevelManager.Instance.LoadScene(1, (ac) =>
        {
          

            MonoManager.Instance.StartCoroutine(LevelManager.Instance.LoadLevel("ShipSelectionLevel", (level) =>
            {

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
                //���پɵ�Ship
                if(GameManager.Instance.gameEntity.currentShip != null)
                {
                    GameObject.Destroy(GameManager.Instance.gameEntity.currentShip.container.gameObject);
                    GameManager.Instance.gameEntity.currentShip = null;
                }




                GameManager.Instance.gameEntity.currentShip =  LevelManager.Instance.SpawnShipAtPos(GameManager.Instance.gameEntity.currentShipSelection.itemconfig.Prefab, level.startPoint,Quaternion.identity,false);
                GameManager.Instance.gameEntity.currentShip.LoadRuntimeData(GameManager.Instance.gameEntity.runtimeData);
                GameManager.Instance.gameEntity.currentShip.InitialShip();

                //��ʼ����Ӱ��
                //CameraManager.Instance.ChangeVCameraLookAtTarget(GameManager.Instance.gameEntity.currentShip.transform);
                CameraManager.Instance.ChangeVCameraFollowTarget(GameManager.Instance.gameEntity.currentShip.transform);
                CameraManager.Instance.vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.x = 0;
                CameraManager.Instance.vcam.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset.x = 0;
                CameraManager.Instance.SetVCameraBoard(level.cameraBoard);
                CameraManager.Instance.vcam.m_Lens.OrthographicSize = 35;

                GameManager.Instance.gameEntity.currentShip.gameObject.SetActive(true);

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


                UIManager.Instance.ShowUI<ShipBuilderHUD>("ShipBuilderHUD", E_UI_Layer.Mid, GameManager.Instance, (panel) =>
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
                        GameManager.Instance.gameEntity.currentShip.Initialization();
                        GameManager.Instance.gameEntity.currentShip.controller.IsUpdate = true;


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

        if(UIManager.Instance.IsMouseOverUI())
        {
            InputDispatcher.Instance.ChangeInputMode("UI");
        }
        else
        {
            InputDispatcher.Instance.ChangeInputMode("Player");
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
            GameManager.Instance.gameEntity.currentShip.SaveRuntimeData();
        }

        LevelManager.Instance.UnloadCurrentLevel();


        if (GameManager.Instance.gameEntity.currentShip != null)
        {
            GameObject.Destroy(GameManager.Instance.gameEntity.currentShip.container.gameObject);
            GameManager.Instance.gameEntity.currentShip = null;
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

        LevelManager.Instance.UnloadCurrentLevel();
        GameManager.Instance.InitialRuntimeData();

        if (GameManager.Instance.gameEntity.currentShip != null)
        {
            GameObject.Destroy(GameManager.Instance.gameEntity.currentShip.container.gameObject);
            GameManager.Instance.gameEntity.currentShip = null;
        }

        GameEvent.Trigger(EGameState.EGameState_MainMenu);

        

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

