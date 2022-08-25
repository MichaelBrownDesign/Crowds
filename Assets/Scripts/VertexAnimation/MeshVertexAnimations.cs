using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct VertexAnimation
{
    public string       name;
    public int          frames;
    public Vector3[]    vertices;
}

[CreateAssetMenu(menuName = "Mesh Vertex Animations")]
[PreferBinarySerialization]
public class MeshVertexAnimations : ScriptableObject
{
    public Mesh mesh;
    public VertexAnimation[] animations;

    private int vertexCount = -1;

    public int VertexCount
    {
        get
        {
            if(vertexCount < 0)
            {
                vertexCount = mesh.vertices.Length;
            }
            return vertexCount;
        }
    }
}
