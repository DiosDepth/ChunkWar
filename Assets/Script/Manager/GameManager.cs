using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum EGameState
{
    EGameState_None,
    EGameState_WelcomScreen,
    EGameState_MainMenu,
    EGameState_ShipSelection,
    EGameState_GamePrepare,
    EGameState_GameStart,
    EGameState_GamePause,
    EGameState_GameUnPause,
    EGameState_GameCompleted,
    EGameState_GameOver,
    EGameState_GameEnd,
    EGameState_GameReset,
}

public enum ScoreEventType
{
    Change,
    Reset,
}

public struct GameStateTransitionEvent
{
    public EGameState targetState;
    public GameStateTransitionEvent(EGameState m_state)
    {
        targetState = m_state;
    }

    public static GameStateTransitionEvent e;
    public static void Trigger(EGameState m_state)
    {
        e.targetState = m_state;
        EventCenter.Instance.TriggerEvent<GameStateTransitionEvent>(e);
    }
}

public struct GameEvent
{
    public EGameState gamesate;
    public GameEvent(EGameState m_state)
    {
        gamesate = m_state;
    }
    public static GameEvent e;
    public static void Trigger(EGameState m_state)
    {
        e.gamesate = m_state;
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

public class GameManager : Singleton<GameManager>, EventListener<GameEvent>,EventListener<ScoreEvent>,EventListener<GameStateTransitionEvent>
{
    public StateMachine<EGameState> gamestate = new StateMachine<EGameState>(null, true, true);

    public GameEntity gameEntity;



    public bool isInitialCompleted = false;

    public GameManager()
    {
        Application.targetFrameRate = 120;

        //Initialization();
        this.EventStartListening<GameEvent>();
        this.EventStartListening<ScoreEvent>();
        this.EventStartListening<GameStateTransitionEvent>();
        gameEntity = new GameEntity();

        Initialization();
        LocalizationManager.Instance.SetLanguage(SystemLanguage.ChineseSimplified);
        GMTalkManager.Instance.Initialization();
    }

    public override void Initialization()
    {
        base.Initialization();
        gameEntity.Initialization();
        isInitialCompleted = true;
        Debug.Log("GameManager Initialization");

    }
    public void OnEvent(GameEvent evt)
    {
        gamestate.ChangeState(evt.gamesate);
        //switch (evt.gamesate)
        //{
        //    case EGameState.EGameState_WelcomScreen:

        //        break;
        //    case EGameState.EGameState_MainMenu:

        //        break;

        //    case EGameState.EGameState_ShipSelection:

        //        break;
        //    case EGameState.EGameState_GamePrepare:
        //        Debug.Log("GameState = GamePrepare");
                
        //        //MonoManager.Instance.StartCoroutine(LevelManager.Instance.LoadScene(1, (ac) =>
        //        //{

        //        //}));
        //        //MonoManager.Instance.StartCoroutine(LevelManager.Instance.LevelPreparing(() =>
        //        //{
        //        //    GameEvent.Trigger(GameState.GameStart);
        //        //}));

        //        // 创建玩家,并且关闭玩家 
        //        //创建一些关键组件, 比如对象池,声音组件,

        //        break;
        //    case EGameState.EGameState_GameStart:
        //        Debug.Log("GameState = GameStart");

        //        MonoManager.Instance.StartDelay(3, () =>
        //        {
        //            UIManager.Instance.HiddenUI("LoadingText");

        //            GUIBasePanel gui = UIManager.Instance.GetGUIFromDic("BackGround");
        //            if(gui != null)
        //            {
        //                MonoManager.Instance.StartCoroutine(UIManager.Instance.FadeUI(gui.uiGroup, 0, 1, 0, 1, () =>
        //                {
        //                    UIManager.Instance.HiddenUI("BackGround");

        //                    UIManager.Instance.ShowUI<PlayerHUD>("PlayerHUD", E_UI_Layer.Mid, this, (panel) =>
        //                    {
        //                        //UIManager.Instance.playerHUD = panel;
        //                        panel.Initialization();
        //                        LevelManager.Instance.StartSpawn();
                       
        //                        //LevelManager.Instance.player.gameObject.SetActive(true);
        //                    });

        //                }));
        //            }
        //            else
        //            {
        //                UIManager.Instance.HiddenUI("BackGround");

        //                UIManager.Instance.ShowUI<PlayerHUD>("PlayerHUD", E_UI_Layer.Mid, this, (panel) =>
        //                {
        //                    //UIManager.Instance.playerHUD = panel;
        //                    panel.Initialization();
        //                    LevelManager.Instance.StartSpawn();
                
        //                    //LevelManager.Instance.player.gameObject.SetActive(true);
        //                });

        //            }

        //        });


        //        break;

        //    case EGameState.EGameState_GameOver:
        //        Debug.Log("GameState = GameOver");
        //        LevelManager.Instance.GameOver();
                
        //        UIManager.Instance.HiddenUI("PlayerHUD");
        //        UIManager.Instance.ShowUI<BackGroundPanel>("BackGround", E_UI_Layer.Bot, null);
        //        UIManager.Instance.ShowUI<Record>("Record", E_UI_Layer.Top, this, (panel) =>
        //        {
        //            panel.Initialization();

        //        });
        //        break;
        //    case EGameState.EGameState_GameEnd:
        //        Debug.Log("GameState = GameEnd");
        //        UIManager.Instance.HiddenUI("Record");
        //        ScoreEvent.Trigger(ScoreEventType.Reset, 0);
        //        MonoManager.Instance.StartCoroutine(LevelManager.Instance.LoadScene(0, (ac) => 
        //        {
        //            GameEvent.Trigger(EGameState.EGameState_MainMenu);
        //        }));

        //        break;
        //    case EGameState.EGameState_GameReset:
        //        Debug.Log("GameState = GameReset");
        //        ScoreEvent.Trigger(ScoreEventType.Reset, 0);
        //        UIManager.Instance.HiddenUI("Record");
        //        GameEvent.Trigger(EGameState.EGameState_GamePrepare);
        //        break;

        //}
    }

    public void InitialRuntimeData()
    {
        gameEntity.runtimeData = new RuntimeData((RogueManager.Instance.currentShipSelection.itemconfig as PlayerShipConfig).Map);
    }

    public void OnEvent(ScoreEvent evt)
    {
        switch (evt.type)
        {
            case ScoreEventType.Change:
                gameEntity.score += evt.scorechange;
                break;
            case ScoreEventType.Reset:
                gameEntity.score = 0;
                break;
        }
    }

    public void OnEvent(GameStateTransitionEvent evt)
    {
        UIManager.Instance.ShowUI<LoadingScreen>("LoadingScreen", E_UI_Layer.Top, this, (panel) =>
        {
            panel.Initialization();
            panel.SetDoorState(true);
            panel.CloseLoadingDoor(()=> 
            {
                switch (evt.targetState)
                {
                    case EGameState.EGameState_None:
                        
                        break;
                    case EGameState.EGameState_WelcomScreen:
                        break;
                    case EGameState.EGameState_MainMenu:
                        GameEvent.Trigger(EGameState.EGameState_MainMenu);
                        break;
                    case EGameState.EGameState_ShipSelection:
                        GameEvent.Trigger(EGameState.EGameState_ShipSelection);
                        break;
                    case EGameState.EGameState_GamePrepare:
                        GameEvent.Trigger(EGameState.EGameState_GamePrepare);
                        break;
                    case EGameState.EGameState_GameStart:
                        GameEvent.Trigger(EGameState.EGameState_GameStart);
                        break;
                    case EGameState.EGameState_GamePause:
                        GameEvent.Trigger(EGameState.EGameState_GamePause);
                        break;
                    case EGameState.EGameState_GameUnPause:
                        GameEvent.Trigger(EGameState.EGameState_GameUnPause);
                        break;
                    case EGameState.EGameState_GameOver:
                        GameEvent.Trigger(EGameState.EGameState_GameOver);
                        break;
                    case EGameState.EGameState_GameCompleted:
                        GameEvent.Trigger(EGameState.EGameState_GameCompleted);
                        break;
                    case EGameState.EGameState_GameEnd:
                        GameEvent.Trigger(EGameState.EGameState_GameEnd);
                        break;
                    case EGameState.EGameState_GameReset:
                        GameEvent.Trigger(EGameState.EGameState_GameReset);
                        break;
                }
            });
        });

    }
}