using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IJobDisposeable 
{
    void Dispose();
    void DisposeReturnValue();
    void UpdateData();

}
