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
    [LabelText("�ı�")]
    [Searchable]
    [InfoBox("��׺���У�Auto�����ļ�Ϊ�Զ����ɣ���Ҫ�ֶ����ģ������� \n�����Ҫ����������Ҳ�+����ߵİ�ť�л���ʾģʽ", InfoMessageType = InfoMessageType.Info)]
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
    /// ID�Ƿ����
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