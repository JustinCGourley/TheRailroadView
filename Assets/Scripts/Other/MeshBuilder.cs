using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder
{
    public GameObject BuildTerrainPlane(float width, float height, float uvPercentageX, float uvPercentageY, float uvSize)
    {
        GameObject obj = new GameObject("Plane");
        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(0,0,0),
            new Vector3(width, 0, 0),
            new Vector3(0, 0, height),
            new Vector3(width, 0, height),
        };

        mesh.uv = new Vector2[]
        {
            new Vector2(uvPercentageX, uvPercentageY),
            new Vector2(uvPercentageX, uvPercentageY + uvSize),
            new Vector2(uvPercentageX + uvSize, uvPercentageY),
            new Vector2(uvPercentageX + uvSize, uvPercentageY + uvSize)
        };

        mesh.triangles = new int[] { 0, 2, 3, 3, 1, 0 };

        meshFilter.mesh = mesh;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return obj;
    }

    public GameObject BuildCliffPlane(float width, float height)
    {
        GameObject obj = new GameObject("Plane");
        MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(0,0,0),
            new Vector3(width, 0, 0),
            new Vector3(0, height, 0),
            new Vector3(width, height, 0),
        };

        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(1, 1)
        };

        mesh.triangles = new int[] { 0, 2, 3, 3, 1, 0 };

        meshFilter.mesh = mesh;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return obj;
    }
}
