using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class GMTalkManager : Singleton<GMTalkManager>
{
    private const string Key_FetchReward = "FetchReward";
    private const string Key_AddMaterial = "AddMaterial";

    private Dictionary<string, Func<string[], bool>> GMFunctionDic;

    private const int BattleTestCreateIndex_Start = 100000;

    public override void Initialization()
    {
        base.Initialization();
        InitFunctionDic();
    }

    private void InitFunctionDic()
    {
        GMFunctionDic = new Dictionary<string, Func<string[], bool>>();
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