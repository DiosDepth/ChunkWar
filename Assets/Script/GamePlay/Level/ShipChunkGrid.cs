using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipChunkGrid : MonoBehaviour, IPoolable
{

    public int PosX;
    public int PosY;

    private SpriteRenderer _fill;

    public void Awake()
    {
        _fill = transform.Find("Fill").SafeGetComponent<SpriteRenderer>();
    }

    public void SetUp(Vector2Int position, bool occupied)
    {
        PosX = position.x;
        PosY = position.y;
        transform.position = new Vector3(position.x, position.y);
        _fill.transform.SafeSetActive(occupied);
    }

    public void SetOccupied(bool occupied)
    {
        _fill.transform.SafeSetActive(occupied);
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
