using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipChunkGrid : MonoBehaviour, IPoolable
{

    private SpriteRenderer _fill;

    public void Awake()
    {
        _fill = transform.Find("Fill").SafeGetComponent<SpriteRenderer>();
    }

    public void SetUp(Vector2 position, bool occupied)
    {
        transform.position = position;
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
