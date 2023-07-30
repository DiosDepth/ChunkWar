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
        GameEvent.Trigger(EGameState.EGameState_GameReset);
    }

    private void GiveUpClick()
    {
        GameEvent.Trigger(EGameState.EGameState_GameEnd);
    }

    public void UpdateRecord(int record)
    {
        TMP_Text title = GetGUIComponent<TMP_Text>("NewRecord");
        TMP_Text score = GetGUIComponent<TMP_Text>("Score");
        TMP_Text date = GetGUIComponent<TMP_Text>("Date");

        if(record > RogueManager.Instance.saveData.newRecord)
        {
            RogueManager.Instance.saveData.newRecord = record;
            RogueManager.Instance.saveData.date = System.DateTime.Now.ToString("yyyy:mm:dd");
            SaveLoadManager.Save(RogueManager.Instance.saveData, "SaveData");
            title.text = "New Record";
            score.text = RogueManager.Instance.saveData.newRecord.ToString();
            date.text = RogueManager.Instance.saveData.date;
        }
        else
        {
            title.text = "Your Record";
            score.text = record.ToString();
            date.text = System.DateTime.Now.ToString("yyyy:mm:dd");
        }

    }
}
