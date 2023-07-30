﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Events;

public class MainMenu : GUIBasePanel
{



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Initialization()
    {
        base.Initialization();
        GetGUIComponent<Button>("Play").onClick.AddListener(PlayButtonPressed);
        GetGUIComponent<Button>("Options").onClick.AddListener(OptionButtonPressed);
        GetGUIComponent<Button>("Quit").onClick.AddListener(QuitButtonPressed);

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

    public void LoadSaveData()
    {


        RogueManager.Instance.saveData = SaveLoadManager.Load("SaveData") as SaveData;

        if (RogueManager.Instance.saveData == null)
        {
            RogueManager.Instance.saveData = new SaveData();
            SaveLoadManager.Save(RogueManager.Instance.saveData, "SaveData");
        }


 
    }


}
