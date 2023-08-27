using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MonoShield : MonoBehaviour
{
    public Material Mat;

    [SerializeField]
    [Range(1, 360)]
    private float angle = 360;

    [SerializeField]
    private float Radius;

    private int Quality = 60;

    private MeshFilter m_mesh;
    private MeshRenderer m_render;

    private GeneralShieldHPComponet shieldCmpt;
    private BaseShip ownerShip;

    public void Awake()
    {
        m_mesh = transform.SafeGetComponent<MeshFilter>();
        m_render = transform.SafeGetComponent<MeshRenderer>();
    }

    public void InitShield(GeneralShieldHPComponet cmpt, BaseShip ownerShip)
    {
        shieldCmpt = cmpt;
        this.ownerShip = ownerShip;
        UpdateShieldRatio();
    }

    public void UpdateShieldRatio()
    {
        Radius = shieldCmpt.ShieldRatio;
        GetSector(transform.position, angle, Radius, Quality);
    }

    private void GetSector(Vector3 center, float angle, float radius, int quality)
    {
        float eachAngle = angle / quality;
        List<Vector3> vertices = new List<Vector3>();
        vertices.Add(center);
        for (int i = 0; i <= quality; i++) 
        {
            Vector3 vertex = Quaternion.Euler(0, 0, angle / 2 - eachAngle * i) * Vector2.up * radius;
            vertices.Add(vertex);
        }

        CreateMesh(vertices);
    }

    private void CreateMesh(List<Vector3> vertices)
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

        for(int i =1;i<uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, 1);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles;
        mesh.uv = uvs;
        m_mesh.mesh = mesh;
        m_render.material = Mat;
    }
}
