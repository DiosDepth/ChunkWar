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

    [Button("��������", ButtonSizes.Large), GUIColor(0,1,0)]
    public static void ShowEnemyEditor()
    {
        EnemyShipMainEditor.ShowWindow();
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


    [Button("�ؿ�Ԥ��༭", ButtonSizes.Large), GUIColor(0, 1, 0)]
    public static void ShowLevelEditor()
    {
        LevelPresetEditor.ShowWindow();
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

    [Button("�ɾ�����", ButtonSizes.Large)]
    public static void ShowAchievementEditor()
    {
        AchievementEditor.ShowWindow();
        var win = GetWindow<MainCollectionEditor>();
        win.Close();
    }


    [Button("��Ӫ����", ButtonSizes.Large)]
    public static void ShowCampEditor()
    {
        CampEditor.ShowWindow();
        var win = GetWindow<MainCollectionEditor>();
        win.Close();
    }

    [Button("�ӵ�����", ButtonSizes.Large)]
    public static void ShowBulletEditor()
    {
        BulletEditor.ShowWindow();
        var win = GetWindow<MainCollectionEditor>();
        win.Close();
    }
}