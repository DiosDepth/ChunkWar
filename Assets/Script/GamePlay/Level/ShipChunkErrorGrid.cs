using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipChunkErrorGrid : MonoBehaviour, IPoolable
{

    public void SetUp(Vector2Int position)
    {
        transform.position = new Vector3(position.x, position.y);
    }

    public void PoolableDestroy()
    {
        PoolableReset();
        PoolManager.Instance.BackObject(this.gameObject.name, this.gameObject);
    }

    public void PoolableReset()
    {
        
    }

    public void PoolableSetActive(bool isactive = true)
    {

    }
}
