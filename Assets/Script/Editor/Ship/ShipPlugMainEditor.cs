using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ShipPlugMainEditor : OdinEditorWindow
{
    [InlineEditor]
    [HideReferenceObjectPicker]
    [HorizontalGroup("VVV", Order = 99)]
    public ShipPlugConfig ShipPlugCfg;

    private const string ShipPlugConfigPath = "Assets/Resources/Configs/Main/ShipPlugMainConfig.asset";

    public static void ShowWindow()
    {
        var win = GetWindow<ShipPlugMainEditor>("½¢´¬²å¼þ±à¼­Æ÷");
        win.position = GUIHelper.GetEditorWindowRect().AlignCenter(1400, 800);
        win.minSize = new Vector2(1500, 900);
        win.Show();
    }

    protected override void Initialize()
    {
        base.Initialize();
        ShipPlugCfg = AssetDatabase.LoadAssetAtPath<ShipPlugConfig>(ShipPlugConfigPath);
    }

    [OnInspectorDispose]
    private void OnDispose()
    {
        EditorUtility.SetDirty(ShipPlugCfg);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
