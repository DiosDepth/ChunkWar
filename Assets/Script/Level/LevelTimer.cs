using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 * 关卡计时器
 */
public class LevelTimer 
{
    private float _timer;

    /// <summary>
    /// 累计秒数
    /// </summary>
    public int TotalSeconds
    {
        get;
        private set;
    }

    /// <summary>
    /// 倒计时秒数
    /// </summary>
    private int _currentSecond;
    public int CurrentSecond
    {
        get { return _currentSecond; }
    }

    public Action<int> OnTimeSecondUpdate;

    private bool _pause = true;

    private List<LevelTimerTrigger> _triggers = new List<LevelTimerTrigger>();
    private Dictionary<string, LevelTimerTrigger> _triggerDic = new Dictionary<string, LevelTimerTrigger>();

    public void InitTimer(int totalSecond)
    {
        _currentSecond = totalSecond;
        _pause = true;
        TotalSeconds = 0;
    }

    public void OnUpdate()
    {
        if (_pause || _currentSecond <= 0)
            return;

        _timer += Time.deltaTime;
        if(_timer >= 1)
        {
            _currentSecond--;
            TotalSeconds++;
            _timer = 0;
            OnTimeSecondUpdate?.Invoke(_currentSecond);
            UpdateTrigger();
            if (_currentSecond <= 0)
            {
                _pause = true;
                RogueManager.Instance.OnWaveFinish();  
            }
        }

    }

    public void AddTrigger(LevelTimerTrigger trigger)
    {
        _triggers.Add(trigger);
        if (!string.IsNullOrEmpty(trigger.TriggerName) && !_triggerDic.ContainsKey(trigger.TriggerName)) 
        {
            _triggerDic.Add(trigger.TriggerName, trigger);
        }
    }

    public void StartTimer()
    {
        _pause = false;
    }

    public void Pause()
    {
        _pause = true;
    }

    public void RemoveAllTrigger()
    {
        _triggerDic.Clear();
        _triggers.Clear();
    }

    private void UpdateTrigger()
    {
        for (int i = _triggers.Count - 1; i >= 0; i--) 
        {
            var trigger = _triggers[i];
            trigger.OnUpdateSecond(TotalSeconds);
            if (trigger.IsNeedToRemove)
            {
                _triggers.RemoveAt(i);
                if (_triggerDic.ContainsKey(trigger.TriggerName))
                {
                    _triggerDic.Remove(trigger.TriggerName);
                }
            }
        }
    }
}

/*
 * 通用关卡时间触发器
 */
public class LevelTimerTrigger
{
    public string TriggerName;

    /// <summary>
    /// 起始时间
    /// </summary>
    private int _startSecond;

    /// <summary>
    /// 总共间隔
    /// </summary>
    private int _secondDelta;

    /// <summary>
    /// 是否循环
    /// </summary>
    private bool _isLoop = false; 

    /// <summary>
    /// 触发次数
    /// </summary>
    private int _totalloopCount;
    /// <summary>
    /// 当前触发次数
    /// </summary>
    private int _currentLoopCount;

    private int _secondTimer;

    public Action<int> TriggerAction;

    private int paramInt;

    public bool IsNeedToRemove
    {
        get;
        private set;
    }


    public static LevelTimerTrigger CreateTriger(int startSecond, int secondDelta, int loopCount, string TriggerName = null)
    {
        LevelTimerTrigger trigger = new LevelTimerTrigger(startSecond, secondDelta, loopCount);
        trigger.TriggerName = TriggerName;
        trigger.IsNeedToRemove = false;
        return trigger;
    }

    private LevelTimerTrigger(int startSecond, int secondDelta, int loopCount)
    {
        _startSecond = startSecond;
        _secondDelta = secondDelta;
        _isLoop = loopCount <= 0;
        _totalloopCount = loopCount;
        _currentLoopCount = 0;
        _secondTimer = 0;
    }

    public void BindChangeAction(Action<int> action, int param = 0)
    {
        this.paramInt = param;
        TriggerAction = action;
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <returns>是否Remove</returns>
    public void OnUpdateSecond(int currentSecond)
    {
        if (IsNeedToRemove)
            return;

        _secondTimer++;

        if (_startSecond > currentSecond)
            return;

        if(_secondTimer >= _secondDelta)
        {
            TriggerAction?.Invoke(paramInt);
            _currentLoopCount++;
            _secondTimer = 0;
            if (!_isLoop && _currentLoopCount > _totalloopCount)
            {
                IsNeedToRemove = true;
            }
        }
    }
}
