using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Triangle
{
    public Color color;
    public int index1;
    public int index2;
    public int index3;

    public Triangle(int index1, int index2, int index3)
    {
        this.index1 = index1;
        this.index2 = index2;
        this.index3 = index3;
    }

    public override bool Equals(object obj)
    {
        var triangle = obj as Triangle;
        if (triangle == null)
        {
            return false;
        }
        // This ensures that the triangle is the same regardless of the order of the indices.
        var indicesThis = new List<int> { index1, index2, index3 };
        var indicesTriangle = new List<int> { triangle.index1, triangle.index2, triangle.index3 };
        indicesThis.Sort();
        indicesTriangle.Sort();

        return indicesThis.SequenceEqual(indicesTriangle);
    }

    public override int GetHashCode()
    {
        var indicesThis = new List<int> { index1, index2, index3 };
        indicesThis.Sort();
        return indicesThis[0].GetHashCode() ^ indicesThis[1].GetHashCode() << 2 ^ indicesThis[2].GetHashCode() >> 2;
    }
}

public class RayCastHitVertex : MonoBehaviour
{
    private Triangle lastTriangle;
    public bool firstTime = true;
    public Vector3[] vertices;
    public int[] triangles;
    public Mesh mesh;
    public LayerMask mask;

    // store hit triangles for painting
    [SerializeField]
    public List<Triangle> hitTriangles = new List<Triangle>();

    void Start()
    {
        lastTriangle = new Triangle(0, 0, 0);
    }

    void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1000f, mask))
        {
            MeshCollider meshCollider = hit.collider as MeshCollider;

            if (meshCollider == null || meshCollider.sharedMesh == null)
                return;

            if (meshCollider.sharedMesh != mesh && meshCollider.name == "Terrain(Clone)" || firstTime == true && meshCollider.name == "Terrain(Clone)")
            {
                firstTime = false;
                mesh = meshCollider.sharedMesh;
                vertices = mesh.vertices;
                triangles = mesh.triangles;
            }

            Triangle currentTriangle = new Triangle(
            triangles[hit.triangleIndex * 3 + 0],
            triangles[hit.triangleIndex * 3 + 1],
            triangles[hit.triangleIndex * 3 + 2]
            );

            // Add the current triangle to the list if it's not already there
            if (!hitTriangles.Contains(currentTriangle))
            {
                hitTriangles.Add(currentTriangle);
            }

            lastTriangle = currentTriangle;

            // Debugging stuff
            Debug.DrawRay(transform.position, Vector3.down * hit.distance, Color.yellow);
            Debug.DrawLine(vertices[lastTriangle.index1], vertices[lastTriangle.index2], Color.red);
            Debug.DrawLine(vertices[lastTriangle.index2], vertices[lastTriangle.index3], Color.red);
            Debug.DrawLine(vertices[lastTriangle.index3], vertices[lastTriangle.index1], Color.red);
        }
    }
}
