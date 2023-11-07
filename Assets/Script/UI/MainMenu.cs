using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : GUIBasePanel
{
    private TextMeshProUGUI _versionText;


    protected override void Awake()
    {
        base.Awake();
        _versionText = transform.Find("uiGroup/Version/Value").SafeGetComponent<TextMeshProUGUI>();
    }

    public override void Initialization()
    {
        base.Initialization();
        SetUpVersion();
    }

    public override void Show()
    {
        base.Show();
    }

    private void SetUpVersion()
    {
        _versionText.text = GameManager.GetGameVersionString;
    }
   
}
