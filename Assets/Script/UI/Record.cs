using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Record : GUIBasePanel
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
        UpdateRecord(GameManager.Instance.gameEntity.score);
        GetGUIComponent<Button>("TryAgain").onClick.AddListener(TryAgainClick);
        GetGUIComponent<Button>("GiveUp").onClick.AddListener(GiveUpClick);

    }

    private void TryAgainClick()
    {
        GameEvent.Trigger(GameState.GameReset);
    }

    private void GiveUpClick()
    {
        GameEvent.Trigger(GameState.GameEnd);
    }

    public void UpdateRecord(int record)
    {
        TMP_Text title = GetGUIComponent<TMP_Text>("NewRecord");
        TMP_Text score = GetGUIComponent<TMP_Text>("Score");
        TMP_Text date = GetGUIComponent<TMP_Text>("Date");

        if(record > GameManager.Instance.gameEntity.saveData.newRecord)
        {
            GameManager.Instance.gameEntity.saveData.newRecord = record;
            GameManager.Instance.gameEntity.saveData.date = System.DateTime.Now.ToString("yyyy:mm:dd");
            SaveLoadManager.Save(GameManager.Instance.gameEntity.saveData, "SaveData");
            title.text = "New Record";
            score.text = GameManager.Instance.gameEntity.saveData.newRecord.ToString();
            date.text = GameManager.Instance.gameEntity.saveData.date;
        }
        else
        {
            title.text = "Your Record";
            score.text = record.ToString();
            date.text = System.DateTime.Now.ToString("yyyy:mm:dd");
        }

    }
}
