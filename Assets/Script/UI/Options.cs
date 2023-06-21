using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Options : GUIBasePanel
{


    public override void Initialization()
    {
        base.Initialization();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    public void GameSettingButtonPressed()
    {

    }

    public void ControlButtonPressed()
    {
        UIManager.Instance.ShowUI<KeyBinding>("KeyBinding", E_UI_Layer.Mid,owner,(panel) =>
        {
            panel.Initialization();
        });
        UIManager.Instance.HiddenUI("Options");
    }

    public void SoundButtonPressed()
    {

    }


    public void AcceptButtonPressed()
    {

    }

    public void BackButtonPressed()
    {

        UIManager.Instance.ShowUI<MainMenu>("MainMenu", E_UI_Layer.Mid,owner, (panel) => 
        {
            panel.Initialization();
        });
        UIManager.Instance.HiddenUI("Options");
    }
}
