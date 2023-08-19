using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathFunctionTest : MonoBehaviour
{
    public Transform target;

    public Vector3 targetDir;
    public Vector3 crossRes;

    public float referenceangle;
    public float clampAngle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        Debug.DrawLine(transform.position, transform.up * 5 + transform.position, Color.red, 0.1f);

        targetDir = transform.position.DirectionToXY(target.transform.position);
        Debug.DrawLine(transform.position, targetDir * 5 + transform.position, Color.green,0.1f);

        crossRes = Vector3.Cross(transform.up, targetDir);



    }

    private void OnDrawGizmos()
    {

    }
}
