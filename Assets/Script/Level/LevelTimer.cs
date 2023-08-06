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
    /// 倒计时秒数
    /// </summary>
    private int _totalSecond;
    public int TotalSecond
    {
        get { return _totalSecond; }
    }

    public Action<int> OnTimeSecondUpdate;

    private bool _pause = true;

    private List<LevelTimerTrigger> _triggers = new List<LevelTimerTrigger>();

    public void InitTimer(int totalSecond)
    {
        _totalSecond = totalSecond;
        _pause = true;
    }

    public void OnUpdate()
    {
        if (_pause || _totalSecond <= 0)
            return;

        _timer += Time.deltaTime;
        if(_timer >= 1)
        {
            _totalSecond--;
            _timer = 0;
            OnTimeSecondUpdate?.Invoke(_totalSecond);
            UpdateTrigger();
            if (_totalSecond <= 0)
            {
                _pause = true;
                RogueManager.Instance.OnWaveFinish();  
            }
        }

    }

    public void AddTrigger()
    {

    }

    public void StartTimer()
    {
        _pause = false;
    }

    public void Pause()
    {
        _pause = true;
    }

    private void UpdateTrigger()
    {
        for (int i = 0; i < _triggers.Count; i++) 
        {
            var trigger = _triggers[i];
            if (trigger.OnUpdateSecond())
            {

            }
        }
    }
}

/*
 * 通用关卡时间触发器
 */
public class LevelTimerTrigger
{
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

    public Action TriggerAction;


    public static LevelTimerTrigger CreateTriger(int secondDelta, int loopCount, Action action)
    {
        LevelTimerTrigger trigger = new LevelTimerTrigger(secondDelta, loopCount);
        trigger.TriggerAction = action;
        return trigger;
    }

    private LevelTimerTrigger(int secondDelta, int loopCount)
    {
        _secondDelta = secondDelta;
        _isLoop = loopCount <= 0;
        _totalloopCount = loopCount;
        _currentLoopCount = 0;
        _secondTimer = 0;
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <returns>是否Remove</returns>
    public bool OnUpdateSecond()
    {
        _secondTimer++;
        if(_secondTimer >= _secondDelta)
        {
            TriggerAction?.Invoke();
            _currentLoopCount++;
            _secondTimer = 0;
            if (!_isLoop && _currentLoopCount > _totalloopCount)
            {
                return true;
            }
        }
        return false;
    }
}
