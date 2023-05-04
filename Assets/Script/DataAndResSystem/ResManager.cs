using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResManager : Singleton<ResManager>
{
    public T Load<T>(string path) where T : Object
    {
        T res = Resources.Load<T>(path);
        if (res is GameObject)
        {
            return GameObject.Instantiate(res);
        }
        else
        {
            return res;
        }
    }

    public void LoadAsync<T>(string path, UnityAction<T> callback) where T : Object
    {
        MonoManager.Instance.StartCoroutine(RealyLoadAsync<T>(path, callback));
    }
    private IEnumerator RealyLoadAsync<T>(string path, UnityAction<T> callback) where T : Object
    {
        ResourceRequest r = Resources.LoadAsync<T>(path);
        yield return r;
        if (r.asset is GameObject)
        {
            callback(GameObject.Instantiate(r.asset) as T);
        }
        else
        {
            callback(r.asset as T);
        }
    }
}
