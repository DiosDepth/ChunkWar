using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionMainPage : GUIBasePanel
{
    private List<CollectionItemTabCmpt> allTabs = new List<CollectionItemTabCmpt>();
    private static string CollectionTabCmpt_PrefabPath = "Prefab/GUIPrefab/PoolUI/CollectionItemTab";


    protected override void Awake()
    {
        base.Awake();
    }

    public override void Initialization()
    {
        base.Initialization();
        GetGUIComponent<Button>("BackBtn").onClick.AddListener(ClosePage);
        InitTab();
    }

    private void InitTab()
    {
        var allMenus = DataManager.Instance.gameMiscCfg.CollectionMenu;
        var parentTrans = transform.Find("Content/TabContent/Scroll View/Viewport/Content");

        for (int i = 0; i < allMenus.Length; i++)
        {
            GenerateTabs(allMenus[i], parentTrans);
        }
    }

    private void GenerateTabs(CollectionMenuKey parentMenu, Transform parentRoot)
    {
        Transform parentTrans = null;

        ///ParentMenu
        PoolManager.Instance.GetObjectSync(CollectionTabCmpt_PrefabPath, true, (obj) =>
        {
            var cmpt = obj.transform.SafeGetComponent<CollectionItemTabCmpt>();
            allTabs.Add(cmpt);

        }, parentTrans);

        var childs = parentMenu.SubMenus;
        for(int i = 0; i < childs.Length; i++)
        {

        }
    }

    private void ClosePage()
    {
        UIManager.Instance.HiddenUI("CollectionMainPage");
    }

  
}
