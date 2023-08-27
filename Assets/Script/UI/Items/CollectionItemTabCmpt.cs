using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum CollectionItemTabState
{
    Default,
    Active,
    Hide,
}

public class CollectionItemTabCmpt : MonoBehaviour, IPoolable
{
    private List<CollectionItemTabCmpt> childCmpts = new List<CollectionItemTabCmpt>();
    public RectTransform DownArrowRect;

    private RectTransform rect;
    private Vector2 startSize;

    private bool isOpen
    {
        get;set;
    }

    public void Awake()
    {
        transform.Find("Content/Btn").SafeGetComponent<Button>().onClick.AddListener(OnButtonClick);
        DownArrowRect = transform.Find("Content/Arrow").SafeGetComponent<RectTransform>();
        rect = GetComponent<RectTransform>();
        startSize = rect.sizeDelta;
        isOpen = false;
    }

    public void SetUp(CollectionMenuKey menu)
    {
        
    }

    public void SetItemParent(CollectionItemTabCmpt parent)
    {
        transform.parent = parent.transform;
        parent.AddChild(this);
        ///Offset
        GetComponent<VerticalLayoutGroup>().padding = new RectOffset((int)parent.DownArrowRect.sizeDelta.x, 0, 0, 0);

        if (parent.isOpen)
        {
            AddParnetSize((int)rect.sizeDelta.y);
        }
        else
        {
            transform.SafeSetActive(false);
        }
    }

    public void UpdateRectTransformSize(int change)
    {
        rect.sizeDelta = new Vector2(startSize.x, rect.sizeDelta.y + change);
    }

    /// <summary>
    /// 增加父节点的高度
    /// </summary>
    /// <param name="change"></param>
    public void AddParnetSize(int change)
    {
        var cmpt = transform.parent.SafeGetComponent<CollectionItemTabCmpt>();
        if(cmpt != null)
        {
            cmpt.UpdateRectTransformSize(change);
            cmpt.AddParnetSize(change);
        }
    }


    /// <summary>
    /// 增加子菜单
    /// </summary>
    private void AddChild(CollectionItemTabCmpt child)
    {
        childCmpts.Add(child);
        if(childCmpts.Count >= 1)
        {

        }
    }

    private void OnButtonClick()
    {
        if (isOpen)
        {
            ///Close
            CloseChilds();
            isOpen = false;
        }
        else
        {
            OpenClilds();
            isOpen = true;
        }
    }

    private void CloseChilds()
    {
        if (childCmpts.Count <= 0)
            return;

        foreach(var child in childCmpts)
        {
            child.gameObject.SetActive(false);
            child.AddParnetSize(-(int)child.rect.sizeDelta.y);
        }
        ///SetArrow
    }

    private void OpenClilds()
    {
        if (childCmpts.Count <= 0)
            return;
        
        foreach(var child in childCmpts)
        {
            child.gameObject.SetActive(true);
            child.AddParnetSize((int)rect.sizeDelta.y);
        }
        ///SetArrow
    }

    public void PoolableReset()
    {

    }

    public void PoolableDestroy()
    {
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableSetActive(bool isactive = true)
    {
        this.gameObject.SetActive(isactive);
    }
}
