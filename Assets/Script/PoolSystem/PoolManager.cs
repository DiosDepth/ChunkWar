using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PoolData
{ 


    public GameObject fatherObj;
    public List<GameObject> poolList;

    public PoolData(GameObject m_obj, GameObject m_poolobj)
    {
        fatherObj = new GameObject(m_obj.name);
        fatherObj.transform.parent = m_poolobj.transform;
        poolList = new List<GameObject>() { m_obj };
        m_obj.transform.parent = fatherObj.transform;
        m_obj.SetActive(false);
    }

    public GameObject GetObject(bool m_active)
    {
        GameObject temp_obj = null;
        temp_obj = poolList[0];
        poolList.RemoveAt(0);
        temp_obj.SetActive(m_active);
        temp_obj.transform.parent = null;
        return temp_obj;
    }

    public void BackObject(GameObject m_obj)
    {
        m_obj.SetActive(false);
        poolList.Add(m_obj);
        m_obj.transform.SetParent(fatherObj.transform);
    }

    public void DestroyObject(int m_count = 1)
    {
        if (m_count >= poolList.Count)
        {
            Clear();
            return;
        }
        GameObject temp_obj = null;
        for (int i = 0; i < m_count; i++)
        {
            temp_obj = poolList[0];
            poolList.RemoveAt(0);
            GameObject.Destroy(temp_obj);
        }

        if (poolList.Count <= 0)
        {
            GameObject.Destroy(fatherObj);
        }
    }

    public void Clear()
    {
        poolList.Clear();
        GameObject.Destroy(fatherObj);
    }

}

public class PoolManager : Singleton<PoolManager>
{
    public Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();

    public GameObject poolObj;


   
    public PoolManager()
    {
        Initialization();
        poolDic = new Dictionary<string, PoolData>();

    }

    public override void Initialization()
    {
        base.Initialization();
    }
    public void GetObjectAsync(string m_key, bool m_active, Transform trs = null, UnityAction<GameObject,Transform> callback = null,  Transform parentTrans = null)
    {
        //GameObject temp_obj = null;

        if (poolDic.ContainsKey(m_key) && poolDic[m_key].poolList.Count > 0)
        {
            //temp_obj = poolDic[m_key].GetObject();
            GameObject obj = poolDic[m_key].GetObject(m_active);
            callback(obj,trs);
        }
        else
        {
            ResManager.Instance.LoadAsync<GameObject>(m_key, (obj) =>
            {
                obj.name = m_key;
                obj.SetActive(m_active);

                if (parentTrans != null)
                    obj.transform.SetParent(parentTrans, false);

                callback(obj, trs);
            });
            /*temp_obj =  GameObject.Instantiate(Resources.Load<GameObject>(m_key));
             temp_obj.name = m_key;*/
        }
    }

    public void GetObjectAsync(string m_key, bool m_active, UnityAction<GameObject> callback, Transform parentTrans = null)
    {
        //GameObject temp_obj = null;

        if (poolDic.ContainsKey(m_key) && poolDic[m_key].poolList.Count > 0)
        {
            //temp_obj = poolDic[m_key].GetObject();
            callback(poolDic[m_key].GetObject(m_active));
        }
        else
        {
            ResManager.Instance.LoadAsync<GameObject>(m_key, (obj) =>
            {
                obj.name = m_key;
                obj.SetActive(m_active);
                if (parentTrans != null)
                obj.transform.SetParent(parentTrans, false);

                callback(obj);
            });
            /*temp_obj =  GameObject.Instantiate(Resources.Load<GameObject>(m_key));
             temp_obj.name = m_key;*/
        }
    }


    public void GetObjectSync(string m_key, bool m_active, UnityAction<GameObject> callback, Transform parentTrans = null)
    {
        GameObject temp_obj = null;

        if (poolDic.ContainsKey(m_key) && poolDic[m_key].poolList.Count > 0)
        {
            temp_obj = poolDic[m_key].GetObject(m_active);
        }
        else
        {
            temp_obj = ResManager.Instance.Load<GameObject>(m_key);
        }

        if (temp_obj != null)
        {
            temp_obj.name = m_key;
            temp_obj.SetActive(m_active);
            temp_obj.transform.localScale = Vector3.one;
            if (parentTrans != null)
            {
                temp_obj.transform.SetParent(parentTrans, false);
            }

            callback(temp_obj);
        }
    }

    public void BackObject(string m_key, GameObject m_obj)
    {
        if (poolObj == null)
        {
            poolObj = new GameObject("Pool");
            GameObject.DontDestroyOnLoad(poolObj);
        }

        if (poolDic.ContainsKey(m_key))
        {
            poolDic[m_key].BackObject(m_obj);
        }
        else
        {
            poolDic.Add(m_key, new PoolData(m_obj, poolObj));
        }
    }

    public void DestroyObject(string m_key, int m_count = 1)
    {
        if (poolObj == null)
        {
            Debug.Log("No poolObj in the scene");
            return;
        }

        if (poolDic.ContainsKey(m_key))
        {
            poolDic[m_key].DestroyObject(m_count);
        }

        if (poolDic[m_key].poolList.Count <= 0)
        {
            poolDic.Remove(m_key);
        }
    }

    public void Clear(string m_key)
    {
        if (poolObj == null)
        {
            Debug.Log("No poolObj in the scene");
            return;
        }

        if (poolDic.ContainsKey(m_key))
        {
            poolDic[m_key].Clear();
            poolDic.Remove(m_key);
        }
    }
    
    public void ClearAll()
    {
        if (poolObj == null)
        {
            Debug.Log("No poolObj in the scene");
            return;
        }

        foreach(KeyValuePair<string, PoolData> item in poolDic)
        {
            poolDic[item.Key].Clear();
        
        }
        poolDic.Clear();
    }
}
