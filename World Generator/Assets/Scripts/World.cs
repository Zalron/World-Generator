using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.UI;
using Unity.Jobs;
namespace WorldGenerator
{
    public class World : MonoBehaviour
    {
        public Sprite icon;
        public int seed;
        public Transform Player;
        public Vector3 spawnPosition;

        public Material material;
        public Material transparentMaterial;
        public BlockType[] blockType;
        public Biomes biome;

        Chunk[,,] chunks = new Chunk[WorldSizeInChunks, WorldSizeInChunks, WorldSizeInChunks];

        List<ChunkCoord> activeChunks = new List<ChunkCoord>();
        public ChunkCoord playerChunkCoord;
        ChunkCoord playerLastChunkCoord;

        List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
        List<Chunk> chunksToUpdate = new List<Chunk>();
        public Queue<Chunk> chunksToDraw = new Queue<Chunk>();
        private bool IsCreatingChunks;

        bool applyingModifications = false;

        Queue<Queue<BlockMod>> modifications = new Queue<Queue<BlockMod>>();

        public GameObject debugScreen;

        public static readonly int ViewDistanceInChunks = 5;
        public static readonly int WorldSizeInChunks = 64;
        public static int WorldSizeInBlocks
        {
            get { return WorldSizeInChunks * Chunk.chunkSize; }
        }
        public void Start()
        {
            seed = Random.Range(1, 9999999);
            Random.InitState(seed);
            spawnPosition = new Vector3((WorldSizeInChunks * Chunk.chunkSize) / 2f, biome.solidGroundHeight + 20, (WorldSizeInChunks * Chunk.chunkSize) / 2f);
            GenerateWorld();
            playerLastChunkCoord = GetChunkCoordFromVector3(Player.position);
        }
        public void Update()
        {
            playerChunkCoord = GetChunkCoordFromVector3(Player.position);
            if (!playerChunkCoord.Equals(playerLastChunkCoord))
            {
                CheckViewDistance();
            }
            if (!applyingModifications)
            {
                ApplyModifications();
            }
            if (chunksToCreate.Count > 0)
            {
                CreateChunk();
            }
            if (chunksToUpdate.Count > 0)
            {
                UpdateChunks();
            }
            if (chunksToDraw.Count > 0)
            {
                lock (chunksToDraw)
                {
                    if (chunksToDraw.Peek().isEditable)
                    {
                        chunksToDraw.Dequeue().CreateMesh();
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.F3))
            {
                debugScreen.SetActive(!debugScreen.activeSelf);
            }
        }
        void CreateChunk()
        {

            ChunkCoord c = chunksToCreate[0];
            chunksToCreate.RemoveAt(0);
            activeChunks.Add(c);
            chunks[c.x, c.y, c.z].Init();

        }
        void UpdateChunks()
        {

            bool updated = false;
            int index = 0;

            while (!updated && index < chunksToUpdate.Count - 1)
            {

                if (chunksToUpdate[index].isEditable)
                {
                    chunksToUpdate[index].UpdateChunk();
                    chunksToUpdate.RemoveAt(index);
                    updated = true;
                }
                else
                {
                    index++;
                }
            }
        }
        ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
        {
            int x = Mathf.FloorToInt(pos.x / Chunk.chunkSize);
            int y = Mathf.FloorToInt(pos.y / Chunk.chunkSize);
            int z = Mathf.FloorToInt(pos.z / Chunk.chunkSize);
            return new ChunkCoord(x,y,z);
        }
        public Chunk GetChunkFromVector3(Vector3 pos)
        {
            int x = Mathf.FloorToInt(pos.x / Chunk.chunkSize);
            int y = Mathf.FloorToInt(pos.y / Chunk.chunkSize);
            int z = Mathf.FloorToInt(pos.z / Chunk.chunkSize);
            return chunks[x, y, z];
        }
        void CheckViewDistance()
        {
            ChunkCoord coord = GetChunkCoordFromVector3(Player.position);
            playerLastChunkCoord = playerChunkCoord;

            List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

            for (int x = coord.x - ViewDistanceInChunks; x < coord.x + ViewDistanceInChunks; x++)
            {
                for (int y = coord.y - ViewDistanceInChunks; y < coord.y + ViewDistanceInChunks; y++)
                {
                    for (int z = coord.z - ViewDistanceInChunks; z < coord.z + ViewDistanceInChunks; z++)
                    {
                        if (IsChunkInWorld(new ChunkCoord(x,y,z)))
                        {
                            if (chunks[x, y, z] == null)
                            {
                                chunks[x, y, z] = new Chunk(new ChunkCoord(x, y, z), this, false);
                                chunksToCreate.Add(new ChunkCoord(x,y,z));
                            }
                            else if(!chunks[x,y,z].IsActive)
                            {
                                chunks[x, y, z].IsActive = true;
                            }
                            activeChunks.Add(new ChunkCoord(x, y, z));
                        }
                        for (int i = 0; i < previouslyActiveChunks.Count; i++)
                        {
                            if (previouslyActiveChunks[i].Equals(new ChunkCoord(x,y,z)))
                            {
                                previouslyActiveChunks.RemoveAt(i);
                            }
                        }
                    }
                }
            }
            foreach (ChunkCoord c in previouslyActiveChunks)
            {
                chunks[c.x, c.y, c.z].IsActive = false;
            } 
        }
        void GenerateWorld()
        {
            Player.position = spawnPosition;
            for (int x = (WorldSizeInChunks/2) - ViewDistanceInChunks; x < (WorldSizeInChunks / 2) + ViewDistanceInChunks; x++)
            {
                for (int y = (WorldSizeInChunks / 2) - ViewDistanceInChunks; y < (WorldSizeInChunks / 2) + ViewDistanceInChunks; y++)
                {
                    for (int z = (WorldSizeInChunks / 2) - ViewDistanceInChunks; z < (WorldSizeInChunks / 2) + ViewDistanceInChunks; z++)
                    {
                        chunks[x, y, z] = new Chunk(new ChunkCoord(x,y,z), this, true);
                        activeChunks.Add(new ChunkCoord(x, y, z));
                    }
                }
            }
        }
        void ApplyModifications()
        {
            applyingModifications = true;
            while (modifications.Count > 0)
            {
                Queue<BlockMod> queue = modifications.Dequeue();
                while (queue.Count > 0)
                {
                    BlockMod v = queue.Dequeue();
                    ChunkCoord c = GetChunkCoordFromVector3(v.position);
                    if (chunks[c.x, c.y, c.z] == null)
                    {
                        chunks[c.x, c.y, c.z] = new Chunk(c, this, true);
                        activeChunks.Add(c);
                    }
                    chunks[c.x, c.y, c.z].modifications.Enqueue(v);
                    if (!chunksToUpdate.Contains(chunks[c.x, c.y, c.z]))
                    {
                        chunksToUpdate.Add(chunks[c.x, c.y, c.z]);
                    }
                }

            }
            applyingModifications = false;
        }
        public bool CheckForSolidBlockInChunk(Vector3 pos)
        {
            ChunkCoord thisChunk = new ChunkCoord(pos);

            if (!IsBlockInWorld(pos))
            {
                return false;
            }
            if (chunks[thisChunk.x, thisChunk.y, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.y, thisChunk.z].isEditable)
            {
                return blockType[chunks[thisChunk.x, thisChunk.y, thisChunk.z].GetBlockFromGlobalVector3(pos)].isSolid;
            }
            return blockType[GetBlock(pos)].isSolid;
        }
        public bool CheckForTransparentBlockInChunk(Vector3 pos)
        {
            ChunkCoord thisChunk = new ChunkCoord(pos);

            if (!IsBlockInWorld(pos))
            {
                return false;
            }
            if (chunks[thisChunk.x, thisChunk.y, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.y, thisChunk.z].isEditable)
            {
                return blockType[chunks[thisChunk.x, thisChunk.y, thisChunk.z].GetBlockFromGlobalVector3(pos)].IsTransparent;
            }
            return blockType[GetBlock(pos)].IsTransparent;
        }
        public byte GetBlock(Vector3 pos)
        {
            // IMMUTABLE PASS
            int yPos = Mathf.FloorToInt(pos.y);
            if (!IsBlockInWorld(pos))
            {
                return 0;
            }
            if (yPos <= 3)
            {
                return 1;
            }
            // BASIC TERRAIN PASS
            int terrainHeight = Terrian.GenerateHeight(new Vector2(pos.x, pos.z), biome.solidGroundHeight, biome.terrainHeightFromSoild, biome.terrainOffset, biome.terrainSmooth, biome.terrainOctaves, biome.terrainScale);
            byte blockValue;
            if (yPos == terrainHeight)
            {
                blockValue = 3; //Grass
            }
            else if (yPos <= terrainHeight && yPos > terrainHeight - 4)
            {
                blockValue = 5; // Dirt
            }
            else if (yPos > terrainHeight)
            {
                return 0; // Air
            }
            else
            {
                blockValue = 2; //Stone
            }

            // ORE TERRAIN PASS
            if (blockValue == 2)
            {
                foreach (Lode lode in biome.lodes)
                {
                    if (yPos >= lode.minHeight && yPos <= lode.maxHeight)
                    {
                        if (Terrian.FBM3D(pos.x, pos.y, pos.z, lode.offset, lode.octaves, (int)lode.persistance, lode.scale, lode.threshold))
                        {
                            blockValue = lode.BlockID;
                        }
                    }
                }
            }
            

            // TREE TERRAIN PASS
            if (yPos == terrainHeight)
            {
                if (Terrian.TreeGeneration(new Vector2(pos.x,pos.z), 0, biome.treeZoneScale) > biome.treeZoneThreshold) 
                {
                    if (Terrian.TreeGeneration(new Vector2(pos.x, pos.z), 0, biome.treePlacementScale) > biome.treeZonePlacementThreshold)
                    {
                        modifications.Enqueue(Structure.MakeTree(pos, biome.minTreeHeight, biome.maxTreeHeight));
                    }
                }
            }
            return blockValue;
        }
        bool IsChunkInWorld(ChunkCoord coord)
        {
            if (coord.x > 0 && coord.x < WorldSizeInChunks - 1 && coord.y > 0 && coord.y < WorldSizeInChunks - 1 && coord.z > 0 && coord.z < WorldSizeInChunks - 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        bool IsBlockInWorld(Vector3 pos)
        {
            if (pos.x >= 0 && pos.x < WorldSizeInBlocks && pos.y >= 0 && pos.y < WorldSizeInBlocks && pos.z >= 0 && pos.z < WorldSizeInBlocks)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public class BlockMod
    {
        public Vector3 position;
        public byte id;
        public BlockMod()
        {
            position = new Vector3();
            id = 0;
        }
        public BlockMod(Vector3 _position, byte _id)
        {
            position = _position;
            id = _id;
        }

    }
}
