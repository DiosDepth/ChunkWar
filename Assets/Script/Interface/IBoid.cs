using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBoid
{
    
    Vector3 GetPosition();
    Vector3 GetVelocity();
    void SetVelocity( Vector3 m_vect);

    void UpdateIBoid();
}
