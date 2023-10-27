using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBoid
{
    
    Vector3 GetPosition();
    Vector3 GetVelocity();
    float GetRadius();
    float GetRotationZ();
    void SetVelocity( Vector3 m_vect);

    void UpdateBoid();
}
