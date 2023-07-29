using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;

public static class LanguageGenerator
{
    private const string TargetRootPath = "Assets/EditorRes/Config/Localization";

    public static void GenerateTextData(Dictionary<string, string> textMap, string targetFileName)
    {
        string fullPath = TargetRootPath + "/" + targetFileName + ".asset";

        if (File.Exists(fullPath))
        {
            ///Read OldData
            LocalizationSerlizationData data = AssetDatabase.LoadAssetAtPath<LocalizationSerlizationData>(fullPath);
            if (data != null)
            {
                data.LoadDataToDic();

                foreach (var text in textMap)
                {
                    if (text.Value == null)
                    {
                        textMap[text.Key] = string.Empty;
                    }

                    string replaceLine = text.Value.Replace("\r\n", "\n");
                    if (data.IsTextIDExists(text.Key))
                    {
                        var item = data.GetTextItem(text.Key);
                        item.ChineseSimplified = replaceLine;
                    }
                    else
                    {
                        ///AddNew

                        data.Texts.Add(new TextEditorItem()
                        {
                            ChineseSimplified = replaceLine,
                            TextID = text.Key
                        });
                    }
                }
                EditorUtility.SetDirty(data);
            }
        }
        else
        {
            ///CreateNewData
            var data = SimEditorUtility.CreateAssets<LocalizationSerlizationData>("Assets/EditorRes/Config/Localization", targetFileName, obj =>
            {
                obj.TextSetName = targetFileName;
            });

            List<TextEditorItem> items = new List<TextEditorItem>();
            foreach (var item in textMap)
            {
                items.Add(new TextEditorItem()
                {
                    TextID = item.Key,
                    ChineseSimplified = item.Value
                });
            }
            data.Texts = items;
            EditorUtility.SetDirty(data);

            Debug.Log("创建新文本 :" + targetFileName);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

}