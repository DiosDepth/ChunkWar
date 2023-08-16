using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathFunctionTest : MonoBehaviour
{
    public Transform target;

    public Vector3 referenceDir;
    public Vector3 randomDir;

    public float referenceangle;
    public float randomangle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        referenceDir = transform.position.DirectionToXY(target.transform.position);
        Debug.DrawLine(transform.position, referenceDir * 5 + transform.position, Color.green,0.1f);



        Vector3 dir = MathExtensionTools.GetRandomDirection(referenceDir, 10);
        //randomDir = MathExtensionTools.GetRandomDirection(referenceDir, 15);
        Debug.DrawLine(transform.position, dir * 5 + transform.position, Color.red, 0.1f);
    }

    private void OnDrawGizmos()
    {

    }
}
