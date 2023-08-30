using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Linq;

public class LevelPresetEditor : OdinMenuEditorWindow
{
    public static void ShowWindow()
    {
        var win = GetWindow<LevelPresetEditor>("�ؿ�Ԥ��༭��");
        win.position = GUIHelper.GetEditorWindowRect().AlignCenter(1400, 800);
        win.minSize = new Vector2(1500, 900);
        win.Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        LocalizationManager.Instance.SetLanguage(SystemLanguage.ChineseSimplified);
        LevelPresetDataOverview.Instance.RefreshAndLoad();
        var tree = new OdinMenuTree(true);
        tree.Config.DrawSearchToolbar = true;

        var menu = tree.AddAllAssetsAtPath("�ؿ��б�", "Assets/Resources/Configs/Levels", typeof(LevelSpawnConfig), true, true);
        menu.ForEach(x =>
        {
            var childs = x.ChildMenuItems;
            childs.ForEach(child =>
            {
                LevelSpawnConfig info = child.Value as LevelSpawnConfig;
                child.Name = string.Format("[{0}]_{1}", info.LevelPresetID, child.Name);
            });
        });
        tree.SortMenuItemsByName();
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

            if (SirenixEditorGUI.ToolbarButton(new GUIContent("�¹ؿ�Ԥ��")))
            {
                ItemCreateEditor.OpenCreateWindow(ItemCreateType.LevelPreset);
            }

        }
        SirenixEditorGUI.EndHorizontalToolbar();
    }

}
