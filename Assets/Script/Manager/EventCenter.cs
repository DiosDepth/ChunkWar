using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EventCenter : Singleton<EventCenter>
{
    private Dictionary<Type, List<BaseListener>> listenerDic;

    public EventCenter()
    {
        Initialization();
        listenerDic = new Dictionary<Type, List<BaseListener>>();
    }

    public override void Initialization()
    {
        base.Initialization();
    }
    public void AddListener<EventType>(EventListener<EventType> listener) where EventType : struct
    {
        Type evttype = typeof(EventType);
        if (!listenerDic.ContainsKey(evttype))
        {
            listenerDic[evttype] = new List<BaseListener>();
        }
        if (!ListenerExists(evttype, listener))
        {
            listenerDic[evttype].Add(listener);
        }
    }

    public void RemoveListener<EventType>(EventListener<EventType> listener) where EventType : struct
    {
        Type evttype = typeof(EventType);
        if (listenerDic.ContainsKey(evttype))
        {
            List<BaseListener> temp_listeners;
            temp_listeners = listenerDic[evttype];
            if (ListenerExists(evttype, listener))
            {
                for (int i = 0; i < temp_listeners.Count; i++)
                {
                    if (temp_listeners[i] == listener)
                    {
                        listenerDic[evttype].Remove(temp_listeners[i]);
                    }
                    if (temp_listeners.Count == 0)
                    {
                        listenerDic.Remove(evttype);
                    }
                }
            }
        }
    }

    public void TriggerEvent<EventType>(EventType newEvent) where EventType : struct
    {
        List<BaseListener> temp_list;
        Type temp_evttype = typeof(EventType);
        if (listenerDic.TryGetValue(temp_evttype, out temp_list))
        {
            for (int i = 0; i < temp_list.Count; i++)
            {
                (temp_list[i] as EventListener<EventType>).OnEvent(newEvent);
            }
        }
    }

    public bool ListenerExists(Type m_type, BaseListener m_listener)
    {
        List<BaseListener> temp_listenerList;
        if (!listenerDic.TryGetValue(m_type, out temp_listenerList))
        {
            return false;
        }
        bool exists = false;
        for (int i = 0; i < temp_listenerList.Count; i++)
        {
            if (temp_listenerList[i] == m_listener)
            {
                exists = true;
            }
        }
        return exists;
    }
}

public static class EventRegister
{
    public delegate void Delegate<T>(T eventType);
    public static void EventStartListening<EventType>(this EventListener<EventType> caller) where EventType : struct
    {
        EventCenter.Instance.AddListener<EventType>(caller);
    }
    public static void EventStopListening<EventType>(this EventListener<EventType> caller) where EventType : struct
    {
        EventCenter.Instance.RemoveListener<EventType>(caller);
    }
}

public interface BaseListener
{

}

public interface EventListener<T> : BaseListener
{
    void OnEvent(T evt);
}
