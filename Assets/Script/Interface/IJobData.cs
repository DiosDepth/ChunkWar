using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IJobData 
{

    void Dispose();
    void DisposeReturnValue();
    void UpdateData();

}
