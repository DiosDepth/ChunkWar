using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


#if GMDEBUG
public class GMTalkManager : Singleton<GMTalkManager>
{
    private Dictionary<string, Func<string[], bool>> GMFunctionDic;

    private const int BattleTestCreateIndex_Start = 100000;

    private bool isShowGMTalkWindow = false;
    private bool _isCmdSuccess;

    private Dictionary<int, bool> toggleValueCache = new Dictionary<int, bool>();

    public bool IsEnemyImmortal
    {
        get;
        private set;
    }

    public override void Initialization()
    {
        base.Initialization();
        InitFunctionDic();

        MonoManager.Instance.AddUpdateListener(ListenerGMSwitch);

        InitialCmd();
    }

    private void InitFunctionDic()
    {
        GMFunctionDic = new Dictionary<string, Func<string[], bool>>();
        IsEnemyImmortal = false;
    }

    private void InitialCmd()
    {
        AddGMFunctionToDic("harbor", EnterHarbor);
        AddGMFunctionToDic("exp", AddEXP);
        AddGMFunctionToDic("waste", AddWaste);
        AddGMFunctionToDic("campscore", AddCampScore);
        AddGMFunctionToDic("win", Win);
        AddGMFunctionToDic("Shop", EnterShop);
        AddGMFunctionToDic("createBattleLog", CreateBattleLog);
        AddGMFunctionToDic("addPlug", AddPlug);
        AddGMFunctionToDic("saveGame", GM_SaveGame);
        AddGMFunctionToDic("EnemyImmortal", AllAIShip_Immortal);
        AddGMFunctionToDic("PlayerImmortal", PlayerShip_Immortal);
        AddGMFunctionToDic("EnemyUnImmortal", AllAIShip_UnImmortal);
        AddGMFunctionToDic("PlayerUnImmortal", PlayerShip_UnImmortal);
        AddGMFunctionToDic("addWreckage", AddWreckage);
        AddGMFunctionToDic("exportPlugPreset", ExportPlugPreset);
    }

    public void AddToggleStorage(int key, bool value)
    {
        if (toggleValueCache.ContainsKey(key))
        {
            toggleValueCache[key] = value;
        }
        else
        {
            toggleValueCache.Add(key, value);
        }
    }

    public bool GetToggleCacheValue(int key)
    {
        if (toggleValueCache.ContainsKey(key))
        {
            return toggleValueCache[key];
        }
        return false;
    }

    
    public void ListenerGMSwitch()
    {
        if(Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            SwicthGMPage();
        }
        else if (Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            ///一键杀敌
            KillAllEnemy();
        }
    }

    public void SwicthGMPage()
    {
        if (isShowGMTalkWindow)
        {
            UIManager.Instance.ShowUI<GMTalkMainPage>("GMTalkMainPage", E_UI_Layer.System, this, (panel) =>
            {
                panel.Initialization();
                InputDispatcher.Instance.ChangeInputMode("UI");
            });
        }
        else
        {
            UIManager.Instance.HiddenUI("GMTalkMainPage");
            InputDispatcher.Instance.ChangeInputMode("Player");
        }
        isShowGMTalkWindow = !isShowGMTalkWindow;
    }
    
    public void HandleGMTalkInputContent(string content)
    {
        var strlst = GetTextContentBySpaceSplit(content);
        if (strlst == null || strlst.Count <= 0)
            return;

        var keyWords = strlst[0];
        keyWords.ToLower();
        strlst.RemoveAt(0);

        if (GMFunctionDic.ContainsKey(keyWords))
        {
            var func = GMFunctionDic[keyWords];
            bool succ = func(strlst.ToArray());
        }
    }

    private List<string> GetTextContentBySpaceSplit(string content)
    {
        if (string.IsNullOrEmpty(content))
            return null;

        return content.Split(' ').ToList();
    }

    #region Function

    private void AddGMFunctionToDic(string key, Func<string[], bool> actions)
    {
        key.ToLower();
        if (GMFunctionDic.ContainsKey(key))
            return;

        GMFunctionDic.Add(key, actions);
    }

    private bool AddCampScore(string[] content)
    {
        if (content.Length != 2)
            return false;

        CampLevelUpInfo info = null;

        int.TryParse(content[0], out int campID);
        int.TryParse(content[1], out int value);
        var campData = GameManager.Instance.GetCampDataByID(campID);
        if(campData != null)
        {
            campData.AddCampScore(value, out info);
        }
        return true;
    }

    private bool Win(string[] content)
    {
        CloseGMTalkPage();
        GameEvent.Trigger(EGameState.EGameState_GameOver);
        return true;
    }

    private bool EnterShop(string[] content)
    {
        CloseGMTalkPage();
        RogueManager.Instance.EnterShop();
        return true;
    }

    private bool CreateBattleLog(string[] content)
    {
        CloseGMTalkPage();
        RogueManager.Instance.CreateBattleLog();
        return true;
    }

