using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GMTalkMainPage : GUIBasePanel
{
    private InputField _filed;

    public override void Initialization()
    {
        GetGUIComponent<Button>("SendGMBtn").onClick.AddListener(SendGMBtnPressed);
        GetGUIComponent<Button>("Close").onClick.AddListener(ClosePage);
        GetGUIComponent<Button>("Shop").onClick.AddListener(JumpToShop);
        _filed = transform.Find("Content/InputContent/GMCommandField").SafeGetComponent<InputField>();
    }

    private void SendGMBtnPressed()
    {
        GMTalkManager.Instance.HandleGMTalkInputContent(_filed.text);
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
        GMTalkManager.Instance.HandleGMTalkInputContent("jumptoshop");
    }
}
