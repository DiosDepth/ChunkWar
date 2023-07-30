using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.InputSystem;

public class GMTalkManager : Singleton<GMTalkManager>
{
    private const string Key_FetchReward = "FetchReward";
    private const string Key_AddMaterial = "AddMaterial";

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

        /// Cmd : jump to shop
        AddGMFunctionToDic("jumptoshop", (starry) => 
        {
            if(LevelManager.Instance.currentLevel.levelName == AvaliableLevel.BattleLevel_001.ToString())
            {
                GameStateTransitionEvent.Trigger(EGameState.EGameState_GameCompleted);
                return true;
            }
            else
            {
                return false;
            }
 
        });

        AddGMFunctionToDic("exp", (param) =>
        {
            if (param.Length != 1)
                return false;
            int value = 0;
            int.TryParse(param[0], out value);
            RogueManager.Instance.AddEXP(value);
            return true;
        });

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
        if (GMFunctionDic.ContainsKey(key))
            return;

        GMFunctionDic.Add(key, actions);
    }

    #endregion
}