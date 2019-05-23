using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace WorldGenerator
{
    public static class Block
    {
        public static readonly int TextureAtlasSizeInBlocks = 16;
        public static float NormalizedBlockTextureSize
        {
            get { return 1f / (float)TextureAtlasSizeInBlocks; }
        }
        public static readonly Vector3[] Verts = new Vector3[8]
        {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f)
        };
        public static readonly Vector3[] faceChecks = new Vector3[6]
        {
         new Vector3(0.0f, 0.0f, -1.0f), // Checks Back Face
         new Vector3(0.0f, 0.0f, 1.0f), // Checks Front Face
         new Vector3(0.0f, 1.0f, 0.0f), // Checks Top Face
         new Vector3(0.0f, -1.0f, 0.0f), // Checks Bottom Face
         new Vector3(-1.0f, 0.0f, 0.0f), // Checks Left Face
         new Vector3(1.0f, 0.0f, 0.0f), // Checks Right Face
        };
        public static readonly int[,] Tris = new int[6, 4]
        {
        {0,3,1,2}, // Back Face
        {5,6,4,7}, // Front Face
        {3,7,2,6}, // Top Face
        {1,5,0,4}, // Bottom Face
        {4,7,0,3}, // Left Face
        {1,2,5,6}  // Right Face
        };
        public static readonly Vector3[] UVs = new Vector3[4]
        {
            new Vector3(0.0f, 0.0f),
            new Vector3(1.0f, 0.0f),
            new Vector3(0.0f, 1.0f),
            new Vector3(1.0f, 1.0f)
        };
    }
}
