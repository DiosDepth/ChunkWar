using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampEditor : OdinMenuEditorWindow
{
    public static void ShowWindow()
    {
        var win = GetWindow<CampEditor>("阵营编辑器");
        win.position = GUIHelper.GetEditorWindowRect().AlignCenter(1400, 800);
        win.minSize = new Vector2(1500, 900);
        win.Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        LocalizationManager.Instance.SetLanguage(SystemLanguage.ChineseSimplified);
        var tree = new OdinMenuTree(true);
        tree.Config.DrawSearchToolbar = true;

        var menu = tree.AddAllAssetsAtPath("阵营列表", "Assets/Resources/Configs/Camps", typeof(CampConfig), true, true);
        menu.ForEach(x =>
        {
            var childs = x.ChildMenuItems;
            childs.ForEach(child =>
            {
                CampConfig info = child.Value as CampConfig;
                child.Name = string.Format("[{0}]_{1}", info.CampID, child.Name);
            });
        });
        tree.SortMenuItemsByName();
        return tree;
    }

}
