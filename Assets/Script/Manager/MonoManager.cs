using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Internal;
using UnityEngine.Scripting;
using UnityEngine.Bindings;
using System;

public class MonoManager : Singleton<MonoManager>
{
    public MonoController controller;

    public MonoManager()
    {
        Initialization();

    }

    public override void Initialization()
    {
        base.Initialization();
        GameObject obj = new GameObject("MonoController");
        controller = obj.AddComponent<MonoController>();
    }

    public void AddStartListener(UnityAction fun)
    {
        controller.AddStartListener(fun);
    }
    public void RemoveStartListener(UnityAction fun)
    {
        controller.RemoveStartListener(fun);
    }

    public void AddUpdateListener(UnityAction fun)
    {
        controller.AddUpdateListener(fun);
    }

    public void RemoveUpdateListener(UnityAction fun)
    {
        controller.RemoveUpdateListener(fun);
    }

    public void AddFixedUpdateListener(UnityAction fun)
    {
        controller.AddFixedUpdateListener(fun);
    }

    public void RemoveFixedUpdateListener(UnityAction fun)
    {
        controller.RemoveFixedUpdateListener(fun);
    }

    public void AddLaterUpdateListener(UnityAction fun)
    {
        controller.AddLaterUpdateListener(fun);
    }

    public void RemoveLaterUpdateListener(UnityAction fun)
    {
        controller.RemoveLaterUpdateListener(fun);
    }
    //
    // 摘要:
    //     Starts a coroutine named methodName.
    //
    // 参数:
    //   methodName:
    //
    //   value:
    [ExcludeFromDocs]
    public Coroutine StartCoroutine(string methodName)
    {
        return controller.StartCoroutine(methodName);
    }
    //
    // 摘要:
    //     Starts a coroutine.
    //
    // 参数:
    //   routine:
    public Coroutine StartCoroutine(IEnumerator routine)
    {
        return controller.StartCoroutine(routine);
    }
    //
    // 摘要:
    //     Starts a coroutine named methodName.
    //
    // 参数:
    //   methodName:
    //
    //   value:
    public Coroutine StartCoroutine(string methodName, [DefaultValue("null")] object value)
    {
        return controller.StartCoroutine(methodName, value);
    }
    [Obsolete("StartCoroutine_Auto has been deprecated. Use StartCoroutine instead (UnityUpgradable) -> StartCoroutine([mscorlib] System.Collections.IEnumerator)", false)]
    public Coroutine StartCoroutine_Auto(IEnumerator routine)
    {
        return controller.StartCoroutine(routine);
    }
    //
    // 摘要:
    //     Stops all coroutines running on this behaviour.
    public void StopAllCoroutines()
    {
        controller.StopAllCoroutines();
    }
    //
    // 摘要:
    //     Stops the first coroutine named methodName, or the coroutine stored in routine
    //     running on this behaviour.
    //
    // 参数:
    //   methodName:
    //     Name of coroutine.
    //
    //   routine:
    //     Name of the function in code, including coroutines.
    public void StopCoroutine(IEnumerator routine)
    {
        controller.StopCoroutine(routine);
    }
    //
    // 摘要:
    //     Stops the first coroutine named methodName, or the coroutine stored in routine
    //     running on this behaviour.
    //
    // 参数:
    //   methodName:
    //     Name of coroutine.
    //
    //   routine:
    //     Name of the function in code, including coroutines.
    public void StopCoroutine(Coroutine routine)
    {
        controller.StopCoroutine(routine);
    }
    //
    // 摘要:
    //     Stops the first coroutine named methodName, or the coroutine stored in routine
    //     running on this behaviour.
    //
    // 参数:
    //   methodName:
    //     Name of coroutine.
    //
    //   routine:
    //     Name of the function in code, including coroutines.
    public void StopCoroutine(string methodName)
    {
        controller.StopCoroutine(methodName);
    }

    public void DontDestroyOnLoad(UnityEngine.Object target)
    {
        controller.DontDestroy(target);
    }

    public Coroutine StartDelay(float delaytime, UnityAction callback)
    {
        return StartCoroutine(Delay(delaytime, callback));
    }

    public IEnumerator Delay(float delaytime, UnityAction callback)
    {
        float timestamp = Time.time + delaytime;
        while(Time.time < timestamp)
        {
            yield return null;
        } 

        if (callback != null)
        {
            callback.Invoke();
        }
    }
}
