using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<int> transparentTriangles = new List<int>();
        Material[] materials = new Material[2];
        List<Vector2> uvs = new List<Vector2>();

        public Vector3 position;

        public byte[,,] blockMap = new byte[chunkSize, chunkSize, chunkSize];

        public Queue<BlockMod> modifications = new Queue<BlockMod>();

        World world;

        private bool _isActive;
        private bool IsBlockMapPopulated = false;
        public bool threadLocked = false;
        public Chunk(ChunkCoord _coord, World _world, bool generateOnLoad)
        {
            coord = _coord;
            world = _world;
            IsActive = true;
            if (generateOnLoad)
            {
                Init();
            }
        }
        public void Init()
        {
            chunkObject = new GameObject();
            meshFilter = chunkObject.AddComponent<MeshFilter>();
            meshRenderer = chunkObject.AddComponent<MeshRenderer>();
            materials[0] = world.material;
            materials[1] = world.transparentMaterial;
            meshRenderer.materials = materials;

            chunkObject.transform.SetParent(world.transform);
            chunkObject.transform.position = new Vector3(coord.x * chunkSize, coord.y * chunkSize, coord.z * chunkSize);
            chunkObject.name = "Chunk " + coord.x + "x, " + coord.y + "y, " + coord.z + "z";
            position = chunkObject.transform.position;

            Thread PopulateBlockMapThread = new Thread(new ThreadStart(PopulateBlockMap));
            PopulateBlockMapThread.Start();
            
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
            _updateChunk();
            IsBlockMapPopulated = true;
        }
        public void UpdateChunk()
        {
            Thread ChunkThread = new Thread(new ThreadStart(_updateChunk));
            ChunkThread.Start();
        }
        private void _updateChunk()
        {
            threadLocked = true;
            while (modifications.Count > 0)
            {
                BlockMod m = modifications.Dequeue();
                Vector3 pos = m.position -= position;
                blockMap[(int)pos.x, (int)pos.y, (int)pos.z] = m.id;
            }
            ClearMeshData();
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        if (world.blockType[blockMap[x, y, z]].isSolid)
                        {
                            UpdateMeshData(new Vector3(x, y, z));
                        }
                    }
                }
            }
            lock (world.chunksToDraw)
            {
                world.chunksToDraw.Enqueue(this);
            }

            threadLocked = false;
        }
        void ClearMeshData()
        {
            vertexIndex = 0;
            vertices.Clear();
            triangles.Clear();
            transparentTriangles.Clear();
            uvs.Clear();
        }
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                if (chunkObject != null)
                {
                    chunkObject.SetActive(value);
                }
            }
        }
        public bool isEditable
        {
            get
            {
                if (!IsBlockMapPopulated || threadLocked)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        bool IsBlockInChunk(int x, int y, int z)
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
        public void EditBlock(Vector3 pos, byte newID)
        {
            int xCheck = Mathf.FloorToInt(pos.x);
            int yCheck = Mathf.FloorToInt(pos.y);
            int zCheck = Mathf.FloorToInt(pos.z);

            xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
            yCheck -= Mathf.FloorToInt(chunkObject.transform.position.y);
            zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

            blockMap[xCheck, yCheck, zCheck] = newID;

            UpdateSurroundingBlocks(xCheck, yCheck, zCheck);

            _updateChunk();
        }
        void UpdateSurroundingBlocks(int x, int y, int z)
        {
            Vector3 thisBlock = new Vector3(x, y, z);
            for (int p = 0; p < 6; p++)
            {
                Vector3 currentBlock = thisBlock + Block.faceChecks[p];
                if (!IsBlockInChunk((int)currentBlock.x, (int)currentBlock.y, (int)currentBlock.z))
                {
                    world.GetChunkFromVector3(currentBlock + position).UpdateChunk();
                }
            }
        }
        bool CheckBlock(Vector3 pos)
        {
            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);
            int z = Mathf.FloorToInt(pos.z);
            if (!IsBlockInChunk(x, y, z))
            {
                return world.CheckForTransparentBlockInChunk(pos + position);
            }
            return world.blockType[blockMap[x, y, z]].IsTransparent;
        }
        public byte GetBlockFromGlobalVector3(Vector3 pos)
        {
            int xCheck = Mathf.FloorToInt(pos.x);
            int yCheck = Mathf.FloorToInt(pos.y);
            int zCheck = Mathf.FloorToInt(pos.z);

            xCheck -= Mathf.FloorToInt(position.x);
            yCheck -= Mathf.FloorToInt(position.y);
            zCheck -= Mathf.FloorToInt(position.z);

            return blockMap[xCheck, yCheck, zCheck];
        }
        public byte GetBlockID(Vector3 pos)
        {
            byte blockID = 0;
            int xCheck = Mathf.FloorToInt(pos.x);
            int yCheck = Mathf.FloorToInt(pos.y);
            int zCheck = Mathf.FloorToInt(pos.z);

            xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
            yCheck -= Mathf.FloorToInt(chunkObject.transform.position.y);
            zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);
            blockMap[xCheck, yCheck, zCheck] = blockID;
            return blockID;
        }
        void UpdateMeshData(Vector3 pos)
        {
            byte blockID = blockMap[(int)pos.x, (int)pos.y, (int)pos.z];
            bool isTransparent = world.blockType[blockID].IsTransparent;
            for (int p = 0; p < 6; p++)
            {
                if (CheckBlock(pos + Block.faceChecks[p]))
                {
                    vertices.Add(pos + Block.Verts[Block.Tris[p, 0]]);
                    vertices.Add(pos + Block.Verts[Block.Tris[p, 1]]);
                    vertices.Add(pos + Block.Verts[Block.Tris[p, 2]]);
                    vertices.Add(pos + Block.Verts[Block.Tris[p, 3]]);
                    AddTexture(world.blockType[blockID].GetTextureID(p));
                    if (!isTransparent)
                    {
                        triangles.Add(vertexIndex);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + 2);
                        triangles.Add(vertexIndex + 2);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + 3);
                    }
                    else
                    {
                        transparentTriangles.Add(vertexIndex);
                        transparentTriangles.Add(vertexIndex + 1);
                        transparentTriangles.Add(vertexIndex + 2);
                        transparentTriangles.Add(vertexIndex + 2);
                        transparentTriangles.Add(vertexIndex + 1);
                        transparentTriangles.Add(vertexIndex + 3);
                    }
                    vertexIndex += 4;
                }
            }
        }
        public void CreateMesh()
        {
            Mesh mesh = new Mesh();
            {
                mesh.vertices = vertices.ToArray();
                mesh.subMeshCount = 2;
                mesh.SetTriangles(triangles.ToArray(), 0);
                mesh.SetTriangles(transparentTriangles.ToArray(), 1);
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

        public ChunkCoord()
        {
            x = 0;
            y = 0;
            z = 0;
        }
        public ChunkCoord(int _x, int _y, int _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
        public ChunkCoord(Vector3 pos)
        {
            int xCheck = Mathf.FloorToInt(pos.x);
            int yCheck = Mathf.FloorToInt(pos.y);
            int zCheck = Mathf.FloorToInt(pos.z);

            x = xCheck / Chunk.chunkSize;
            y = yCheck / Chunk.chunkSize;
            z = zCheck / Chunk.chunkSize;
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
