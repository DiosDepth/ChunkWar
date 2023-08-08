using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolable 
{
    void PoolableReset();
    void PoolableDestroy();
    void PoolableSetActive(bool isactive = true);

}
