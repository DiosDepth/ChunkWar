using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.Linq;
using System.IO;
using System.Text;
using Sirenix.Serialization;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;

public class LocalizationEditor : OdinMenuEditorWindow
{
    [MenuItem("工具/文本编辑器")]
    public static void ShowWindow()
    {
        var win = GetWindow<LocalizationEditor>("文本编辑器");
        win.position = GUIHelper.GetEditorWindowRect().AlignCenter(1400, 800);
        win.minSize = new Vector2(1500, 900);
        win.Show();
    }

    private string _currentCreateTextSetName;

    private const string LanguageOutputPath = "Assets/Resources/Configs/Localization/";

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(true);
        tree.DefaultMenuStyle.IconSize = 28.00f;
        tree.Config.DrawSearchToolbar = true;

        tree.AddAllAssetsAtPath("文本", "Assets/EditorRes/Config/Localization", typeof(LocalizationSerlizationData), true, true);

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

            if (SirenixEditorGUI.ToolbarButton(new GUIContent("检测文本重复")))
            {

            }

            if (SirenixEditorGUI.ToolbarButton(new GUIContent("导出文本")))
            {
                ExportLanguageData();
            }

            _currentCreateTextSetName = GUILayout.TextField(_currentCreateTextSetName);

            if (SirenixEditorGUI.ToolbarButton(new GUIContent("创建文本集")))
            {
                CreateNewTextSet();
                _currentCreateTextSetName = string.Empty;
            }
        }
        SirenixEditorGUI.EndHorizontalToolbar();
    }

    private void CreateNewTextSet()
    {
        string fileName = string.Format("TextData_{0}", _currentCreateTextSetName);
        string fullPath = string.Format("Assets/EditorRes/Config/Localization/{0}.asset", fileName);
        if (File.Exists(fullPath))
        {
            Debug.LogError("文本集已经存在！");
            return;
        }

        SimEditorUtility.CreateAssets<LocalizationSerlizationData>("Assets/EditorRes/Config/Localization/", fileName, obj =>
        {
            obj.TextSetName = obj.name;
            var parentWin = GetWindow<LocalizationEditor>();
            parentWin.TrySelectMenuItemWithObject(obj);
        });
        Debug.Log("创建文本集成功");
    }

    private void ExportLanguageData()
    {
        EditorUtility.DisplayProgressBar("读取文本配置", "读取文本配置中...", 0.1f);
        var allTextAssets = AssetDatabase.FindAssets("t:LocalizationSerlizationData")
           .Select(guid => AssetDatabase.LoadAssetAtPath<LocalizationSerlizationData>(AssetDatabase.GUIDToAssetPath(guid)))
           .ToArray();

        if (allTextAssets == null || allTextAssets.Length <= 0)
            return;

        Dictionary<SystemLanguage, Dictionary<string, string>> languageDic = new Dictionary<SystemLanguage, Dictionary<string, string>>();

        languageDic.Add(SystemLanguage.ChineseSimplified, new Dictionary<string, string>());
        languageDic.Add(SystemLanguage.Japanese, new Dictionary<string, string>());
        languageDic.Add(SystemLanguage.English, new Dictionary<string, string>());

        StringBuilder ErrorSb = new StringBuilder();

        for (int i = 0; i < allTextAssets.Length; i++)
        {
            var info = allTextAssets[i];
            EditorUtility.DisplayProgressBar("读取文本配置", string.Format("生成文本配置中....文本集名 ：{0}", info.TextSetName), i / allTextAssets.Length);
            foreach (var item in info.Texts)
            {
                if (!string.IsNullOrEmpty(item.TextID))
                {
                    if (!languageDic[SystemLanguage.ChineseSimplified].ContainsKey(item.TextID))
                    {
                        ///Replace
                        string replaceLine = item.ChineseSimplified.Replace("\\n", "\n");
                        item.ChineseSimplified = replaceLine;
                        languageDic[SystemLanguage.ChineseSimplified].Add(item.TextID, item.ChineseSimplified);
                    }
                    else
                    {
                        ErrorSb.Append(string.Format("存在相同文本ID, ID = {0} \n", item.TextID));
                        continue;
                    }

                    if (!languageDic[SystemLanguage.English].ContainsKey(item.TextID))
                    {
                        languageDic[SystemLanguage.English].Add(item.TextID, item.English);
                    }

                    if (!languageDic[SystemLanguage.Japanese].ContainsKey(item.TextID))
                    {
                        languageDic[SystemLanguage.Japanese].Add(item.TextID, item.Japanese);
                    }
                }
            }
        }

        foreach (var TextDic in languageDic)
        {
            string outPath = string.Format("{0}TextData_{1}.bytes", LanguageOutputPath, TextDic.Key.ToString());
            var bytes = SerializationUtility.SerializeValue<Dictionary<string, string>>(TextDic.Value, DataFormat.Binary);
            File.WriteAllBytes(outPath, bytes);
        }

        EditorUtility.ClearProgressBar();

        var errorStr = ErrorSb.ToString();
        if (!string.IsNullOrEmpty(errorStr))
        {
            Debug.LogError(errorStr);
        }
        Debug.Log("文本导出完成");
    }
}