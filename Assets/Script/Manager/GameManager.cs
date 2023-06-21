using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GameState
{
    WelcomScreen,
    MainMenu,
    GamePrepare,
    GameStart,
    GameOver,
    GameEnd,
    GameReset,
}

public enum ScoreEventType
{
    Change,
    Reset,
}


public struct GameEvent
{
    public GameState gamesate;
    public GameEvent(GameState state)
    {
        gamesate = state;
    }
    public static GameEvent e;
    public static void Trigger(GameState state)
    {
        e.gamesate = state;
        EventCenter.Instance.TriggerEvent<GameEvent>(e);
    }
}

public struct ScoreEvent
{
    public ScoreEventType type;
    public int scorechange;
    public ScoreEvent(ScoreEventType m_type, int m_scorechange)
    {
        type = m_type;
        scorechange = m_scorechange;
    }
    public static ScoreEvent e;
    public static void Trigger(ScoreEventType m_type, int m_scorechange)
    {
        e.type = m_type;
        e.scorechange = m_scorechange;
        EventCenter.Instance.TriggerEvent<ScoreEvent>(e);
    }
}

public class GameManager : Singleton<GameManager>, EventListener<GameEvent>,EventListener<ScoreEvent>
{
    public StateMachine<GameState> gamestate = new StateMachine<GameState>(null, true);
    public string playerPrefabPath = "Prefab/Chunk/ShipContainer";


    public int score = 0;
    public SaveData saveData;
    public RuntimeData runtimeData;

    public bool isInitialCompleted = false;


    public int brickCount = 100;
    public int brickConsumePerChunk = 5;

    public Ship currentShip;





    public GameManager()
    {
        //Initialization();
        this.EventStartListening<GameEvent>();
        this.EventStartListening<ScoreEvent>();
        Initialization();
    }

