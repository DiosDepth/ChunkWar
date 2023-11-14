using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleLogDialog : GUIBasePanel, EventListener<RogueEvent>
{
    public class BattleLogData
    {
        public int enemyCount;
        public float enemyThread;
        public int Frame;
    }

    private TextMeshProUGUI _frame;
    private TextMeshProUGUI _enemyCount;
    private TextMeshProUGUI _enemyThread;

    protected override void Awake()
    {
        base.Awake();
        _enemyCount = transform.Find("LogItem/Value").SafeGetComponent<TextMeshProUGUI>();
        _enemyThread = transform.Find("LogItem2/Value").SafeGetComponent<TextMeshProUGUI>();
        _frame = transform.Find("Frame/Value").SafeGetComponent<TextMeshProUGUI>();
    }

    public override void Initialization()
    {
        base.Initialization();
        _enemyCount.text = "0";
        _enemyThread.text = "0";
        _frame.text = ((int)(1f / Time.unscaledDeltaTime)).ToString();
        this.EventStartListening<RogueEvent>();
    }

    public override void Hidden()
    {
        base.Hidden();
        this.EventStopListening<RogueEvent>();
    }

    public override void Initialization(params object[] param)
    {
        base.Initialization(param);
    }

    public void OnEvent(RogueEvent evt)
    {
        switch (evt.type)
        {
            case RogueEventType.BattleLog:
                BattleLogData data = (BattleLogData)evt.param[0];
                RefreshBattleLog(data);
                break;
        }
    }

    private void RefreshBattleLog(BattleLogData data)
    {
        _frame.text = data.Frame.ToString();
        _enemyCount.text = data.enemyCount.ToString();
        _enemyThread.text = data.enemyThread.ToString();
    }

}
