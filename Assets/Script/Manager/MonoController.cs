using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonoController : MonoBehaviour
{
    private event UnityAction updateEvent;
    private event UnityAction fixedUpdateEvent;
    private event UnityAction laterUpdateEvent;
    private event UnityAction awakeEvent;
    private event UnityAction startEvent;



    private void Awake()
    {
        if (awakeEvent != null)
        {
            awakeEvent();
        }
    }

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        if (startEvent != null)
        {
            startEvent();
        }
    }

    void Update()
    {
        AchievementManager.Instance.OnUpdate();
        if (updateEvent != null)
        {
            updateEvent();
        }
    }

    private void LateUpdate()
    {
        if(laterUpdateEvent != null)
        {
            laterUpdateEvent();
        }
    }

    private void FixedUpdate()
    {
        if(fixedUpdateEvent != null)
        {
            fixedUpdateEvent();
        }
    }


    public void AddFixedUpdateListener(UnityAction fun)
    {
        fixedUpdateEvent += fun;
    }
    

    public void RemoveFixedUpdateListener(UnityAction fun)
    {
        fixedUpdateEvent -= fun;
    }

public void AddUpdateListener(UnityAction fun)
    {
        updateEvent += fun;
    }

    public void RemoveUpdateListener(UnityAction fun)
    {
        updateEvent -= fun;
    }

    public void AddAwakeListener(UnityAction fun)
    {
        awakeEvent += fun;
    }
    public void RemoveAwakeListener(UnityAction fun)
    {
        awakeEvent -= fun;
    }

    public void AddStartListener(UnityAction fun)
    {
        startEvent += fun;
    }
    public void RemoveStartListener(UnityAction fun)
    {
        startEvent -= fun;
    }


    public void AddLaterUpdateListener(UnityAction fun)
    {
        laterUpdateEvent += fun;
    }

    public void RemoveLaterUpdateListener(UnityAction fun)
    {
        laterUpdateEvent -= fun;
    }

    public void DontDestroy(Object target)
    {
        DontDestroyOnLoad(target);
    }

    private void OnApplicationQuit()
    {
        SaveLoadManager.Instance.SaveGlobalSaveData();
    }

}
