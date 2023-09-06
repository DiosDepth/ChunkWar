using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MT_WaveState : ModifyTriggerData
{
    private MTC_WaveState _waveConfig;

    public MT_WaveState(ModifyTriggerConfig cfg, uint uid) : base(cfg, uid)
    {
        _waveConfig = cfg as MTC_WaveState;
    }

    public override void OnTriggerAdd()
    {
        base.OnTriggerAdd();
        RogueManager.Instance.OnWaveStateChange += OnWaveStateChange;
    }

    private void OnWaveStateChange(bool isStart)
    {
        if(_waveConfig.IsStart == isStart)
        {
            Trigger();
        }
    }
}
