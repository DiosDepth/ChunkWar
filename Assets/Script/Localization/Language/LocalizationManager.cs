using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Sirenix.Serialization;

public class LocalizationManager : Singleton<LocalizationManager>
{
    private SystemLanguage CurrentLanguage = SystemLanguage.Unknown;

    public static string LanguagePathRoot = "Configs/Localization/";
    private Dictionary<string, string> TextData = new Dictionary<string, string>();

    private static char ReplaceKey = '#';

    public LocalizationManager()
    {

    }

    public void LoadLanguage()
    {
        TextData.Clear();
        var path = GetCurLanguageFilePath();
        var textAssets = ResManager.Instance.Load<TextAsset>(path);
        if(textAssets != null)
        {
            var bytes = textAssets.bytes;
            var dic = SerializationUtility.DeserializeValue<Dictionary<string, string>>(bytes, DataFormat.Binary);
            TextData = dic;
        }
        else
        {
            Debug.LogError("TextData Read Fail, path=" + path);
        }
    }

    private string GetCurLanguageFilePath()
    {
        return string.Format("{0}TextData_{1}", LanguagePathRoot, CurrentLanguage.ToString());
    }

    public string GetTextValue(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }

        var result = string.Empty;
        TextData.TryGetValue(key, out result);
        return result;
    }

    public string ReplaceTextBySpecialValue(string rowText, params string[] replaceStrs)
    {
        if (replaceStrs == null || replaceStrs.Length == 0 || string.IsNullOrEmpty(rowText))
        {
            return rowText;
        }

        string newText = rowText;
        for (int i = 0; i < replaceStrs.Length; i++)
        {
            var replaceKey = ReplaceKey + i.ToString();
            newText = newText.Replace(replaceKey, replaceStrs[i]);
        }

        return newText;
    }


    public void SetLanguage(SystemLanguage language)
    {
        if (CurrentLanguage == language)
            return;

        CurrentLanguage = language;
        LoadLanguage();
    }

}