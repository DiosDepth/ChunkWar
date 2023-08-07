using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 * �ؿ���ʱ��
 */
public class LevelTimer 
{
    private float _timer;

    /// <summary>
    /// �ۼ�����
    /// </summary>
    public int TotalSeconds
    {
        get;
        private set;
    }

    /// <summary>
    /// ����ʱ����
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
            trigger.OnUpdateSecond();
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
 * ͨ�ùؿ�ʱ�䴥����
 */
public class LevelTimerTrigger
{
    public string TriggerName;

    /// <summary>
    /// �ܹ����
    /// </summary>
    private int _secondDelta;

    /// <summary>
    /// �Ƿ�ѭ��
    /// </summary>
    private bool _isLoop = false; 

    /// <summary>
    /// ��������
    /// </summary>
    private int _totalloopCount;
    /// <summary>
    /// ��ǰ��������
    /// </summary>
    private int _currentLoopCount;

    private int _secondTimer;

    public Action TriggerAction;

    public bool IsNeedToRemove
    {
        get;
        private set;
    }


    public static LevelTimerTrigger CreateTriger(int secondDelta, int loopCount, Action action, string TriggerName = null)
    {
        LevelTimerTrigger trigger = new LevelTimerTrigger(secondDelta, loopCount);
        trigger.TriggerAction = action;
        trigger.TriggerName = TriggerName;
        trigger.IsNeedToRemove = false;
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
    /// ����
    /// </summary>
    /// <returns>�Ƿ�Remove</returns>
    public void OnUpdateSecond()
    {
        if (IsNeedToRemove)
            return;

        _secondTimer++;
        if(_secondTimer >= _secondDelta)
        {
            TriggerAction?.Invoke();
            _currentLoopCount++;
            _secondTimer = 0;
            if (!_isLoop && _currentLoopCount > _totalloopCount)
            {
                IsNeedToRemove = true;
            }
        }
    }
}
