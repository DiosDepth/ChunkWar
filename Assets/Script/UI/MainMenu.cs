using System.Collections;
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
        LoadSaveData();
    }

    public override void Show()
    {
        base.Show();
    }

    public void PlayButtonPressed()
    {
        GameEvent.Trigger(GameState.GamePrepare);

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


        GameManager.Instance.saveData = SaveLoadManager.Load("SaveData") as SaveData;

        if (GameManager.Instance.saveData == null)
        {
            GameManager.Instance.saveData = new SaveData();
            SaveLoadManager.Save(GameManager.Instance.saveData, "SaveData");
        }


        UpdateNewRecord();
    }

    public void UpdateNewRecord()
    {
        TMP_Text scoretmp = GetGUIComponent<TMP_Text>("Score");
        TMP_Text datetmp = GetGUIComponent<TMP_Text>("Date");
        scoretmp.text = GameManager.Instance.saveData.newRecord.ToString();
        datetmp.text = GameManager.Instance.saveData.date.ToString();
    }
}
