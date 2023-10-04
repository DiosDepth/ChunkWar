using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum IndicatorShape
{
    Rect,
    Circle,
    Line,
}
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class DamageIndicator : MonoBehaviour, IPoolable
{

    public MeshFilter meshFilter;
    public MeshRenderer meshRender;
    public Material Mat;

    [Header("---IndicatorSettings---")]
    public IndicatorShape shape;
    private float _lifeTime;

    [Header("---RectSettings---")]
    public Vector2 rectSize;

    [Header("---CircleSettings---")]
    public float radius;
    [SerializeField]
    [Range(1, 360)]
    public float angle;
    public int quality = 16;


    private List<Vector3> _vertices;
    // Start is called before the first frame update
    void Start()
    {
        switch (shape)
        {
            case IndicatorShape.Rect:
                break;
            case IndicatorShape.Circle:
                _vertices = CreateCircle(transform.position, angle, radius, quality);
                break;
            case IndicatorShape.Line:
                break;
        }


        DrawIndicatorMesh(_vertices);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetIndicator()
    {

    }

    private List<Vector3> CreateCircle(Vector3 center, float angle, float radius, int quality)
    {
        float eachAngle = angle / quality;
        List<Vector3> vertices = new List<Vector3>();

        vertices.Add(center);
        for (int i = 0; i <= quality; i++)
        {
            Vector3 vertex = center + Quaternion.Euler(0, 0, angle / 2 - eachAngle * i) * Vector2.up * radius;
            vertices.Add(vertex);
        }
        return vertices;
    }

    private List<Vector3> CreateRect(Vector3 center, float length, float width)
    {
        List<Vector3> vertices = new List<Vector3>();


        return vertices;
    }

    private void DrawIndicatorMesh(List<Vector3> vertices)
    {
        int[] triangles;
        int triangleCount = vertices.Count - 2;
        triangles = new int[3 * triangleCount];
        for (int i = 0; i < triangleCount; i++)
        {
            triangles[3 * i] = 0;
            triangles[3 * i + 1] = i + 1;
            triangles[3 * i + 2] = i + 2;
        }

        Vector2[] uvs = new Vector2[vertices.Count];
        uvs[0] = new Vector2(vertices[0].x, vertices[0].y);

        for (int i = 1; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, 1);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles;
        mesh.uv = uvs;
        meshFilter.mesh = mesh;
        meshRender.material = Mat;
    }

    public void PoolableReset()
    {
        throw new System.NotImplementedException();
    }

    public void PoolableDestroy()
    {
        throw new System.NotImplementedException();
    }

    public void PoolableSetActive(bool isactive = true)
    {
        throw new System.NotImplementedException();
    }
}
