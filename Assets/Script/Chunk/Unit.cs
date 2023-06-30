using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum UnitType
{
    ChunkParts,
    Buildings,
    Ship,
}
public class Unit : MonoBehaviour
{
    public string unitName;
    public float hp = 100;
    public UnitState state = UnitState.None;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
