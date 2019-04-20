﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder : ThreadedProcess
{
    byte[] faces = new byte[Chunk.size.x * Chunk.size.y * Chunk.size.z];
    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;

    Vector3Int position;
    Block[] blocks;

    int sizeEstimate = 0;
    int vertexIndex = 0, trianglesIndex = 0;
    bool IsVisible = false;

    public MeshBuilder(Vector3Int pos, Block[] blocks)
    {
        this.position = pos;
        this.blocks = blocks;
    }

    public override void ThreadFunction()
    {

        int index = 0;


        for (int x = 0; x < Chunk.size.x; x++)
        {
            for (int y = 0; y < Chunk.size.y; y++)
            {
                for (int z = 0; z < Chunk.size.z; z++)
                {
                    if (blocks[index].IsTransparent())
                    {
                        faces[index] = 0;
                        index++;
                        continue;
                    }

                    if (z == 0)
                    {
                        faces[index] |= (byte)Direction.South;
                        sizeEstimate += 4;
                    }
                    else if (z > 0 && blocks [index - 1] == Block.Air)
                    {
                        faces[index] |= (byte)Direction.South;
                        sizeEstimate += 4;
                    }

                    if (z == Chunk.size.z - 1)
                    {
                        faces[index] |= (byte)Direction.North;
                        sizeEstimate += 4;
                    }
                    else if (z < Chunk.size.z - 1 && blocks[index + 1] == Block.Air)
                    {
                        faces[index] |= (byte)Direction.North;
                        sizeEstimate += 4;
                    }

                    if (y == 0)
                    {
                        faces[index] |= (byte)Direction.Down;
                        sizeEstimate += 4;
                    }
                    else if (y > 0 && blocks[index - Chunk.size.z] == Block.Air)
                    {
                        faces[index] |= (byte)Direction.Down;
                        sizeEstimate += 4;
                    }

                    if (y == Chunk.size.y - 1)
                    {
                        faces[index] |= (byte)Direction.Up;
                        sizeEstimate += 4;
                    }
                    else if (y < Chunk.size.y - 1 && blocks[index + Chunk.size.z] == Block.Air)
                    {
                        faces[index] |= (byte)Direction.Up;
                        sizeEstimate += 4;
                    }

                    if (x == 0)
                    {
                        faces[index] |= (byte)Direction.West;
                        sizeEstimate += 4;
                    }
                    else if (x > 0 && blocks[index - Chunk.size.z * Chunk.size.y ] == Block.Air)
                    {
                        faces[index] |= (byte)Direction.West;
                        sizeEstimate += 4;
                    }

                    if (x == Chunk.size.x - 1)
                    {
                        faces[index] |= (byte)Direction.East;
                        sizeEstimate += 4;
                    }
                    else if (x < Chunk.size.x - 1 && blocks[index + Chunk.size.z * Chunk.size.y] == Block.Air)
                    {
                        faces[index] |= (byte)Direction.East;
                        sizeEstimate += 4;
                    }
                    IsVisible = true;
                    index++;
                }
            }
        }
        if (IsVisible == false)
        {
            return;
        }

        // Generate Mesh
        
        vertices = new Vector3[sizeEstimate];
        uvs = new Vector2[sizeEstimate];
        triangles = new int [(int)(sizeEstimate * 1.5f)];

        index = 0;

        for (int x = 0; x < Chunk.size.x; x++)
        {
            for (int y = 0; y < Chunk.size.y; y++)
            {
                for (int z = 0; z < Chunk.size.z; z++)
                {
                    if (faces[index] == 0)
                    {
                        index++;
                        continue;
                    }
                    if ((faces[index] & (byte)Direction.North) != 0)
                    {
                        vertices[vertexIndex] = new Vector3(x + position.x, y + position.y, z + position.z + 1);
                        vertices[vertexIndex + 2] = new Vector3(x + position.x, y + position.y + 1, z + position.z + 1);
                        vertices[vertexIndex + 1] = new Vector3(x + position.x + 1, y + position.y, z + position.z + 1);
                        vertices[vertexIndex + 3] = new Vector3(x + position.x + 1, y + position.y + 1, z + position.z + 1);

                        triangles[trianglesIndex] = vertexIndex + 1;
                        triangles[trianglesIndex + 1] = vertexIndex + 2;
                        triangles[trianglesIndex + 2] = vertexIndex;

                        triangles[trianglesIndex + 3] = vertexIndex + 1;
                        triangles[trianglesIndex + 4] = vertexIndex + 3;
                        triangles[trianglesIndex + 5] = vertexIndex + 2;

                        vertexIndex += 4;
                        trianglesIndex += 6;
                    }
                    if ((faces[index] & (byte)Direction.East) != 0)
                    {
                        vertices[vertexIndex] = new Vector3(x + position.x + 1, y + position.y, z + position.z);
                        vertices[vertexIndex + 1] = new Vector3(x + position.x + 1, y + position.y, z + position.z + 1);
                        vertices[vertexIndex + 2] = new Vector3(x + position.x + 1, y + position.y + 1, z + position.z);
                        vertices[vertexIndex + 3] = new Vector3(x + position.x + 1, y + position.y + 1, z + position.z + 1);

                        triangles[trianglesIndex] = vertexIndex;
                        triangles[trianglesIndex + 1] = vertexIndex + 2;
                        triangles[trianglesIndex + 2] = vertexIndex + 1;

                        triangles[trianglesIndex + 3] = vertexIndex + 2;
                        triangles[trianglesIndex + 4] = vertexIndex + 3;
                        triangles[trianglesIndex + 5] = vertexIndex + 1;

                        vertexIndex += 4;
                        trianglesIndex += 6;
                    }
                    if ((faces[index] & (byte)Direction.South) != 0)
                    {
                        vertices[vertexIndex] = new Vector3(x + position.x , y + position.y, z + position.z);
                        vertices[vertexIndex + 1] = new Vector3(x + position.x + 1, y + position.y, z + position.z);
                        vertices[vertexIndex + 2] = new Vector3(x + position.x, y + position.y + 1, z + position.z);
                        vertices[vertexIndex + 3] = new Vector3(x + position.x + 1, y + position.y + 1, z + position.z);

                        triangles[trianglesIndex] = vertexIndex;
                        triangles[trianglesIndex + 1] = vertexIndex + 2;
                        triangles[trianglesIndex + 2] = vertexIndex + 1;

                        triangles[trianglesIndex + 3] = vertexIndex + 2;
                        triangles[trianglesIndex + 4] = vertexIndex + 3;
                        triangles[trianglesIndex + 5] = vertexIndex + 1;

                        vertexIndex += 4;
                        trianglesIndex += 6;
                    }
                    if ((faces[index] & (byte)Direction.West) != 0)
                    {
                        vertices[vertexIndex] = new Vector3(x + position.x, y + position.y, z + position.z);
                        vertices[vertexIndex + 2] = new Vector3(x + position.x, y + position.y + 1, z + position.z);
                        vertices[vertexIndex + 1] = new Vector3(x + position.x, y + position.y, z + position.z + 1);
                        vertices[vertexIndex + 3] = new Vector3(x + position.x, y + position.y + 1, z + position.z + 1);

                        triangles[trianglesIndex] = vertexIndex + 1;
                        triangles[trianglesIndex + 1] = vertexIndex + 2;
                        triangles[trianglesIndex + 2] = vertexIndex ;

                        triangles[trianglesIndex + 3] = vertexIndex + 1;
                        triangles[trianglesIndex + 4] = vertexIndex + 3;
                        triangles[trianglesIndex + 5] = vertexIndex + 2;

                        vertexIndex += 4;
                        trianglesIndex += 6;
                    }
                    if ((faces[index] & (byte)Direction.Up) != 0)
                    {
                        vertices[vertexIndex] = new Vector3(x + position.x, y + position.y + 1, z + position.z);
                        vertices[vertexIndex + 1] = new Vector3(x + position.x + 1, y + position.y + 1, z + position.z);
                        vertices[vertexIndex + 2] = new Vector3(x + position.x, y + position.y + 1, z + position.z + 1);
                        vertices[vertexIndex + 3] = new Vector3(x + position.x + 1, y + position.y + 1, z + position.z + 1);

                        triangles[trianglesIndex] = vertexIndex;
                        triangles[trianglesIndex + 1] = vertexIndex + 2;
                        triangles[trianglesIndex + 2] = vertexIndex + 1;

                        triangles[trianglesIndex + 3] = vertexIndex + 2;
                        triangles[trianglesIndex + 4] = vertexIndex + 3;
                        triangles[trianglesIndex + 5] = vertexIndex + 1;

                        vertexIndex += 4;
                        trianglesIndex += 6;
                    }
                    if ((faces[index] & (byte)Direction.Down) != 0)
                    {
                        vertices[vertexIndex] = new Vector3(x + position.x, y + position.y, z + position.z);
                        vertices[vertexIndex + 1] = new Vector3(x + position.x, y + position.y, z + position.z + 1);
                        vertices[vertexIndex + 2] = new Vector3(x + position.x + 1, y + position.y, z + position.z);
                        vertices[vertexIndex + 3] = new Vector3(x + position.x + 1, y + position.y, z + position.z + 1);

                        triangles[trianglesIndex] = vertexIndex;
                        triangles[trianglesIndex + 1] = vertexIndex + 2;
                        triangles[trianglesIndex + 2] = vertexIndex + 1;

                        triangles[trianglesIndex + 3] = vertexIndex + 2;
                        triangles[trianglesIndex + 4] = vertexIndex + 3;
                        triangles[trianglesIndex + 5] = vertexIndex + 1;

                        vertexIndex += 4;
                        trianglesIndex += 6;
                    }
                    index++;
                }
            }
        }
    }
    public Mesh GetMesh(ref Mesh copy)
    {
        if (copy == null)
        {
            copy = new Mesh();
        }
        else
        {
            copy.Clear();
        }
        if (IsVisible == false || vertexIndex == 0)
        {
            return copy;
        }
        if (vertexIndex > 65000)
        {
            copy.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }
        copy.vertices = vertices;
        copy.uv = uvs;
        copy.triangles = triangles;

        copy.RecalculateNormals();

        return copy;
    }
}
