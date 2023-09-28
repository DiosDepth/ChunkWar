using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.Utilities;

public class ShipUnitEditorPreivewItem
{
    public int X;
    public int Y;

    public Texture2D tex;
    public int Value;
}

[CreateAssetMenu(fileName = "Configs_PlayerShip_", menuName = "Configs/Unit/PlayerShipConfig")]
public class PlayerShipConfig : BaseShipConfig
{

    [LabelText("���Ĳ��ID")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 150)]
    public int CorePlugID;

    [LabelText("��ӪID")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 150)]
    public int PlayerShipCampID;

    [LabelText("Ĭ�Ͻ���")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 150)]
    public bool UnlockDefault;

    [LabelText("EditorPrefab")]
    [LabelWidth(80)]
    [HorizontalGroup("B", 300)]
    public GameObject EditorPrefab;

    [LabelText("���Զ�������")]
    [LabelWidth(100)]
    public string ShipPropertyDesc;

    [FoldoutGroup("����")]
    [LabelText("���������Ѷ�")]
    [LabelWidth(100)]
    public List<int> ShipHardLevels = new List<int>();

    [FoldoutGroup("����")]
    [LabelText("��ʼ���")]
    [LabelWidth(100)]
    public List<int> ShipOriginPlugs = new List<int>();

    [OnValueChanged("OnShipEditorSpriteChange")]
    public Sprite ShipEditorSprite;

    private Dictionary<Vector2Int, Texture2D> _textureMap;

    [TableMatrix(ResizableColumns = false, SquareCells = true, DrawElementMethod = "DrawPreviewTable")]
    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/B", 800)]
    [ShowInInspector]
    private ShipUnitEditorPreivewItem[,] PreviewItems;

    [FoldoutGroup("��������")]
    [HorizontalGroup("��������/A", 200)]
    [Button("ͬ������")]
    public void SetData()
    {
        if (PreviewItems == null)
            return;

        for (int x = 0; x < Map.GetLength(0); x++)
        {
            for (int y = 0; y < Map.GetLength(1); y++)
            {
                Map[x, y] = PreviewItems[x, y].Value;
            }
        }
        CalculateSlotCount();
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [System.Obsolete]
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override Vector2Int GetMapPivot()
    {
        return base.GetMapPivot();
    }

    private void OnShipEditorSpriteChange()
    {

    }

    [OnInspectorInit]
    protected override void InitData()
    {
        base.InitData();
        _textureMap = new Dictionary<Vector2Int, Texture2D>();
        CacheShipTexture();
    }

    [OnInspectorDispose]
    private void OnDispose()
    {
        _textureMap.Clear();
        SetData();
    }

    protected ShipUnitEditorPreivewItem DrawPreviewTable(Rect rect, ShipUnitEditorPreivewItem item)
    {
        if(item.tex != null)
        {
            EditorGUI.DrawTextureTransparent(rect.Padding(0.5f), item.tex);
            EditorGUI.DrawRect(rect.Padding(0.5f), new Color32(0,0,0,100));
        }


        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            item.Value += 1;
            GUI.changed = true;
            Event.current.Use();
            if (item.Value == 3)
            {
                item.Value = 0;
            }
        }

        if (item.Value == 1)
        {
            UnityEditor.EditorGUI.DrawRect(rect.Padding(1), new Color32(0, 255, 0, 80));
            UnityEditor.EditorGUI.LabelField(rect.AlignCenterXY(rect.width, rect.height), item.Value.ToString());
        }
        else if (item.Value == 2)
        {
            UnityEditor.EditorGUI.DrawRect(rect.Padding(1), new Color32(255, 0, 0, 80));
            UnityEditor.EditorGUI.LabelField(rect.AlignCenterXY(rect.width, rect.height), item.Value.ToString());
        }
        return item;
    }

    private void DrawShipBG()
    {
        if (ShipEditorSprite == null)
            return;


    }

    private void CacheShipTexture()
    {
        var pix = GameGlobalConfig.ShipSlotPix;

        if (ShipEditorSprite == null)
            return;

        var tex = ShipEditorSprite.texture;
        if (tex.height % pix != 0 || tex.width % pix != 0)
        {
            Debug.LogError(string.Format("������񲻷�������Ϊ {0} �ı���", pix));
            return;
        }

        if ((tex.width / pix) % 2 == 0)
        {
            Debug.LogError("�����ߴ���󣡿����ҪΪ������pix");
            return;
        }

        if (!tex.isReadable)
        {
            Debug.LogError("��Ҫ���ý���ͼƬ isReadable!");
            return;
        }

        int width = tex.width / pix;
        int height = tex.height / pix;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Texture2D newTex = new Texture2D(pix, pix, TextureFormat.RGBA32, false);
                _textureMap.Add(new Vector2Int(i, j), newTex);
            }
        }

        Dictionary<Vector2Int, Color32[]> colorMap = new Dictionary<Vector2Int, Color32[]>();
        var allPix = tex.GetPixels32(0);
        for (int i = 0; i < tex.height; i++)
        {
            for (int j = 0; j < tex.width; j++)
            {
                var pixColor = allPix[i * tex.width + j];
                int indexX = j / pix;
                int indexY = i / pix;
                var index = new Vector2Int(indexX, indexY);

                int colorIndex = i % pix * pix + j % pix;
                if (colorMap.ContainsKey(index))
                {
                    colorMap[index][colorIndex] = pixColor;
                }
                else
                {
                    colorMap.Add(index, new Color32[pix * pix]);
                    colorMap[index][colorIndex] = pixColor;
                }
            }
        }

        foreach (var item in _textureMap)
        {
            var newIndex = new Vector2Int(item.Key.x, height - item.Key.y - 1);
            if (colorMap.ContainsKey(newIndex))
            {
                item.Value.SetPixels32(colorMap[newIndex]);
            }
            item.Value.Apply();
        }

        InitPreviewItems();
    }

    private void InitPreviewItems()
    {
        int maxSize = GameGlobalConfig.ShipMaxSize;
        PreviewItems = new ShipUnitEditorPreivewItem[maxSize, maxSize];
        var tex = ShipEditorSprite.texture;
        int shipXSize = tex.width / GameGlobalConfig.ShipSlotPix;
        int shipYSize = tex.height / GameGlobalConfig.ShipSlotPix;

        int xOffsetStart = (maxSize - 1) / 2 - (shipXSize - 1) / 2;
        int xOffsetEnd = (maxSize - 1) / 2 + (shipXSize - 1) / 2;
        int yOffsetStart = (maxSize - 1) / 2 - (shipYSize - 1) / 2;
        int yOffsetEnd = (maxSize - 1) / 2 + (shipYSize - 1) / 2;

        for (int i = 0; i < maxSize; i++) 
        {
            for (int j = 0; j < maxSize; j++) 
            {
                ShipUnitEditorPreivewItem item = new ShipUnitEditorPreivewItem()
                {
                    X = i,
                    Y = j,
                };

                if(i >= xOffsetStart && i <= xOffsetEnd && j >= yOffsetStart && j <= yOffsetEnd)
                {
                    var index = new Vector2Int(i - xOffsetStart, j - yOffsetStart);
                    if (_textureMap.ContainsKey(index))
                    {
                        item.tex = _textureMap[index];
                        item.Value = Map[i,j];
                    }
                }

                PreviewItems[i, j] = item;
            }
        }
    }

}
