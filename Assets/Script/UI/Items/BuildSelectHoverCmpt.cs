using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class BuildSelectHoverCmpt : UnitDetailHover
{
    private Image _sprite;
    private RectTransform _spriteRect;

    private static float SizePerUnit = 44;

    protected override void Awake()
    {
        _sprite = transform.Find("Corsor").SafeGetComponent<Image>();
        _spriteRect = transform.Find("Corsor").SafeGetComponent<RectTransform>();
        base.Awake();
    }

    public void SetUp(Vector2 pos, Vector2 size, Vector2 offset, int unitID, uint uid)
    {
        var height = size.y * SizePerUnit;
        var width = size.x * SizePerUnit;
        _spriteRect.SetRectHeight(height);
        _spriteRect.SetRectWidth(width);

        Vector2 newPos = new Vector2(pos.x - width / 2 + offset.x * SizePerUnit, pos.y + height / 2 + offset.y * SizePerUnit);
        transform.localPosition = newPos;
      
        int minSize = (int)Mathf.Min(size.x, size.y);
        int sizeDelta = 3 - minSize;
        _sprite.pixelsPerUnitMultiplier = Mathf.Clamp(sizeDelta, 1, sizeDelta) * 1.1f;
        _itemUID = uid;
        SetUp(unitID, false);
    }

    public override void Update()
    {

    }
}
