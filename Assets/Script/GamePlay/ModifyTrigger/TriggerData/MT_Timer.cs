using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MT_Timer : ModifyTriggerData
{
    private MTC_Timer _timerCfg;

    private float _timer;
    private float _timeDelta;

    private bool _requireUpdate = false;

    private int addTime;

    public MT_Timer(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _timerCfg = cfg as MTC_Timer;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        addTime = RogueManager.Instance.Timer.TotalSeconds;
        if (_timerCfg.TimerTriggerType == MTC_Timer.TimerType.OnceTime)
        {
            RogueManager.Instance.Timer.OnTimeSecondUpdate += OnSpecialTimeUpdate;
        }
        else if (_timerCfg.TimerTriggerType == MTC_Timer.TimerType.EveryPerTime)
        {
            _requireUpdate = true;
            _timeDelta = _timerCfg.TimeParam;
        }
    }

    public override void OnUpdateBattle()
    {
        base.OnUpdateBattle();
        if (!_requireUpdate)
            return;

        _timer += Time.deltaTime;
        if(_timer > _timeDelta)
        {
            _timer = 0;
            Trigger();
        }
    }

    public override void Reset()
    {
        base.Reset();
        _timer = 0;
    }

    private void OnSpecialTimeUpdate(int time)
    {
        if(_timerCfg.SpecialType == MTC_Timer.SpecialTimeType.LastSeconds)
        {
            if(time <= _timerCfg.TimeParam && currentTriggerCount == 0)
            {
                Trigger();
            }
        }
        else if(_timerCfg.SpecialType == MTC_Timer.SpecialTimeType.StartSeconds)
        {
            var totalSecond = RogueManager.Instance.Timer.TotalSeconds;
            if(totalSecond >= _timerCfg.TimeParam && currentTriggerCount == 0)
            {
                Trigger();
            }
        }
        else
        {
            var totalSecond = RogueManager.Instance.Timer.TotalSeconds;
            if(totalSecond >= _timerCfg.TimeParam + addTime && currentTriggerCount == 0)
            {
                Trigger();
            }
        }
    }
}
