using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShipMainEditor : OdinMenuEditorWindow
{
    public static void ShowWindow()
    {
        var win = GetWindow<EnemyShipMainEditor>("���˱༭��");
        win.position = GUIHelper.GetEditorWindowRect().AlignCenter(1400, 800);
        win.minSize = new Vector2(1500, 900);
        win.Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(true);
        LocalizationManager.Instance.SetLanguage(SystemLanguage.ChineseSimplified);
        tree.Config.DrawSearchToolbar = true;

        var menu = tree.AddAllAssetsAtPath("����", "Assets/Resources/Configs/EnemyShips", typeof(AIShipConfig), true, true);
        menu.ForEach(x =>
        {
            var childs = x.ChildMenuItems;
            childs.ForEach(child =>
            {
                AIShipConfig info = child.Value as AIShipConfig;
                child.Name = string.Format("[{0}]_{1}", info.ID, LocalizationManager.Instance.GetTextValue(info.GeneralConfig.Name));
            });
        });

        var menu2 = tree.AddAllAssetsAtPath("���˻�", "Assets/Resources/Configs/Drones", typeof(DroneConfig), true, true);
        menu2.ForEach(x =>
        {
            var childs = x.ChildMenuItems;
            childs.ForEach(child =>
            {
                DroneConfig info = child.Value as DroneConfig;
                child.Name = string.Format("[{0}]_{1}", info.ID, LocalizationManager.Instance.GetTextValue(info.GeneralConfig.Name));
            });
        });

        tree.SortMenuItemsByName();
        return tree;
    }
}
