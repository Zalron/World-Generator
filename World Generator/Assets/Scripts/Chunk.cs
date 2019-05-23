using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public static int chunkSize = 16;
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    bool[,,] blockMap = new bool[chunkSize, chunkSize, chunkSize];

    public void Start()
    {
        PopulateBlockMap();
        CreateChunkMesh();
        CreateMesh();
    }
    void PopulateBlockMap()
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    blockMap[x, y, z] = true;
                }
            }
        }
    }
    void AddBlockDataToChunk(Vector3 pos)
    {
        for (int p = 0; p < 6; p++)
        {
            if (!CheckVoxel(pos + Block.faceChecks[p]))
            {
                vertices.Add(pos + Block.Verts[Block.Tris[p, 0]]);
                vertices.Add(pos + Block.Verts[Block.Tris[p, 1]]);
                vertices.Add(pos + Block.Verts[Block.Tris[p, 2]]);
                vertices.Add(pos + Block.Verts[Block.Tris[p, 3]]);
                uvs.Add(Block.UVs[0]);
                uvs.Add(Block.UVs[1]);
                uvs.Add(Block.UVs[2]);
                uvs.Add(Block.UVs[3]);
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;
            }
        }
    }
    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        if (x < 0 || x > chunkSize - 1 || y < 0 || y > chunkSize - 1 || z < 0 || z > chunkSize - 1)
        {
            return false;
        }
        return blockMap[x, y, z];
    }
    void CreateChunkMesh()
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    AddBlockDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }
    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        {
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
            meshFilter.sharedMesh = mesh;
        }
    }
}
