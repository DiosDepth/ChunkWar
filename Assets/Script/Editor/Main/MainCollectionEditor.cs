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
    [MenuItem("π§æﬂ/±‡º≠∆˜∫œºØ %Q")]
    public static void ShowWindow()
    {
        var win = GetWindow<MainCollectionEditor>("±‡º≠∆˜∫œºØ");
        win.position = GUIHelper.GetEditorWindowRect().AlignCenter(300, 600);
        win.minSize = new Vector2(300, 600);
        win.Show();
    }

    [Button("Ω¢¥¨≈‰÷√", ButtonSizes.Large)]
    public static void ShowShipEditor()
    {
        ShipMainEditor.ShowWindow();
        var win = GetWindow<MainCollectionEditor>();
        win.Close();
    }

    [Button("µ–»À≈‰÷√", ButtonSizes.Large), GUIColor(0,1,0)]
    public static void ShowEnemyEditor()
    {
        EnemyShipMainEditor.ShowWindow();
        var win = GetWindow<MainCollectionEditor>();
        win.Close();
    }

    [Button("Ω¢¥¨≤Âº˛≈‰÷√", ButtonSizes.Large)]
    public static void ShowShipPlugEditor()
    {
        ShipPlugMainEditor.ShowWindow();
        var win = GetWindow<MainCollectionEditor>();
        win.Close();
    }

    [Button("…ÃµÍ≈‰÷√", ButtonSizes.Large)]
    public static void ShowShopEditor()
    {
        ShopMainEditor.ShowWindow();
        var win = GetWindow<MainCollectionEditor>();
        win.Close();
    }

    [Button("Œƒ±æ◊¢≤·", ButtonSizes.Medium)]
    public static void ShowLocalizationEditor()
    {
        LocalizationEditor.ShowWindow();
        var win = GetWindow<MainCollectionEditor>();
        win.Close();
    }

    [Button("≥…æÕ≈‰÷√", ButtonSizes.Large)]
    public static void ShowAchievementEditor()
    {
        AchievementEditor.ShowWindow();
        var win = GetWindow<MainCollectionEditor>();
        win.Close();
    }
}