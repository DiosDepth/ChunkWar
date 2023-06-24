using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GameState
{
    WelcomScreen,
    MainMenu,
    ShipSelection,
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
  
    public string playerPrefabPath = "Prefab/Chunk/ShipContainer";
    public GameEntity gameEntity;



    public bool isInitialCompleted = false;







    public GameManager()
    {
        //Initialization();
        this.EventStartListening<GameEvent>();
        this.EventStartListening<ScoreEvent>();
        gameEntity = new GameEntity();
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
        gameEntity.gamestate.ChangeState(evt.gamesate);
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
                            LeanTween.scale(panel.uiGroup.gameObject, Vector3.zero, 0.25f).setOnComplete(() => 
                            {
                                UIManager.Instance.HiddenUI("WellcomScreen");
                                GameEvent.Trigger(GameState.MainMenu);
                            });
                        
                        });
                    }));
                });
                break;
            case GameState.MainMenu:
                Debug.Log("GameState = MainMenu");
                UIManager.Instance.ShowUI<MainMenu>("MainMenu", E_UI_Layer.Mid,this, (panel) =>
                {
                    panel.uiGroup.alpha = 0;
                    LeanTween.value(0, 1, 0.25f).setOnUpdate((value) => 
                    {
                        panel.uiGroup.alpha = value;
                    });
                });
                break;

            case GameState.ShipSelection:
                Debug.Log("GameState = ShipSelection");
                UIManager.Instance.ShowUI<LoadingScreen>("LoadingScreen", E_UI_Layer.Top, this, (panel) =>
                {
                    panel.Initialization();
                    panel.CloseLoadingDoor(() =>
                    {
                        UIManager.Instance.HiddenUI("MainMenu");
                        MonoManager.Instance.StartCoroutine(LevelManager.Instance.LoadScene(1, (ac) =>
                        {
                            MonoManager.Instance.StartCoroutine(LevelManager.Instance.LevelPreparing("ShipSelectionLevel"));

                            LeanTween.delayedCall(1, () =>
                            {

                            });
                        }));

                    });

                });
                break;
            case GameState.GamePrepare:
                Debug.Log("GameState = GamePrepare");
                
                //MonoManager.Instance.StartCoroutine(LevelManager.Instance.LoadScene(1, (ac) =>
                //{

                //}));
                //MonoManager.Instance.StartCoroutine(LevelManager.Instance.LevelPreparing(() =>
                //{
                //    GameEvent.Trigger(GameState.GameStart);
                //}));

                // 创建玩家,并且关闭玩家 
                //创建一些关键组件, 比如对象池,声音组件,

                break;
            case GameState.GameStart:
                Debug.Log("GameState = GameStart");

                MonoManager.Instance.StartDelay(3, () =>
                {
                    UIManager.Instance.HiddenUI("LoadingText");

                    GUIBasePanel gui = UIManager.Instance.GetGUIFromDic("BackGround");
                    if(gui != null)
                    {
                        MonoManager.Instance.StartCoroutine(UIManager.Instance.FadeUI(gui.uiGroup, 0, 1, 0, 1, () =>
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
                gameEntity.score += evt.scorechange;
                (UIManager.Instance.panelDic["PlayerHUD"] as PlayerHUD).ChangeScore(gameEntity.score);
                break;
            case ScoreEventType.Reset:
                gameEntity.score = 0;
                break;
        }
       

    }



}
