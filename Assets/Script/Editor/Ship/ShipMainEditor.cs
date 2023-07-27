using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShipMainEditor : OdinMenuEditorWindow
{

    public static void ShowWindow()
    {
        var win = GetWindow<ShipMainEditor>("½¢´¬±à¼­Æ÷");
        win.position = GUIHelper.GetEditorWindowRect().AlignCenter(1400, 800);
        win.minSize = new Vector2(1500, 900);
        win.Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(true);
        tree.Config.DrawSearchToolbar = true;

        tree.AddAllAssetsAtPath("½¢´¬", "Assets/Resources/Configs/Ships", typeof(ShipConfig), true, true);
        tree.AddAllAssetsAtPath("½¨Öþ", "Assets/Resources/Configs/Buildings", typeof(BuildingConfig), true, true);
        tree.AddAllAssetsAtPath("ÎäÆ÷", "Assets/Resources/Configs/Weapons", typeof(WeaponConfig), true, true);

        return tree;
    }

    protected override void OnBeginDrawEditors()
    {
        var selected = this.MenuTree.Selection.FirstOrDefault();
        var toolbarHeight = this.MenuTree.Config.SearchToolbarHeight;

        // Draws a toolbar with the name of the currently selected menu item.
        SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
        {
            if (selected != null)
            {
                GUILayout.Label(selected.Name);
            }

            if (SirenixEditorGUI.ToolbarButton(new GUIContent("¼ì²âÎÄ±¾ÖØ¸´")))
            {

            }

            if (SirenixEditorGUI.ToolbarButton(new GUIContent("´´½¨½¢´¬")))
            {
            }
        }
        SirenixEditorGUI.EndHorizontalToolbar();
    }
}
