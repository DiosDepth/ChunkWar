using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.InputSystem;

public class GMTalkManager : Singleton<GMTalkManager>
{
    private Dictionary<string, Func<string[], bool>> GMFunctionDic;

    private const int BattleTestCreateIndex_Start = 100000;

    private bool isShowGMTalkWindow = false;
    private bool _isCmdSuccess;

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
    }

    private void InitialCmd()
    {
        AddGMFunctionToDic("harbor", EnterHarbor);
        AddGMFunctionToDic("exp", AddEXP);
        AddGMFunctionToDic("waste", AddWaste);
        AddGMFunctionToDic("campscore", AddCampScore);
        AddGMFunctionToDic("win", Win);
        AddGMFunctionToDic("Shop", EnterShop);
    }

    
    public void ListenerGMSwitch()
    {
        if(Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            if(isShowGMTalkWindow)
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
        UIManager.Instance.HiddenUI("GMTalkMainPage");
        GameEvent.Trigger(EGameState.EGameState_GameOver);
        return true;
    }

    private bool EnterShop(string[] content)
    {
        UIManager.Instance.HiddenUI("GMTalkMainPage");
        RogueManager.Instance.EnterShop();
        return true;
    }

    private bool EnterHarbor(string[] content)
    {
        UIManager.Instance.HiddenUI("GMTalkMainPage");
        if (LevelManager.Instance.currentLevel.levelName == AvaliableLevel.BattleLevel_001.ToString())
        {
            GameStateTransitionEvent.Trigger(EGameState.EGameState_GameHarbor);
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool AddEXP(string[] content)
    {
        UIManager.Instance.HiddenUI("GMTalkMainPage");
        if (content.Length != 1)
            return false;
        int value = 0;
        int.TryParse(content[0], out value);
        RogueManager.Instance.AddEXP(value);
        return true;
    }

    private bool AddWaste(string[] content)
    {
        UIManager.Instance.HiddenUI("GMTalkMainPage");
        if (content.Length != 1)
            return false;
        int value = 0;
        int.TryParse(content[0], out value);
        RogueManager.Instance.AddDropWasteCount(value);
        return true;
    }

    #endregion

}