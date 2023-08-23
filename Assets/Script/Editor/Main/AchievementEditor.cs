using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementEditor : OdinMenuEditorWindow
{
    public static void ShowWindow()
    {
        var win = GetWindow<AchievementEditor>("�ɾͱ༭��");
        win.position = GUIHelper.GetEditorWindowRect().AlignCenter(1400, 800);
        win.minSize = new Vector2(1500, 900);
        win.Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        LocalizationManager.Instance.SetLanguage(SystemLanguage.ChineseSimplified);
        AchievementDataOverview.Instance.RefreshAndLoad();
        var tree = new OdinMenuTree(true);
        tree.Config.DrawSearchToolbar = true;

        tree.AddAllAssetsAtPath("�ɾ��б�", "Assets/Resources/Configs/Achievement", typeof(AchievementItemConfig), true, true);

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

            if (SirenixEditorGUI.ToolbarButton(new GUIContent("�³ɾ�")))
            {
                ItemCreateEditor.OpenCreateWindow(ItemCreateType.Achievement);
            }

        }
        SirenixEditorGUI.EndHorizontalToolbar();
    }

}
