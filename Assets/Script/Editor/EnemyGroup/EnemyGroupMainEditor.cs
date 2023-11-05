using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroupMainEditor : OdinMenuEditorWindow
{
    public static void ShowWindow()
    {
        var win = GetWindow<EnemyGroupMainEditor>("编队编辑器");
        win.position = GUIHelper.GetEditorWindowRect().AlignCenter(1400, 800);
        win.minSize = new Vector2(1500, 900);
        win.Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        LocalizationManager.Instance.SetLanguage(SystemLanguage.ChineseSimplified);
        var tree = new OdinMenuTree(true);
        tree.Config.DrawSearchToolbar = true;

        var menu = tree.AddAllAssetsAtPath("编队列表", "Assets/EditorRes/EnemyGroup", typeof(EnemyGroupConfigItem), true, true);
        menu.ForEach(x =>
        {
            var childs = x.ChildMenuItems;
            childs.ForEach(child =>
            {
                EnemyGroupConfigItem info = child.Value as EnemyGroupConfigItem;
                child.Name = string.Format("[{0}]_{1}", info.ID, child.Name);
            });
        });
        tree.SortMenuItemsByName();
        return tree;
    }
}
