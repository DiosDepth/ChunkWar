
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;

public interface IOtherTarget
{
    public GameObject GetGameObject();
    public bool GetActiveAndEnabled();
    public float3 GetPosition();
    public void OnUpdateBattle();
}
