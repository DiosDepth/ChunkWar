using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum IndicatorShape
{
    Rect,
    Circle,
    Line,
}


[Serializable]
public class Indicator
{
    [SerializeField]
    public MeshFilter meshFilter;
    [SerializeField]
    public MeshRenderer meshRender;
    [SerializeField]
    public Material mat;
    [HideInInspector]
    public Vector3 center;


    protected List<Vector3> vertices = new List<Vector3>();
    public Indicator(Vector3 m_center)
    {
        center = m_center;
    }
    public Indicator (Vector3 m_center, MeshFilter m_meshFilter, MeshRenderer m_meshRenderer, Material m_mat)
    {
        center = m_center;
        meshFilter = m_meshFilter;
        meshRender = m_meshRenderer;
        mat = m_mat;
    }

 
    public virtual List<Vector3> CreateVertices()
    {
        return null;
    }

    public virtual void DrawIndicator()
    {
        if(vertices == null || vertices.Count == 0) 
        {
            Debug.LogError("Vertices data is null or 0 count , please make sure you already created vertices, you should call CreateVertices() before DrawIndicator()");
            return;
        }
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
        meshRender.material = mat;
    }

}

[Serializable]
public class RectIndicator: Indicator
{

    [Header("---RectSettings---")]
    public Vector2 rectSize;


    public RectIndicator(Vector3 m_center, Vector2 m_rectsize, MeshFilter m_meshFilter, MeshRenderer m_meshRenderer, Material m_mat) : base(m_center, m_meshFilter, m_meshRenderer, m_mat)
    {
        rectSize = m_rectsize;
    }
    public override List<Vector3> CreateVertices()
    {
   


        return vertices;
    }

}

[Serializable]
public class CircleIndicator:Indicator
{
    [Header("---CircleSettings---")]
    public float radius = 3;
    [SerializeField]
    [Range(1, 360)]
    public float angle= 360;
    public int quality = 16;

    public CircleIndicator(Vector3 m_center, MeshFilter m_meshFilter, MeshRenderer m_meshRenderer, Material m_mat) : base(m_center, m_meshFilter, m_meshRenderer, m_mat) { }

    public CircleIndicator(Vector3 m_center, float m_radius , float m_angle , int m_quality , MeshFilter m_meshFilter, MeshRenderer m_meshRenderer, Material m_mat) : base(m_center, m_meshFilter, m_meshRenderer, m_mat)
    {
        radius = m_radius;
        angle = m_angle;
        quality = m_quality;
    }

    public override List<Vector3> CreateVertices()
    {
        float eachAngle = angle / quality;


        vertices.Add(center);
        for (int i = 0; i <= quality; i++)
        {
            Vector3 vertex = center + Quaternion.Euler(0, 0, angle / 2 - eachAngle * i) * Vector2.up * radius;
            vertices.Add(vertex);
        }
        return vertices;
    }
}

[Serializable]
public class LineIndicator:Indicator
{
    public float length;

    public LineIndicator(Vector3 m_center, float m_length): base(m_center)
    {
        length = m_length;
    }
    
    public override List<Vector3> CreateVertices()
    {
        return vertices;
    }
}


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class DamageIndicator : MonoBehaviour, IPoolable, IPauseable
{

    [Header("---IndicatorSettings---")]
    public IndicatorShape shape;
    public MeshFilter meshFilter;
    public MeshRenderer meshRender;
    public Material mat;
    private float _lifeTime;

    [HideInInspector]
    public Indicator indicator;



    private List<Vector3> _vertices;
    // Start is called before the first frame update
    void Start()
    {

    }



    public virtual void Initialization(IndicatorShape m_shape, float m_lifetime = -1)
    {
        GameManager.Instance.RegisterPauseable(this);
        shape = m_shape;
        _lifeTime = m_lifetime;
    }
    public virtual void  CreateIndicator()
    {
        switch (shape)
        {
            case IndicatorShape.Rect:

                break;
            case IndicatorShape.Circle:
                indicator = new CircleIndicator(transform.position, meshFilter, meshRender, mat);
                indicator.CreateVertices();
                break;
            case IndicatorShape.Line:
                break;
        }
    }

    // Update is called once per frame
    public virtual void Update()
    {
      
    }

    public virtual void ShowIndicator()
    {
        indicator.DrawIndicator();
    }

    public virtual void UpdateIndicator()
    {
        if (GameManager.Instance.IsPauseGame()) { return; }

    }

    public virtual void RemoveIndicator()
    {
        PoolableDestroy();
    }

    public virtual void PoolableReset()
    {
        _lifeTime = -1;
    }

    public virtual void PoolableDestroy()
    {
        GameManager.Instance.UnRegisterPauseable(this);
    }

    public virtual void PoolableSetActive(bool isactive = true)
    {
        
    }

    public virtual void PauseGame()
    {
       
    }

    public virtual void UnPauseGame()
    {
        
    }
}
