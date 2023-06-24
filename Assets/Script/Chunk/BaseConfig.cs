using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseConfig : ScriptableObject
{
    public int ID;
    public string UnitName;
    public float HP = 100;
    public Sprite Icon;
    public GameObject Prefab;

}
