using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum AchievementWatcherType
{
    EnemyKill,
}

public interface IAchievementWatcher
{

}

public class AchievementWatcher<T> : IAchievementWatcher 
{
    public AchievementWatcherType WatcherType;

    public Action<T> onValueChange;

    public int ValueInt;


    public AchievementWatcher(Action<T> action, AchievementWatcherType type)
    {
        this.WatcherType = type;
        onValueChange += action;
    }

}