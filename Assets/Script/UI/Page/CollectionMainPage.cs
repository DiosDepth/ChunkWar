using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
        allMenus.OrderBy(x => x.Order);
        var parentTrans = transform.Find("Content/TabContent/Scroll View/Viewport/Content");

        for (int i = 0; i < allMenus.Length; i++)
        {
            GenerateTabs(allMenus[i], parentTrans, null, true);
        }
    }

    private void GenerateTabs(CollectionMenuKey parentMenu, Transform parentRoot, CollectionItemTabCmpt parentTab, bool firstTab)
    {
        CollectionItemTabCmpt parentTransCmpt = null;

        ///ParentMenu
        PoolManager.Instance.GetObjectSync(CollectionTabCmpt_PrefabPath, true, (obj) =>
        {
            var cmpt = obj.transform.SafeGetComponent<CollectionItemTabCmpt>();
            cmpt.SetUp(parentMenu);
            parentTransCmpt = cmpt;
            if (!firstTab)
            {
                cmpt.SetItemParent(parentTab);
            }
            allTabs.Add(cmpt);  
        }, parentRoot);
        

        var childs = parentMenu.SubMenus;
        childs.OrderBy(x => x.Order);
        for(int i = 0; i < childs.Length; i++)
        {
            GenerateTabs(childs[i], parentTransCmpt.transform, parentTransCmpt, false);
        }
    }

    private void ClosePage()
    {
        UIManager.Instance.HiddenUI("CollectionMainPage");
    }

  
}
