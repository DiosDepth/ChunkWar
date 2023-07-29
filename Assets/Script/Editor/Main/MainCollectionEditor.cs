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
    [MenuItem("����/�༭���ϼ� %Q")]
    public static void ShowWindow()
    {
        var win = GetWindow<MainCollectionEditor>("�༭���ϼ�");
        win.position = GUIHelper.GetEditorWindowRect().AlignCenter(300, 600);
        win.minSize = new Vector2(300, 600);
        win.Show();
    }

    [Button("��������", ButtonSizes.Large)]
    public static void ShowShipEditor()
    {
        ShipMainEditor.ShowWindow();
        var win = GetWindow<MainCollectionEditor>();
        win.Close();
    }

    [Button("�����������", ButtonSizes.Large)]
    public static void ShowShipPlugEditor()
    {
        ShipPlugMainEditor.ShowWindow();
        var win = GetWindow<MainCollectionEditor>();
        win.Close();
    }

    [Button("�̵�����", ButtonSizes.Large)]
    public static void ShowShopEditor()
    {
        ShopMainEditor.ShowWindow();
        var win = GetWindow<MainCollectionEditor>();
        win.Close();
    }

    [Button("�ı�ע��", ButtonSizes.Medium)]
    public static void ShowLocalizationEditor()
    {
        LocalizationEditor.ShowWindow();
        var win = GetWindow<MainCollectionEditor>();
        win.Close();
    }
}