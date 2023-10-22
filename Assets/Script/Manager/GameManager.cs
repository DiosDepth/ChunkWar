using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum EGameState
{
    EGameState_None,
    EGameState_WelcomScreen,
    EGameState_MainMenu,
    EGameState_ShipSelection,
    EGameState_GamePrepare,
    EGameState_GameStart,
    EGameState_GameHarbor,
    EGameState_GameOver,
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


public class GameManager : Singleton<GameManager>, EventListener<GameEvent>,EventListener<GameStateTransitionEvent>
{
    public StateMachine<EGameState> gamestate = new StateMachine<EGameState>(null, true, true);
    public List<IPauseable> pauseableList = new List<IPauseable>();


    private bool isInitialCompleted = false;

    /// <summary>
    /// 难度等级信息
    /// </summary>
    private List<HardLevelInfo> _hardLevels;
    public List<HardLevelInfo> GetAllHardLevelInfos
    {
        get { return _hardLevels; }
    }

    /// <summary>
    /// 阵营信息, Global
    /// </summary>
    private Dictionary<int, CampData> _campDatas;
    public List<CampData> GetAllCampData
    {
        get { return _campDatas.Values.ToList(); }
    }

    protected bool _isGamePaused;

    public static string GetGameVersionString
    {
        get
        {
            return string.Format("{0}.{1}", GameVersion.ToString("f1"), InternalVersion);
        }
    }

    /// <summary>
    /// 主版本号
    /// </summary>
    public static float GameVersion
    {
        get { return m_gameVersion; }
    }

    private static float m_gameVersion = 0.1f;

    /// <summary>
    /// 内部版本号
    /// </summary>
    public static int InternalVersion
    {
        get { return m_InternalVersion; }
    }
    private static int m_InternalVersion = 0;


    public GameManager()
    {
        Application.targetFrameRate = 120;

        this.EventStartListening<GameEvent>();
        this.EventStartListening<GameStateTransitionEvent>();
        LocalizationManager.Instance.SetLanguage(SystemLanguage.ChineseSimplified);
        SoundManager.Instance.Initialization();

#if GMDEBUG
        GMTalkManager.Instance.Initialization();
#endif
    }

    public void OnEvent(GameEvent evt)
    {
        gamestate.ChangeState(evt.gamesate);
    }

    public void OnEvent(GameStateTransitionEvent evt)
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
            case EGameState.EGameState_GameOver:
                GameEvent.Trigger(EGameState.EGameState_GameOver);
                break;
            case EGameState.EGameState_GameHarbor:
                GameEvent.Trigger(EGameState.EGameState_GameHarbor);
                break;
        }
    }

    public void InitData()
    {
        if (isInitialCompleted)
            return;

        InitHardLevelData();
        InitCampData();
        isInitialCompleted = true;
    }

    public void ClearBattle()
    {
        LevelManager.Instance.UnloadCurrentLevel();
        RogueManager.Instance.Clear();
        LevelManager.Instance.Clear();
        ECSManager.Instance.UnLoad();
        AchievementManager.Instance.ClearRuntimeData();
        PoolManager.Instance.RecycleAndClearAll();
    }

    public bool IsPauseGame()
    {
        return _isGamePaused;
    }

    public void PauseGame()
    {
        _isGamePaused = true;
        if (pauseableList == null) { return; }
        if(pauseableList.Count == 0) { return; }
    
        for (int i = 0; i < pauseableList.Count; i++)
        {
            pauseableList[i].PauseGame();
        }
        Time.timeScale = 0;
    }

    public void UnPauseGame()
    {
        _isGamePaused = false;
        if (pauseableList == null) { return; }
        if (pauseableList.Count == 0) { return; }
        for (int i = 0; i < pauseableList.Count; i++)
        {
            pauseableList[i].UnPauseGame();
        }
        Time.timeScale = 1;
    }


    public void RegisterPauseable(IPauseable pauseable)
    {
        if (pauseableList.Contains(pauseable)) { return; }
        pauseableList.Add(pauseable);
    }

    public void UnRegisterPauseable(IPauseable pauseable)
    {
        if (!pauseableList.Contains(pauseable)) { return; }
        pauseableList.Remove(pauseable);
    }

    #region HardLevel

    public HardLevelInfo GetHardLevelInfoByID(int hardLevelID)
    {
        return _hardLevels.Find(x => x.HardLevelID == hardLevelID);
    }

    /// <summary>
    /// 刷新指定舰船的hardlevel信息
    /// </summary>
    /// <param name="shipID"></param>
    public void RefreshHardLevelByShip(int shipID)
    {
        for(int i = 0; i < _hardLevels.Count; i++)
        {
            _hardLevels[i].RefreshShipHardLevel(shipID);
        }
    }

    private void InitHardLevelData()
    {
        _hardLevels = new List<HardLevelInfo>();
        var allhardLevels = DataManager.Instance.battleCfg.HardLevels;
        for (int i = 0; i < allhardLevels.Count; i++)
        {
            HardLevelInfo info = new HardLevelInfo(allhardLevels[i]);
            _hardLevels.Add(info);
        }
    }

    #endregion

    #region Camp
    /// <summary>
    /// 获取阵营数据
    /// </summary>
    /// <param name="campID"></param>
    /// <returns></returns>
    public CampData GetCampDataByID(int campID)
    {
        if (_campDatas.ContainsKey(campID))
            return _campDatas[campID];
        return null;
    }

    public List<CampSaveData> CreateCampSaveDatas()
    {
        List<CampSaveData> result = new List<CampSaveData>();
        foreach(var campData in _campDatas.Values)
        {
            CampSaveData data = campData.CreateCampSaveData();
            result.Add(data);
        }
        return result;
    }

    /// <summary>
    /// 初始化阵营数据
    /// </summary>
    private void InitCampData()
    {
        _campDatas = new Dictionary<int, CampData>();
        var allCamps = DataManager.Instance.GetAllCampConfigs();
        for(int i = 0; i < allCamps.Count; i++)
        {
            CampData data = CampData.CreateData(allCamps[i]);
            _campDatas.Add(data.CampID, data);
        }
    }

    #endregion
}