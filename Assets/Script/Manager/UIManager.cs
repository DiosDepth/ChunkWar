using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum E_UI_Layer
{
    Bot,
    Mid,
    Top,
    System,
}

public class UIManager : Singleton<UIManager>
{
    public string resPath = "Prefab/GUIPrefab/";
    public Dictionary<string, GUIBasePanel> panelDic = new Dictionary<string, GUIBasePanel>();
    private Transform canvas;
    public Transform Canvas { get { return canvas; } }

    private RectTransform bot;
    private RectTransform mid;
    private RectTransform top;
    private RectTransform system;

    //public PlayerHUD playerHUD;
    // Start is called before the first frame update
    public UIManager()
    {
        Initialization();
        GameObject obj = ResManager.Instance.Load<GameObject>(resPath + "Canvas");
        canvas = obj.transform;
        GameObject.DontDestroyOnLoad(obj);
        obj = ResManager.Instance.Load<GameObject>(resPath + "EventSystem");
        GameObject.DontDestroyOnLoad(obj);

    


        GetUILayer(ref system, "System", canvas);
        GetUILayer(ref bot, "Bot", canvas);
        GetUILayer(ref mid, "Mid", canvas);
        GetUILayer(ref top, "Top", canvas);





    }

    public GUIBasePanel GetGUIFromDic(string name)
    {
        if(panelDic.ContainsKey(name))
        {
            return panelDic[name];
        }

        return null;
    }

    public IEnumerator FadeUI(CanvasGroup uigroup, float delaytime, float from, float to, float time, UnityAction callback)
    {
        float timestamp = Time.time;
        float t = 0;
        float a;
        float delaystamp = Time.time + delaytime;

        while (Time.time <= delaystamp)
        {
            yield return null;
        }

        timestamp = Time.time;


        while (t <= 1)
        {
            t = (Time.time - timestamp) / time;
            a = Mathf.Lerp(from, to, t);
            uigroup.alpha = a;
            yield return null;
        }

        if (callback != null)
        {
            callback();
        }
        yield return null;
    }
    public void CreatePoolerUI<T>(string m_ui_res_path, bool m_isactive, UnityAction<T> callback) where T : GUIBasePanel
    {
        PoolManager.Instance.GetObject(m_ui_res_path, m_isactive, (obj) =>
        {
            if (callback != null)
            {
                callback(obj.GetComponent<T>());
            }
        });

    }

    public void BackPoolerUI(string m_ui_res_path, GameObject m_backobj)
    {
        PoolManager.Instance.BackObject(m_ui_res_path, m_backobj);
    }

    public void ShowUI<T>(string m_uiname, E_UI_Layer m_uilayer, UnityAction<T> callback) where T : GUIBasePanel
    {
        if (panelDic.ContainsKey(m_uiname))
        {
            if (callback != null)
            {
                panelDic[m_uiname].Show();
                callback(panelDic[m_uiname] as T);
                return;
            }
        }
        ResManager.Instance.LoadAsync<GameObject>(resPath + m_uiname, (obj) =>
        {
            Transform t_father = bot;
            switch (m_uilayer)
            {
                case E_UI_Layer.Top:
                    t_father = top;
                    break;
                case E_UI_Layer.Mid:
                    t_father = mid;
                    break;
                case E_UI_Layer.Bot:
                    t_father = bot;
                    break;
                case E_UI_Layer.System:
                    t_father = system;
                    break;
            }

            obj.transform.SetParent(t_father);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            (obj.transform as RectTransform).offsetMax = Vector2.zero;
            (obj.transform as RectTransform).offsetMin = Vector2.zero;

            T panel = obj.GetComponent<T>();
            if (callback != null)
            {
                callback(panel);
            }
            panelDic.Add(m_uiname, panel);
            panel.Show();

        });
    }

    public void SetUITRSParent(GameObject obj, E_UI_Layer layer = E_UI_Layer.Mid)
    {
        switch (layer)
        {
            case E_UI_Layer.Top:
                obj.transform.SetParent(mid);
                break;
            case E_UI_Layer.Mid:
                obj.transform.SetParent(mid);
                break;
            case E_UI_Layer.Bot:
                obj.transform.SetParent(bot);
                break;
            case E_UI_Layer.System:
                obj.transform.SetParent(system);
                break;
        }
    }
    //TODO 设置UIParent的方法，设置UI位置的方法

    public void HiddenUI(string m_uiname)
    {
        if (panelDic.ContainsKey(m_uiname))
        {
            panelDic[m_uiname].Hidden(m_uiname);
            GameObject.Destroy(panelDic[m_uiname].gameObject);
            panelDic.Remove(m_uiname);
        }
    }

    private void GetUILayer(ref RectTransform trs, string name, Transform parent)
    {
        trs = canvas.Find(name) as RectTransform;
        if (trs == null)
        {

            trs = new GameObject(name).AddComponent< RectTransform>();
            trs.transform.parent = parent;
            trs.anchorMin = Vector2.zero;
            trs.anchorMax = Vector2.one;
            trs.localScale = Vector3.one;
            trs.localPosition = Vector2.zero;
            trs.offsetMax = Vector2.zero;
            trs.offsetMin = Vector2.zero;

        }
    }
}
