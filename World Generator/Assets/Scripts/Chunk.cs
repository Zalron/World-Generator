using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace WorldGenerator
{
    public class Chunk
    {
        public ChunkCoord coord;
        GameObject chunkObject;
        public MeshRenderer meshRenderer;
        public MeshFilter meshFilter;

        public static int chunkSize = 16;
        int vertexIndex = 0;
        readonly List<Vector3> vertices = new List<Vector3>();
        readonly List<int> triangles = new List<int>();
        readonly List<Vector2> uvs = new List<Vector2>();

        readonly byte[,,] blockMap = new byte[chunkSize, chunkSize, chunkSize];

        World world;
        public Chunk(ChunkCoord _coord, World _world)
        {
            coord = _coord;
            world = _world;
            chunkObject = new GameObject();
            meshFilter = chunkObject.AddComponent<MeshFilter>();
            meshRenderer = chunkObject.AddComponent<MeshRenderer>();
            meshRenderer.material = world.material;
            chunkObject.transform.SetParent(world.transform);
            chunkObject.transform.position = new Vector3(coord.x * chunkSize, coord.y * chunkSize, coord.z * chunkSize);
            chunkObject.name = "Chunk " + coord.x + "x, " + coord.z + "z, " + coord.y + "y";
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
                        blockMap[x, y, z] = world.GetBlock(new Vector3(x, y, z) + position);
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
                    byte blockID = blockMap[(int)pos.x, (int)pos.y, (int)pos.x];
                    vertices.Add(pos + Block.Verts[Block.Tris[p, 0]]);
                    vertices.Add(pos + Block.Verts[Block.Tris[p, 1]]);
                    vertices.Add(pos + Block.Verts[Block.Tris[p, 2]]);
                    vertices.Add(pos + Block.Verts[Block.Tris[p, 3]]);
                    AddTexture(world.blockType[blockID].GetTextureID(p));
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
        bool IsVoxelInChunk(int x, int y, int z)
        {
            if (x < 0 || x > chunkSize - 1 || y < 0 || y > chunkSize - 1 || z < 0 || z > chunkSize - 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        bool CheckVoxel(Vector3 pos)
        {
            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);
            int z = Mathf.FloorToInt(pos.z);
            if (!IsVoxelInChunk(x,y,z))
            {
                return world.blockType[world.GetBlock(pos + position)].isSolid;
            }
            return world.blockType[blockMap[x, y, z]].isSolid;
        }
        public bool IsActive
        {
            get { return chunkObject.activeSelf; }
            set { chunkObject.SetActive(value); }
        }
        public Vector3 position
        {
            get { return chunkObject.transform.position; }
        }
        void CreateChunkMesh()
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        if (world.blockType[blockMap[x,y,z]].isSolid)
                        {
                            AddBlockDataToChunk(new Vector3(x, y, z));
                        }
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
        void AddTexture(int textureID)
        {
            float y = textureID / Block.TextureAtlasSizeInBlocks;
            float x = textureID - (y * Block.TextureAtlasSizeInBlocks);
            x *= Block.NormalizedBlockTextureSize;
            y *= Block.NormalizedBlockTextureSize;

            y = 1f - y - Block.NormalizedBlockTextureSize;

            uvs.Add(new Vector2(x, y));
            uvs.Add(new Vector2(x, y + Block.NormalizedBlockTextureSize));
            uvs.Add(new Vector2(x + Block.NormalizedBlockTextureSize, y));
            uvs.Add(new Vector2(x + Block.NormalizedBlockTextureSize, y + Block.NormalizedBlockTextureSize));
        }
    }
    public class ChunkCoord
    {
        public int x;
        public int y;
        public int z;
        public ChunkCoord(int _x, int _y, int _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
        public bool Equals(ChunkCoord other)
        {
            if (other == null)
            {
                return false;
            }
            else if (other.x == x && other.y == y && other.z == z)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