    public override void Initialization()
    {
        base.Initialization();
        isInitialCompleted = true;
        Debug.Log("GameManager Initialization");

    }
    public void OnEvent(GameEvent evt)
    {
        gamestate.ChangeState(evt.gamesate);
        switch (evt.gamesate)
        {
            case GameState.WelcomScreen:
                Debug.Log("GameState = WelcomScreen");

                UIManager.Instance.ShowUI<BackGroundPanel>("BackGround", E_UI_Layer.Bot,this, null);
                UIManager.Instance.ShowUI<WellcomScreen>("WellcomScreen", E_UI_Layer.Top, this, (panel) =>
                {
                    MonoManager.Instance.StartCoroutine(DataManager.Instance.LoadAllData(() => 
                    {
                        MonoManager.Instance.StartDelay(3, () =>
                        {
                            GameEvent.Trigger(GameState.MainMenu);
                        });
                    }));
                });

                break;
            case GameState.MainMenu:
                Debug.Log("GameState = MainMenu");

                UIManager.Instance.ShowUI<MainMenu>("MainMenu", E_UI_Layer.Mid,this, (panel) =>
                {
                    panel.Initialization();
                    panel.uiGroup.alpha = 0;
                    panel.uiGroup.interactable = false;

                    GUIBasePanel basegui = UIManager.Instance.GetGUIFromDic("WellcomScreen");
                    if (basegui != null)
                    {
                        MonoManager.Instance.StartCoroutine(UIManager.Instance.FadeUI(basegui.uiGroup, 0, 1, 0, 1, () =>
                        {
                            UIManager.Instance.HiddenUI("WellcomScreen");
                        }));
                        MonoManager.Instance.StartCoroutine(UIManager.Instance.FadeUI(panel.uiGroup, 0, 0, 1, 1, () =>
                        {

                            panel.uiGroup.interactable = true;

                        }));
                    }
                    else
                    {
                        UIManager.Instance.HiddenUI("WellcomScreen");
                        MonoManager.Instance.StartCoroutine(UIManager.Instance.FadeUI(panel.uiGroup, 0, 0, 1, 1, () =>
                        {

                            panel.uiGroup.interactable = true;

                        }));
                    }


                });
                break;
            case GameState.GamePrepare:
                Debug.Log("GameState = GamePrepare");
   
                GUIBasePanel basegui = UIManager.Instance.GetGUIFromDic("MainMenu");
                if (basegui != null)
                {
                    MonoManager.Instance.StartCoroutine(UIManager.Instance.FadeUI(UIManager.Instance.panelDic["MainMenu"].uiGroup, 0, 1, 0, 1, () =>
                    {
                        UIManager.Instance.HiddenUI("MainMenu");

                        UIManager.Instance.ShowUI<LoadingText>("LoadingText", E_UI_Layer.Mid, this, (panel) =>
                        {
                            panel.Initialization();
                            //
                            MonoManager.Instance.StartCoroutine(LevelManager.Instance.LoadScene(1, (ac) =>
                            {

                            }));
                            MonoManager.Instance.StartCoroutine(LevelManager.Instance.LevelPreparing(() =>
                            {
                                GameEvent.Trigger(GameState.GameStart);
                            }));
                        });

                    }));
                }
                else
                {
                    UIManager.Instance.ShowUI<LoadingText>("LoadingText", E_UI_Layer.Mid,this, (panel) =>
                    {
                        panel.Initialization();
                        MonoManager.Instance.StartCoroutine(LevelManager.Instance.LevelPreparing(() =>
                        {
                            GameEvent.Trigger(GameState.GameStart);
                        }));
                    });
                }


                // 创建玩家,并且关闭玩家 
                //创建一些关键组件, 比如对象池,声音组件,

                break;
            case GameState.GameStart:
                Debug.Log("GameState = GameStart");

                MonoManager.Instance.StartDelay(3, () =>
                {
                    UIManager.Instance.HiddenUI("LoadingText");

                    GUIBasePanel basegui = UIManager.Instance.GetGUIFromDic("BackGround");
                    if(basegui != null)
                    {
                        MonoManager.Instance.StartCoroutine(UIManager.Instance.FadeUI(basegui.uiGroup, 0, 1, 0, 1, () =>
                        {
                            UIManager.Instance.HiddenUI("BackGround");

                            UIManager.Instance.ShowUI<PlayerHUD>("PlayerHUD", E_UI_Layer.Mid, this, (panel) =>
                            {
                                //UIManager.Instance.playerHUD = panel;
                                panel.Initialization();
                                LevelManager.Instance.StartSpawn();
                       
                                //LevelManager.Instance.player.gameObject.SetActive(true);
                            });

                        }));
                    }
                    else
                    {
                        UIManager.Instance.HiddenUI("BackGround");

                        UIManager.Instance.ShowUI<PlayerHUD>("PlayerHUD", E_UI_Layer.Mid, this, (panel) =>
                        {
                            //UIManager.Instance.playerHUD = panel;
                            panel.Initialization();
                            LevelManager.Instance.StartSpawn();
                
                            //LevelManager.Instance.player.gameObject.SetActive(true);
                        });

                    }

                });


                break;

            case GameState.GameOver:
                Debug.Log("GameState = GameOver");
                LevelManager.Instance.GameOver();
                
                UIManager.Instance.HiddenUI("PlayerHUD");
                UIManager.Instance.ShowUI<BackGroundPanel>("BackGround", E_UI_Layer.Bot, null);
                UIManager.Instance.ShowUI<Record>("Record", E_UI_Layer.Top, this, (panel) =>
                {
                    panel.Initialization();

                });
                break;
            case GameState.GameEnd:
                Debug.Log("GameState = GameEnd");
                UIManager.Instance.HiddenUI("Record");
                ScoreEvent.Trigger(ScoreEventType.Reset, 0);
                MonoManager.Instance.StartCoroutine(LevelManager.Instance.LoadScene(0, (ac) => 
                {
                    GameEvent.Trigger(GameState.MainMenu);
                }));

                break;
            case GameState.GameReset:
                Debug.Log("GameState = GameReset");
                ScoreEvent.Trigger(ScoreEventType.Reset, 0);
                UIManager.Instance.HiddenUI("Record");
                GameEvent.Trigger(GameState.GamePrepare);
                break;

        }
    }

    public void OnEvent(ScoreEvent evt)
    {
        switch (evt.type)
        {
            case ScoreEventType.Change:
                score += evt.scorechange;
                (UIManager.Instance.panelDic["PlayerHUD"] as PlayerHUD).ChangeScore(score);
                break;
            case ScoreEventType.Reset:
                score = 0;
                break;
        }
       

    }



}
