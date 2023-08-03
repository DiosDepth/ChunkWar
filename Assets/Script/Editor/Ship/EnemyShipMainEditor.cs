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
        var win = GetWindow<EnemyShipMainEditor>("µÐÈË±à¼­Æ÷");
        win.position = GUIHelper.GetEditorWindowRect().AlignCenter(1400, 800);
        win.minSize = new Vector2(1500, 900);
        win.Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(true);
        tree.Config.DrawSearchToolbar = true;

        tree.AddAllAssetsAtPath("µÐÈË", "Assets/Resources/Configs/EnemyShips", typeof(AIShipConfig), true, true);

        return tree;
    }
}
