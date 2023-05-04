using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;



public class ControlSetting : GUIBasePanel
{
    public Button Forward;
    public Button Jump;



    public bool iskeyboard = true;


    private Button _currentSettingKey;
    private InputAction _action;
    private bool _iswattingforkeyinput;
    public override void Initialization()
    {
        base.Initialization();
        _action = new InputAction("anykey", InputActionType.PassThrough, "*/<Button>");
        _action.Enable();

       

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {

    }
    public void JumpSettingButtonPressed(string keyname)
    {
        Debug.Log(keyname + " is setting..");
        Jump.GetComponentInChildren<Text>().text = "press a key..";
        _iswattingforkeyinput = true;
        _currentSettingKey = GetCurrentSettingKey(keyname);
        StartCoroutine("WaittingForKeyInput");

    }

    public void ForwardSettingButtonPressed(string keyname)
    {
        Debug.Log(keyname + " is setting..");
        Forward.GetComponentInChildren<Text>().text = "press a key..";
        _iswattingforkeyinput = true;
        _currentSettingKey = GetCurrentSettingKey(keyname);
        StartCoroutine("WaittingForKeyInput");
    }


    public void AcceptButtonPressed()
    {

    }

    public void BackButtonPressed()
    {
        UIManager.Instance.ShowUI<Options>("Options", E_UI_Layer.Mid, (panel) => 
        {
            panel.Initialization();
        });
        UIManager.Instance.HiddenUI("ControlSetting");
    }

    IEnumerator WaittingForKeyInput()
    {
        yield return null;
        while(_iswattingforkeyinput)
        {
            if(_action.triggered)
            {
                Debug.Log("_action = " + _action.activeControl.path);
                _currentSettingKey.GetComponentInChildren<Text>().text = _action.activeControl.path.PathToKeyName();
                _iswattingforkeyinput = false;
                break;
            }
            yield return null;
        }
    }

    public Button GetCurrentSettingKey(string keyname)
    {
        switch(keyname)
        {
            case "Forward":
                return Forward;

            case "Jump":
                return Jump;
        }

        return null;
    }
}
