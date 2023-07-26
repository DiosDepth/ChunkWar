using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.Utilities;
using Sirenix.OdinInspector.Editor;
using System.Linq;
using System.IO;
using UnityEditor;
using Sirenix.OdinInspector;
using UnityEditor.SceneManagement;

public static class SimEditorUtility
{

    /// <summary>
    /// 按高度切割图片
    /// </summary>
    /// <param name="row"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static Texture2D[] SplitTextureFromHeight(Texture2D row, int height, int finalScale = 1)
    {
        var width = row.width;

        var allpix = row.GetPixels();
        int count = Mathf.CeilToInt(row.height / (float)height);
        List<Texture2D> result = new List<Texture2D>();

        for (int i = 0; i < count; i++)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var newPixs = new Color[width * height];
            int index = i * width * height;
            Array.ConstrainedCopy(allpix, index, newPixs, 0, width * height);
            tex.SetPixels(newPixs);

            tex.Apply();
            if (finalScale != 1)
            {
                tex = ScaleTexture(tex, finalScale);
            }
            result.Add(tex);
        }

        result.Reverse();
        return result.ToArray();
    }


    public static Texture2D ConvertSpriteImagetoTexture(Sprite sprite)
    {
        var croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        var pixels = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                (int)sprite.textureRect.y,
                                                (int)sprite.textureRect.width,
                                                (int)sprite.textureRect.height);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();
        return croppedTexture;
    }

    public static void SetMoveAnimTextureFormat(Texture2D tex)
    {
        var path = AssetDatabase.GetAssetPath(tex);
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        ti.isReadable = true;
        ti.spritePixelsPerUnit = 16;
        ti.filterMode = FilterMode.Point;
        var defaultSetting = ti.GetDefaultPlatformTextureSettings();
        if (defaultSetting.format != TextureImporterFormat.RGBA32)
        {
            defaultSetting.format = TextureImporterFormat.RGBA32;
        }
        ti.SetPlatformTextureSettings(defaultSetting);
    }

    /// <summary>
    /// 设置图片中心点
    /// </summary>
    public static void SetMapAnimSpriteCenter(Texture2D tex)
    {
        var path = AssetDatabase.GetAssetPath(tex);
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        ti.spritePivot = new Vector2(0.5f, 0);
    }

    public static Texture2D ScaleTexture(Texture2D tex, float scale)
    {
        Texture2D result = new Texture2D((int)(tex.width * scale), (int)(tex.height * scale), TextureFormat.RGBA32, false);

        for (int i = 0; i < result.height; i++)
        {
            for (int j = 0; j < result.width; j++)
            {
                Color newColor = tex.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                result.SetPixel(j, i, newColor);
            }
        }
        result.Apply();
        return result;
    }

    public static Texture2D ReplaceColor(Texture2D source, List<Color32> rowColors, List<Color> replaceColors, byte offset = 2)
    {
        Texture2D ReplaceTex = new Texture2D(source.width, source.height, source.format, false);

        var pixs = source.GetPixels32();
        Color[] newColor = new Color[pixs.Length];

        for (int i = 0; i < pixs.Length; i++)
        {
            var pix = pixs[i];
            if ((Color)pix == Color.clear)
                continue;

            var replaceColor = pix;
            var pixColor = rowColors.Find(x =>
                   Mathf.Abs(x.r - pix.r) <= offset &&
                  Mathf.Abs(x.g - pix.g) <= offset &&
                   Mathf.Abs(x.b - pix.b) <= offset);

            if ((Color)pixColor == Color.clear)
                continue;

            int index = rowColors.IndexOf(pixColor);
            if (index != -1)
            {
                replaceColor = replaceColors[index];
            }
            newColor[i] = (Color)replaceColor;
        }

        ReplaceTex.SetPixels(newColor);
        ReplaceTex.Apply();
        return ReplaceTex;
    }

    public static Texture2D ReplaceColor(Texture2D tex, Color32 source, Color target, byte offset = 2)
    {
        Texture2D result = new Texture2D((int)(tex.width), (int)(tex.height), tex.format, false);

        var pixs = tex.GetPixels32();
        Color32[] newColor = new Color32[pixs.Length];

        for (int i = 0; i < pixs.Length; i++)
        {
            var pix = pixs[i];
            var replaceColor = pix;

            if (Mathf.Abs(pix.r - source.r) <= offset &&
                Mathf.Abs(pix.g - source.g) <= offset &&
                Mathf.Abs(pix.b - source.b) <= offset)
            {
                replaceColor = target;
            }
            newColor[i] = replaceColor;
        }

        result.SetPixels32(newColor, 0);
        result.Apply();
        return result;
    }

    public static Texture2D ReplaceColor(Texture2D source, List<Color> rowColors, List<Color> replaceColors, float offset = 0.05f)
    {
        Texture2D ReplaceTex = new Texture2D(source.width, source.height, source.format, false);

        var pixs = source.GetPixels();
        Color[] newColor = new Color[pixs.Length];

        for (int i = 0; i < pixs.Length; i++)
        {
            var pix = pixs[i];
            if (pix == Color.clear)
                continue;

            var replaceColor = pix;
            var pixColor = rowColors.Find(x =>
                   Mathf.Abs(x.r - pix.r) <= offset &&
                  Mathf.Abs(x.g - pix.g) <= offset &&
                   Mathf.Abs(x.b - pix.b) <= offset);

            if (pixColor == Color.clear)
            {
                newColor[i] = pix;
                continue;
            }

            int index = rowColors.IndexOf(pixColor);
            if (index != -1)
            {
                replaceColor = replaceColors[index];
                newColor[i] = replaceColor;
            }
        }

        ReplaceTex.SetPixels(newColor);
        ReplaceTex.Apply();
        return ReplaceTex;
    }

    public static ValueDropdownList<string> GetPaletteColorLst()
    {
        ValueDropdownList<string> result = new ValueDropdownList<string>();

        var allLst = ColorPaletteManager.Instance.ColorPalettes;
        for (int i = 0; i < allLst.Count; i++)
        {
            result.Add(allLst[i].Name, allLst[i].Name);
        }
        return result;
    }

    public static void DrawRectWithOutline(Rect rect, Color color, Color colorOutline)
    {
        Vector3[] rectVerts = { new Vector3(rect.x, rect.y, 0),
                new Vector3(rect.x + rect.width, rect.y, 0),
                new Vector3(rect.x + rect.width, rect.y + rect.height, 0),
                new Vector3(rect.x, rect.y + rect.height, 0) };
        Handles.DrawSolidRectangleWithOutline(rectVerts, color, colorOutline);
    }

    public static void ShowDialog<T>(string defaultDestinationPath, Action<T> onScritpableObjectCreated = null)
       where T : ScriptableObject
    {
        var selector = new ScriptableObjectSelector<T>(defaultDestinationPath, onScritpableObjectCreated);

        if (selector.SelectionTree.EnumerateTree().Count() == 1)
        {
            selector.SelectionTree.EnumerateTree().First().Select();
            selector.SelectionTree.Selection.ConfirmSelection();
        }
        else
        {
            // Else, we'll open up the selector in a popup and let the user choose.
            selector.ShowInPopup(200);
        }
    }

    public static T CreateAssets<T>(string path, string saveName, Action<T> onScritpableObjectCreated = null) where T : ScriptableObject

    {
        var obj = ScriptableObject.CreateInstance(typeof(T));


        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }

        path = string.Format("{0}/{1}.asset", path, saveName);

        AssetDatabase.CreateAsset(obj, path);

        onScritpableObjectCreated?.Invoke(obj as T);

        EditorUtility.SetDirty(obj);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return obj as T;
    }

    private class ScriptableObjectSelector<T> : OdinSelector<Type> where T : ScriptableObject
    {
        private Action<T> onScritpableObjectCreated;
        private string defaultDestinationPath;

        public ScriptableObjectSelector(string defaultDestinationPath, Action<T> onScritpableObjectCreated = null)
        {
            this.onScritpableObjectCreated = onScritpableObjectCreated;
            this.defaultDestinationPath = defaultDestinationPath;
            this.SelectionConfirmed += this.ShowSaveFileDialog;
        }

        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            var scriptableObjectTypes = AssemblyUtilities.GetTypes(AssemblyTypeFlags.CustomTypes)
                .Where(x => x.IsClass && !x.IsAbstract && x.InheritsFrom(typeof(T)));

            tree.Selection.SupportsMultiSelect = false;
            tree.Config.DrawSearchToolbar = true;
            tree.AddRange(scriptableObjectTypes, x => x.GetNiceName())
                .AddThumbnailIcons();
        }

        private void ShowSaveFileDialog(IEnumerable<Type> selection)
        {
            var obj = ScriptableObject.CreateInstance(selection.FirstOrDefault()) as T;

            string dest = this.defaultDestinationPath.TrimEnd('/');

            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
                AssetDatabase.Refresh();
            }

            dest = EditorUtility.SaveFilePanel("Save object as", dest, "New " + typeof(T).GetNiceName(), "asset");

            if (!string.IsNullOrEmpty(dest) && PathUtilities.TryMakeRelative(Path.GetDirectoryName(Application.dataPath), dest, out dest))
            {
                AssetDatabase.CreateAsset(obj, dest);
                AssetDatabase.Refresh();

                if (this.onScritpableObjectCreated != null)
                {
                    this.onScritpableObjectCreated(obj);
                }
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }
    }
}