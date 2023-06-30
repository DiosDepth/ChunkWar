using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEntity : MonoBehaviour
{
    public string levelName;
    public Vector3 startPoint;
    public PolygonCollider2D cameraBoard;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    public virtual void Initialization()
    {

    }

    public virtual void Unload()
    {

    }
}
