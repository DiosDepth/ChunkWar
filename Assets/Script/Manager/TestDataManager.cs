using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if GMDEBUG
public class TestDataManager : Singleton<TestDataManager>
{
    public List<ShipPlugPresetTestData> PlugPresetDatas;
    public List<ShipPresetTestData> ShipPresetDatas;

    private const string PlugPreset_FileRoot = "Assets/EditorRes/Config/PlugPreset";
    private const string ShipPreset_FileRoot = "Assets/EditorRes/Config/ShipPreset";

    public override void Initialization()
    {
        base.Initialization();
        PlugPresetDatas = new List<ShipPlugPresetTestData>();
        ShipPresetDatas = new List<ShipPresetTestData>();

#if UNITY_EDITOR
        var plugDatas = GetTestDataFiles(PlugPreset_FileRoot);
        for (int i = 0; i < plugDatas.Length; i++)
        {
            var data = AssetDatabase.LoadAssetAtPath<ShipPlugPresetTestData>(plugDatas[i]);
            if(data != null)
            {
                PlugPresetDatas.Add(data);
            }
        }
        var shipDatas = GetTestDataFiles(ShipPreset_FileRoot);
        for (int i = 0; i < shipDatas.Length; i++)
        {
            var data = AssetDatabase.LoadAssetAtPath<ShipPresetTestData>(shipDatas[i]);
            if (data != null)
            {
                ShipPresetDatas.Add(data);
            }
        }

#endif
    }

    private string[] GetTestDataFiles(string root)
    {
        List<string> result = new List<string>();
        var dir = Directory.GetFiles(root);
        foreach(var fileName in dir)
        {
            if (fileName.EndsWith(".asset"))
            {
                result.Add(fileName);
            }
        }
        return result.ToArray();
    }
}
#endif