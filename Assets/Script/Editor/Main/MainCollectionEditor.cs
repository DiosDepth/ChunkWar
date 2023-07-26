using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Sirenix.OdinInspector;

public class MainCollectionEditor : OdinEditorWindow
{
    [MenuItem("工具/编辑器合集 %Q")]
    public static void ShowWindow()
    {
        var win = GetWindow<MainCollectionEditor>("编辑器合集");
        win.position = GUIHelper.GetEditorWindowRect().AlignCenter(300, 600);
        win.minSize = new Vector2(300, 600);
        win.Show();
    }

    [Button("舰船配置", ButtonSizes.Large)]
    public static void ShowShipEditor()
    {
        ShipMainEditor.ShowWindow();
        var win = GetWindow<MainCollectionEditor>();
        win.Close();
    }

    [Button("文本注册", ButtonSizes.Medium)]
    public static void ShowLocalizationEditor()
    {
        LocalizationEditor.ShowWindow();
        var win = GetWindow<MainCollectionEditor>();
        win.Close();
    }
}