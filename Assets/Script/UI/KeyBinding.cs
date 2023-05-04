using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;

public enum BindingScheme
{
    Keyboard,
    Gamepad,
}

public class SchemeChangeEvent : UnityEvent<string>{ }
public class KeyBinding : GUIBasePanel
{
    
    public UnityAction<string> OnSchemeChange;
    private BindingScheme scheme = BindingScheme.Keyboard;
    public GameObject overlay;
    public string Scheme
    {
        get
        {
            string name = "";
            switch (scheme)
            {
                case BindingScheme.Keyboard:
                    name = "Keyboard";
                    break;
                case BindingScheme.Gamepad:
                    name = "Gamepad";
                    break;
            }
            return name;
        }
    }
    public Binding[] bingdingsUI;
    public override void Initialization()
    {
        base.Initialization();
        
        bingdingsUI = GetComponentsInChildren<Binding>();
        //遍历所有的Binding，并且收集他们的基本信息
        for (int i = 0; i < bingdingsUI.Length; i++)
        {
            bingdingsUI[i].Initialization();
            OnSchemeChange +=bingdingsUI[i].RefreshBindinginfo;
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Show()
    {
        base.Show();
    }

    public override void Hidden(string m_uiname)
    {
        base.Hidden(m_uiname);
    }

    public void Accept()
    {
        KeyBindingManager.Instance.SaveBinding();
    }
    public void Back()
    {
        KeyBindingManager.Instance.ResetKeyBindings();

        UIManager.Instance.ShowUI<Options>("Options", E_UI_Layer.Mid, (panel) =>
        {
            panel.Initialization();
        });
        UIManager.Instance.HiddenUI("KeyBinding");
    }

    public void ToKeyboard()
    {
        scheme = BindingScheme.Keyboard;
        if(OnSchemeChange != null)
        {
            OnSchemeChange.Invoke(GetSchemeName());
        }
    }

    public void ToGamepad()
    {
        scheme = BindingScheme.Gamepad;
        if (OnSchemeChange != null)
        {
            OnSchemeChange.Invoke(GetSchemeName());
        }
    }

    public void UpdateBindings()
    {

    }

    public string GetSchemeName()
    {
        string name = "";
        switch (scheme)
        {
            case BindingScheme.Keyboard:
                name = "Keyboard";
                break;
            case BindingScheme.Gamepad:
                name =  "Gamepad";
                break;
        }
        return name;
    }

    public void ShowOverLay(string text = "Press Key...")
    {
        overlay.GetComponentInChildren<Text>().text = text;
        overlay.SetActive(true);
    }

    public void HiddenOverLay()
    {
        overlay.SetActive(false);
    }
}