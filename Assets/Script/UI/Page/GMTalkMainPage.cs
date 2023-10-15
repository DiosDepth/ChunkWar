using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GMTalkMainPage : GUIBasePanel
{

    public override void Initialization()
    {
        GetGUIComponent<Button>("Close").onClick.AddListener(ClosePage);
        GetGUIComponent<Button>("Shop").onClick.AddListener(JumpToShop);
        GetGUIComponent<Button>("Vectory").onClick.AddListener(Vectory);
        GetGUIComponent<Button>("Harbor").onClick.AddListener(EnterHarbor);
        GetGUIComponent<Button>("CreateBattleLog").onClick.AddListener(() =>
        {
            GMTalkManager.Instance.HandleGMTalkInputContent("createBattleLog");
        });
    }



    public override void Show()
    {
        base.Show();
    }

    public override void Hidden()
    {
        base.Hidden();
    }


    private void ClosePage()
    {
        UIManager.Instance.HiddenUI("GMTalkMainPage");
    }

    private void JumpToShop()
    {
        GMTalkManager.Instance.HandleGMTalkInputContent("Shop");
    }

    private void Vectory()
    {
        GMTalkManager.Instance.HandleGMTalkInputContent("win");
    }

    private void EnterHarbor()
    {
        GMTalkManager.Instance.HandleGMTalkInputContent("harbor");
    }
}
