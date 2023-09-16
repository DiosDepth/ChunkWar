using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipChunkGrid : MonoBehaviour, IPoolable
{

    public int PosX;
    public int PosY;

    public Sprite Grid_Empty;
    public Sprite Grid_Error;
    public Sprite Grid_Fill;

    private SpriteRenderer _fill;

    public void Awake()
    {
        _fill = transform.Find("BG").SafeGetComponent<SpriteRenderer>();
    }

    public void SetUp(Vector2Int position, bool occupied)
    {
        PosX = position.x;
        PosY = position.y;
        transform.position = new Vector3(position.x, position.y);
        SetOccupied(occupied);
    }

    public void SetOccupied(bool occupied)
    {
        _fill.sprite = occupied ? Grid_Fill : Grid_Empty;
    }

    public void SetGridNormalEmpty()
    {
        _fill.sprite = Grid_Empty;
    }

    public void SetGridError()
    {
        _fill.sprite = Grid_Error;
    }

    public void PoolableDestroy()
    {

    }

    public void PoolableReset()
    {

    }

    public void PoolableSetActive(bool isactive = true)
    {

    }
}
