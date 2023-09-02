using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

public class LocalizationSerlizationData : SerializedScriptableObject
{
    [HideInInspector]
    public string TextSetName;

    [HideReferenceObjectPicker]
    [TableList(DrawScrollView = true, ShowIndexLabels = true, NumberOfItemsPerPage = 40, ShowPaging = true)]
    [LabelText("文本")]
    [Searchable]
    [InfoBox("后缀带有（Auto）的文件为自动生成，不要手动更改！！！！ \n如果需要搜索，点击右侧+号左边的按钮切换显示模式", InfoMessageType = InfoMessageType.Info)]
    public List<TextEditorItem> Texts = new List<TextEditorItem>();

    private Dictionary<string, TextEditorItem> _textDatas;

    public void LoadDataToDic()
    {
        _textDatas = new Dictionary<string, TextEditorItem>();
        for (int i = 0; i < Texts.Count; i++)
        {
            if (!_textDatas.ContainsKey(Texts[i].TextID))
            {
                _textDatas.Add(Texts[i].TextID, Texts[i]);
            }
        }
    }

    public string GetText(string textID)
    {
        if (_textDatas.ContainsKey(textID))
            return _textDatas[textID].ChineseSimplified;
        return string.Empty;
    }

    /// <summary>
    /// ID是否存在
    /// </summary>
    /// <param name="textID"></param>
    /// <returns></returns>
    public bool IsTextIDExists(string textID)
    {
        return _textDatas.ContainsKey(textID);
    }

    public TextEditorItem GetTextItem(string textID)
    {
        if (_textDatas.ContainsKey(textID))
        {
            return _textDatas[textID];
        }
        return null;
    }

}

[System.Serializable]
public class TextEditorItem
{
    [TableColumnWidth(250, false)]
    public string TextID;

    public string ChineseSimplified;

    public string English;

    public string Japanese;

}