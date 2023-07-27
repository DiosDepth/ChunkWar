using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ShopMainEditor : OdinEditorWindow
{
    [InlineEditor]
    [HideReferenceObjectPicker]
    public ShopMainConfig ShopCfg;

    private const string ShopConfigPath = "Assets/Resources/Configs/Main/ShopMainConfig.asset";

    public static void ShowWindow()
    {
        var win = GetWindow<ShopMainEditor>("ÉÌµê±à¼­Æ÷");
        win.position = GUIHelper.GetEditorWindowRect().AlignCenter(1400, 800);
        win.minSize = new Vector2(1500, 900);
        win.Show();
    }

    protected override void Initialize()
    {
        base.Initialize();
        ShopCfg = AssetDatabase.LoadAssetAtPath<ShopMainConfig>(ShopConfigPath);

        if (ShopCfg == null)
        {
            ShopCfg = SimEditorUtility.CreateAssets<ShopMainConfig>("Assets/Resources/Configs/Main", "ShopMainConfig");
        }
    }

}
