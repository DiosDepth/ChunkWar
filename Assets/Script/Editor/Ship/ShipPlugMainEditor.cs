using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ShipPlugMainEditor : OdinMenuEditorWindow
{

    public static void ShowWindow()
    {
        var win = GetWindow<ShipPlugMainEditor>("½¢´¬²å¼þ±à¼­Æ÷");
        win.position = GUIHelper.GetEditorWindowRect().AlignCenter(1800, 950);
        win.minSize = new Vector2(1800, 950);
        win.Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        LocalizationManager.Instance.SetLanguage(SystemLanguage.ChineseSimplified);
        var tree = new OdinMenuTree(true);
        tree.Config.DrawSearchToolbar = true;

        var menu = tree.AddAllAssetsAtPath("²å¼þÁÐ±í", "Assets/Resources/Configs/ShipPlug", typeof(ShipPlugDataItemConfig), true, true);
        menu.ForEach(x =>
        {
            var childs = x.ChildMenuItems;
            childs.ForEach(child =>
            {
                ShipPlugDataItemConfig info = child.Value as ShipPlugDataItemConfig;
                var name = LocalizationManager.Instance.GetTextValue(info.GeneralConfig.Name);
                child.Name = string.Format("[{0}]_{1}", info.ID, name);
            });
        });
        tree.SortMenuItemsByName();
        return tree;
    }
}
