using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if GMDEBUG
public class GMTalkMainPage : GUIBasePanel
{

    private InputField _inputFiled;

    protected override void Awake()
    {
        base.Awake();
        _inputFiled = transform.Find("Content/InputContent/GMCommandField").SafeGetComponent<InputField>();
    }

    public override void Initialization()
    {
        GetGUIComponent<Button>("Close").onClick.AddListener(ClosePage);
        GetGUIComponent<Button>("Shop").onClick.AddListener(JumpToShop);
        GetGUIComponent<Button>("Vectory").onClick.AddListener(Vectory);
        GetGUIComponent<Button>("Harbor").onClick.AddListener(EnterHarbor);
        GetGUIComponent<Button>("SaveGame").onClick.AddListener(() =>
        {
            GMTalkManager.Instance.HandleGMTalkInputContent("saveGame");
        });
        GetGUIComponent<Button>("CreateBattleLog").onClick.AddListener(() =>
        {
            GMTalkManager.Instance.HandleGMTalkInputContent("createBattleLog");
        });
        GetGUIComponent<Button>("SendGMBtn").onClick.AddListener(() =>
        {
            GMTalkManager.Instance.HandleGMTalkInputContent(_inputFiled.text);
        });
        GetGUIComponent<Button>("ExportPlugPreset").onClick.AddListener(() =>
        {
            GMTalkManager.Instance.HandleGMTalkInputContent("exportPlugPreset");
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
        GMTalkManager.Instance.CloseGMTalkPage();
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
#endif