    private bool EnterHarbor(string[] content)
    {
        CloseGMTalkPage();
        if (LevelManager.Instance.IsBattleLevel())
        {
            RogueManager.Instance.OnWaveFinish();
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool AddEXP(string[] content)
    {
        CloseGMTalkPage();
        if (content.Length != 1)
            return false;
        int value = 0;
        int.TryParse(content[0], out value);
        RogueManager.Instance.AddEXP(value);
        return true;
    }

    private bool AddPlug(string[] content)
    {
        if (content.Length != 1)
            return false;
        int value = 0;
        int.TryParse(content[0], out value);
        RogueManager.Instance.AddNewShipPlug(value);
        return true;
    }

    private bool AddWreckage(string[] content)
    {
        if (content.Length != 1)
            return false;
        int value = 0;
        int.TryParse(content[0], out value);
        RogueManager.Instance.CreateAndAddNewWreckageInfo(value);
        return true;
    }

    private bool AddWaste(string[] content)
    {
        CloseGMTalkPage();
        if (content.Length != 1)
            return false;
        int value = 0;
        int.TryParse(content[0], out value);
        RogueManager.Instance.AddDropWasteCount(value);
        return true;
    }

    private bool GM_SaveGame(string[] content)
    {
        var fileName = System.DateTime.Now.ToString("yyyy-MM-dd HH_MM_ss");
        RogueManager.Instance.CreateInLevelSaveData(string.Format("GM_SAV_{0}", fileName), true);
        CloseGMTalkPage();
        return true;
    }

    /// <summary>
    /// 所有敌人无敌
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private bool AllAIShip_Immortal(string[] content)
    {
        IsEnemyImmortal = true;
        var allAIShip = ECSManager.Instance.activeAIAgentData.shipList;
        for(int i = 0; i < allAIShip.Count; i++)
        {
            allAIShip[i].ForceSetAllUnitState(DamagableState.Immortal);
        }

        return true;
    }

    private bool AllAIShip_UnImmortal(string[] content)
    {
        IsEnemyImmortal = false;
        var allAIShip = ECSManager.Instance.activeAIAgentData.shipList;
        for (int i = 0; i < allAIShip.Count; i++)
        {
            allAIShip[i].ForceSetAllUnitState(DamagableState.Normal);
        }

        return true;
    }

    private bool PlayerShip_Immortal(string[] content)
    {
        var playerShip = RogueManager.Instance.currentShip;
        if(playerShip != null)
        {
            playerShip.ForceSetAllUnitState(DamagableState.Immortal);
        }
        return true;
    }

    private bool PlayerShip_UnImmortal(string[] content)
    {
        var playerShip = RogueManager.Instance.currentShip;
        if (playerShip != null)
        {
            playerShip.ForceSetAllUnitState(DamagableState.Normal);
        }
        return true;
    }

    /// <summary>
    /// 导出插件预设
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private bool ExportPlugPreset(string[] content)
    {
        if (!RogueManager.Instance.InBattle)
            return false;
#if UNITY_EDITOR
        var obj = ScriptableObject.CreateInstance(typeof(ShipPlugPresetTestData));
        var data = obj as ShipPlugPresetTestData;
        data.Name = System.DateTime.Now.ToString("yyyy-MM-dd HH_MM_ss");
        data.ID = 99999;
        data.PlugItems = new List<ShipPlugPresetTestData.PlugTestItem>();

        var allPlugs = RogueManager.Instance.AllCurrentShipPlugs;
        for (int i = 0; i < allPlugs.Count; i++) 
        {
            var plugID = allPlugs[i].PlugID;
            ///剔除舰船插件
            if (plugID <= GameGlobalConfig.Ship_MainPlug_StartID)
                continue;

            var plug = data.PlugItems.Find(x => x.PlugID == plugID);
            if (plug != null)
            {
                plug.count++;
            }
            else
            {
                ShipPlugPresetTestData.PlugTestItem item = new ShipPlugPresetTestData.PlugTestItem
                {
                    count = 1,
                    PlugID = allPlugs[i].PlugID
                };
                data.PlugItems.Add(item);
            }
        }

        var targetPath = string.Format("{0}/{1}.asset", "Assets/EditorRes/Config/PlugPreset", data.Name);
        AssetDatabase.CreateAsset(obj, targetPath);
        EditorUtility.SetDirty(obj);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
        return true;
    }

    #endregion

    private void KillAllEnemy()
    {
        var allEnemy = ECSManager.Instance.activeAIAgentData.shipList;
        for(int i = allEnemy.Count - 1; i >= 0; i--)
        {
            allEnemy[i].ForceKill();
        }

        var allEnemyDrone = ECSManager.Instance.activeAIDroneAgentData.shipList;
        for(int i = allEnemyDrone.Count - 1;i >= 0; i--)
        {
            allEnemyDrone[i].ForceKill();
        }
    }

    public void CloseGMTalkPage()
    {
        if (UIManager.Instance.GetGUIFromDic<GMTalkMainPage>("GMTalkMainPage") == null)
            return;

        UIManager.Instance.HiddenUI("GMTalkMainPage");
        InputDispatcher.Instance.ChangeInputMode("Player");
        isShowGMTalkWindow = !isShowGMTalkWindow;
    }

    public void CreateEnemy(int shipID, int hardLevelID, int count)
    {
        if (!RogueManager.Instance.IsLevelSpawnVaild())
            return;

        WaveEnemySpawnConfig cfg = new WaveEnemySpawnConfig
        {
            AITypeID = shipID,
            DurationDelta = 0,
            LoopCount = 1,
            TotalCount = count,
            MaxRowCount = 2,
        };
        RogueManager.Instance.SpawnEntity(cfg, hardLevelID);
    }
}
#endif