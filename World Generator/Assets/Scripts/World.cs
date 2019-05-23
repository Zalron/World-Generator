﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace WorldGenerator
{
    public class World : MonoBehaviour
    {
        public int seed;
        public Transform Player;
        public Vector3 spawnPosition;

        public Material material;
        public BlockType[] blockType;

        Chunk[,,] chunks = new Chunk[WorldSizeInChunks, WorldSizeInChunks, WorldSizeInChunks];

        List<ChunkCoord> activeChunks = new List<ChunkCoord>();
        ChunkCoord playerChunkCoord;
        ChunkCoord playerLastChunkCoord;

        public static readonly int ViewDistanceInChunks = 5;
        public static readonly int WorldSizeInChunks = 16;
        public static int WorldSizeInBlocks
        {
            get { return WorldSizeInChunks * Chunk.chunkSize; }
        }
        public void Start()
        {
            Random.InitState(seed);
            spawnPosition = new Vector3((WorldSizeInChunks * Chunk.chunkSize) / 2f, (WorldSizeInChunks * Chunk.chunkSize) / 2f, (WorldSizeInChunks * Chunk.chunkSize) / 2f);
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
        }
        ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
        {
            int x = Mathf.FloorToInt(pos.x / Chunk.chunkSize);
            int y = Mathf.FloorToInt(pos.y / Chunk.chunkSize);
            int z = Mathf.FloorToInt(pos.z / Chunk.chunkSize);
            return new ChunkCoord(x,y,z);
        }
        void CheckViewDistance()
        {
            ChunkCoord coord = GetChunkCoordFromVector3(Player.position);

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
                                CreateNewChunk(x, y, z);
                            }
                            else if(!chunks[x,y,z].IsActive)
                            {
                                chunks[x, y, z].IsActive = true;
                                activeChunks.Add(new ChunkCoord(x, y, z));
                            }
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
                chunks[c.x, c.y, c.z].IsActive = true;
            } 
        }
        void GenerateWorld()
        {
            for (int x = 0; x < WorldSizeInChunks; x++)
            {
                for (int y = 0; y < WorldSizeInChunks; y++)
                {
                    for (int z = 0; z <WorldSizeInChunks; z++)
                    {
                        CreateNewChunk(x, y, z);
                    }
                }
            }
            Player.position = spawnPosition;
        }
        void CreateNewChunk(int x, int y, int z)
        {
            chunks[x, y, z] = new Chunk(new ChunkCoord(x, y, z), this);
            activeChunks.Add(new ChunkCoord(x, y, z));
        }
        public byte GetBlock(Vector3 pos)
        {
            // IMMUTABLE PASS
            int yPos = Mathf.FloorToInt(pos.y);
            if (!IsBlockInWorld(pos))
            {
                return 0;
            }

            if (yPos == 0)
            {
                return 1;
            }

            // BASIC TERRAIN PASS
            int terrainHeight = Mathf.FloorToInt(WorldSizeInBlocks * Biomes.Get2DSimplex(new Vector2(pos.x, pos.z), 500, 0.25f));
            if (yPos == terrainHeight)
            {
                return 3;
            }
            else if (yPos>terrainHeight)
            {
                return 0;
            }
            else
            {
                return 2;
            }
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
}
