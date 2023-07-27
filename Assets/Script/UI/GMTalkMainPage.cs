using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GMTalkMainPage : GUIBasePanel
{
    // Start is called before the first frame update
    public override void Initialization()
    {
        GetGUIComponent<Button>("SendGMBtn").onClick.AddListener(SendGMBtnPressed);
    }

    private void SendGMBtnPressed()
    {
        GMTalkManager.Instance.HandleGMTalkInputContent(GetGUIComponent<InputField>("GMCommandField").text);
    }

    public override void Show()
    {
        base.Show();
    }

    public override void Hidden()
    {
        base.Hidden();
    }

}
