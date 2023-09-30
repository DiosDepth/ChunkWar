﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GUIBasePanel : MonoBehaviour
{
    private Dictionary<string, List<UIBehaviour>> GUIDic = new Dictionary<string, List<UIBehaviour>>();
    public object owner;
    protected virtual void Awake()
    {
        FindGUIComponent<Button>();
    }

    public virtual void Initialization()
    {

    }

    public virtual void Initialization(params object[] param)
    {

    }

    protected T GetGUIComponent<T>(string groupname) where T : UIBehaviour
    {
        if (GUIDic.ContainsKey(groupname))
        {
            for (int i = 0; i < GUIDic[groupname].Count; i++)
            {
                if (GUIDic[groupname][i] is T)
                {
                    return GUIDic[groupname][i] as T;
                }
            }
        }
        Debug.Log("There is no component named : " + groupname);
        return null;

    }

    public virtual void Show()
    {

    }

    public virtual void Hidden( )
    {
        
    }

    protected void FindGUIComponent<T>() where T : UIBehaviour
    {
        T[] t_comp = GetComponentsInChildren<T>();
        string t_objname;
        for (int i = 0; i < t_comp.Length; i++)
        {
            t_objname = t_comp[i].gameObject.name;
            if (GUIDic.ContainsKey(t_objname))
            {
                GUIDic[t_objname].Add(t_comp[i]);
            }
            else
            {
                GUIDic.Add(t_comp[i].gameObject.name, new List<UIBehaviour>() { t_comp[i] });
            }
        }
    }
}
