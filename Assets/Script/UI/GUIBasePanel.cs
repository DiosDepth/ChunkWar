using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GUIBasePanel : MonoBehaviour
{
    private Dictionary<string, List<UIBehaviour>> GUIDic = new Dictionary<string, List<UIBehaviour>>();
    public CanvasGroup uiGroup;
    public RectTransform root;
    public object owner;
    private void Awake()
    {
        //FindGUIComponent<CanvasGroup>();
        FindGUIComponent<Button>();
        FindGUIComponent<TMP_Text>();
        FindGUIComponent<Text>();
        FindGUIComponent<Image>();
        FindGUIComponent<Slider>();
        FindGUIComponent<InputField>();

        /*foreach (KeyValuePair<string, List<UIBehaviour>> pair in GUIDic)
        {
            Debug.Log(pair.Key + " : " + pair.Value.Count);
        }*/

    }

    public virtual void Initialization()
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
