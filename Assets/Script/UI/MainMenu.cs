using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : GUIBasePanel
{
    private TextMeshProUGUI _versionText;
    private Button _testButton;


    protected override void Awake()
    {
        base.Awake();
        _versionText = transform.Find("uiGroup/Version/Value").SafeGetComponent<TextMeshProUGUI>();
        _testButton = transform.Find("uiGroup/TestButton").SafeGetComponent<Button>();
        _testButton.transform.SafeSetActive(false);
    }

    public override void Initialization()
    {
        base.Initialization();
        SetUpVersion();
#if GMDEBUG
        _testButton.transform.SafeSetActive(true);
        _testButton.onClick.AddListener(OnTestButtonClick);
#endif
    }

    public override void Show()
    {
        base.Show();
    }

    private void SetUpVersion()
    {
        _versionText.text = GameManager.GetGameVersionString;
    }

#if GMDEBUG
    private void OnTestButtonClick()
    {
        UIManager.Instance.ShowUI<TestMainDialog>("TestMainDialog", E_UI_Layer.Top, null, (panel) =>
        {
            panel.Initialization();

        });
    }
#endif
}
