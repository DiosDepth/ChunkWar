using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;



public enum E_UI_Layer
{
    Bot,
    Mid,
    Top,
    System,
}

public struct GeneralUIEvent
{
    public UIEventType type;
    public object[] param;

    public GeneralUIEvent(UIEventType m_type, params object[] param)
    {
        type = m_type;
        this.param = param;
    }

    public static void Trigger(UIEventType m_type, params object[] param)
    {
        GeneralUIEvent evt = new GeneralUIEvent(m_type, param);
        EventCenter.Instance.TriggerEvent<GeneralUIEvent>(evt);
    }
}

public enum UIEventType
{
    ShipSelectionChange,
    ShipSelectionConfirm,
    ShipSelection_CampSelect,
}

public class UIManager : Singleton<UIManager>
{
    public string resGUIPath = "Prefab/GUIPrefab/";
    public string resPoolUIPath = "Prefab/GUIPrefab/PoolUI/";
    public Dictionary<string, GUIBasePanel> panelDic = new Dictionary<string, GUIBasePanel>();
    private Transform canvas;
    public Transform Canvas { get { return canvas; } }

    private RectTransform bot;
    private RectTransform mid;
    private RectTransform top;
    private RectTransform system;

    private object tempowner;

    private Vector2 _mousePosition;
    public Vector2 CurrentMousePosition
    {
        get { return _mousePosition; }
    }

    private PointerEventData pointerEventData;
    private Camera m_uiCamera;

    private List<RaycastResult> raycastResultsList = new List<RaycastResult>();
    //public PlayerHUD playerHUD;
    // Start is called before the first frame update
    public UIManager()
    {
        Initialization();
        GameObject obj = ResManager.Instance.Load<GameObject>(resGUIPath + "Canvas");
        canvas = obj.transform;
        m_uiCamera = obj.transform.Find("UICamera").SafeGetComponent<Camera>();
        GameObject.DontDestroyOnLoad(obj);
        obj = ResManager.Instance.Load<GameObject>(resGUIPath + "EventSystem");
        GameObject.DontDestroyOnLoad(obj);
        GetUILayer(ref system, "System", canvas);
        GetUILayer(ref bot, "Bot", canvas);
        GetUILayer(ref mid, "Mid", canvas);
        GetUILayer(ref top, "Top", canvas);

        InputDispatcher.Instance.Action_GamePlay_Point += HandleBuildMouseMove;
        InputDispatcher.Instance.Action_UI_Point += HandleBuildMouseMove;
    }

    ~UIManager()
    {
        InputDispatcher.Instance.Action_GamePlay_Point -= HandleBuildMouseMove;
        InputDispatcher.Instance.Action_UI_Point -= HandleBuildMouseMove;
    }

    public GUIBasePanel GetGUIFromDic(string name)
    {
        if(panelDic.ContainsKey(name))
        {
            return panelDic[name];
        }

        return null;
    }

    public T GetGUIFromDic<T>(string name) where T : GUIBasePanel
    {
        if (panelDic.ContainsKey(name))
        {
            return panelDic[name] as T;
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


    public void CreatePoolerUI<T>(string m_uiname, bool m_isactive, E_UI_Layer m_uilayer, object m_owner = null, UnityAction<T> callback = null ) where T : GUIBasePanel
    {
        PoolManager.Instance.GetObjectAsync(resPoolUIPath+m_uiname, m_isactive, (obj) =>
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

            obj.transform.SetParent(t_father, false);
            obj.transform.localScale = Vector3.one;

            T panel = obj.GetComponent<T>();
            panel.owner = m_owner;
           
            if (callback != null)
            {
                callback(panel);
            }
        });

    }

    public void CreatePoolerUI<T>(string m_uiname, Transform parent, UnityAction<T> callback = null) where T : GUIBasePanel
    {
        PoolManager.Instance.GetObjectAsync(resPoolUIPath + m_uiname, true, (obj) =>
        {
            obj.transform.SetParent(parent, false);
            obj.transform.localScale = Vector3.one;

            T panel = obj.GetComponent<T>();
            callback?.Invoke(panel);
        });
    }

    public void BackPoolerUI(string m_uiname, GameObject m_backobj)
    {
        PoolManager.Instance.BackObject(resPoolUIPath + m_uiname, m_backobj);
    }

    public void ShowUI<T>(string m_uiname, E_UI_Layer m_uilayer, object m_owner = null, UnityAction<T> callback = null) where T : GUIBasePanel
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
        tempowner = m_owner;
        ResManager.Instance.LoadAsync<GameObject>(resGUIPath + m_uiname, (obj) =>
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
            panel.owner = tempowner;
            if (callback != null)
            {
                callback(panel);
            }

            if (!panelDic.ContainsKey(m_uiname))
            {
                panelDic.Add(m_uiname, panel);
            }
           
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
            panelDic[m_uiname].Hidden();
            if(panelDic[m_uiname].gameObject != null)
            {
                GameObject.Destroy(panelDic[m_uiname].gameObject);
            }
            panelDic.Remove(m_uiname);
        }
    }


    public void HiddenAllUI()
    {
        List<string> removelist = new List<string>();
        foreach (KeyValuePair<string, GUIBasePanel> kv in panelDic)
        {
            removelist.Add(kv.Key);
        }
        for (int i = 0; i < removelist.Count; i++)
        {
            HiddenUI(removelist[i]);
        }
    }

    public void HiddenUIALLBut(List<string> exceptionlist = null, bool iskeepBG = true )
    {
        
        List<string> removelist = new List<string>();

        foreach(KeyValuePair<string ,GUIBasePanel> kv in panelDic)
        {
            if (iskeepBG)
            {
                if(kv.Key == "BackGround")
                {
                    continue;
                }
            }
            if(exceptionlist == null || !exceptionlist.Contains(kv.Key))
            {
                removelist.Add(kv.Key);
            }
        }

        for (int i = 0; i < removelist.Count; i++)
        {
            HiddenUI(removelist[i]);
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

    public Vector2 GetUIposBWorldPosition(Vector3 pos)
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(pos);
        Vector2 outPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(top, screenPos, m_uiCamera, out outPos);
        return outPos;
    }

    public Vector2 GetUIPosByMousePos()
    {
        float RowX = _mousePosition.x - Screen.width / 2f;
        float RowY = _mousePosition.y - Screen.height / 2f;

        var canvasSize = canvas.transform.SafeGetComponent<RectTransform>().sizeDelta;
        var targetPos = new Vector2(RowX * (canvasSize.x / Screen.width), RowY * (canvasSize.y / Screen.height));
        return targetPos;
    }

    public bool IsMouseOverUI()
    {
        bool mouseOverUI = false;
        pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Mouse.current.position.ReadValue();
        EventSystem.current.RaycastAll(pointerEventData, raycastResultsList);
        for (int i = 0; i < raycastResultsList.Count; i++)
        {
            if (raycastResultsList[i].gameObject.GetType() == typeof(GameObject))
            {
                mouseOverUI = true;
                break;
            }
        }
        return mouseOverUI;
    }

    private void HandleBuildMouseMove(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                _mousePosition = context.ReadValue<Vector2>();
                break;
        }
    }
}
