using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Events;

public class MainMenu : GUIBasePanel
{

    public override void Initialization()
    {
        base.Initialization();
        GetGUIComponent<Button>("Play").onClick.AddListener(PlayButtonPressed);
        GetGUIComponent<Button>("Options").onClick.AddListener(OptionButtonPressed);
        GetGUIComponent<Button>("Quit").onClick.AddListener(QuitButtonPressed);
        GetGUIComponent<Button>("Load").onClick.AddListener(LoadSaveButtonPressed);
        GetGUIComponent<Button>("Collection").onClick.AddListener(CollectionButtonPressed);
    }

    public override void Show()
    {
        base.Show();
    }

    public void PlayButtonPressed()
    {
        GameStateTransitionEvent.Trigger(EGameState.EGameState_ShipSelection);

    }





    public void OptionButtonPressed()
    {
        UIManager.Instance.ShowUI<Options>("Options", E_UI_Layer.Mid,owner, (panel) => 
        {
            panel.Initialization();
        });
        UIManager.Instance.HiddenUI("MainMenu");
    }

    public void QuitButtonPressed()
    {
        Application.Quit();
    }

    private void LoadSaveButtonPressed()
    {
        UIManager.Instance.ShowUI<GameSavePage>("GameSavePage", E_UI_Layer.Mid, owner, (panel) =>
        {
            panel.Initialization();
        });
    }

    private void CollectionButtonPressed()
    {

    }

}
