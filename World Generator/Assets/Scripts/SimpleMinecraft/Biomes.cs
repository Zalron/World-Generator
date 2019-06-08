using System.Collections;
using System.Collections.Generic;
using SimplexNoise;
using UnityEngine;
namespace WorldGenerator
{
    [CreateAssetMenu(fileName = "Biomes", menuName = "ScriptableObject/Biomes", order = 1)]
    public class Biomes : ScriptableObject
    {
        public string biomeName;
        [Header("Terrian Height")]
        public int solidGroundHeight;
        public int terrainHeightFromSoild;
        [Header("Terrian Settings")]
        public float terrainOffset;
        public int terrainOctaves;
        public float terrainSmooth;
        public float terrainScale;
        public float terrainPersistance;
        [Header("Trees")]
        public float treeZoneScale = 1.3f;
        [Range(0.1f,1f)]
        public float treeZoneThreshold = 0.6f;
        public float treePlacementScale = 15f;
        [Range(0.1f, 1f)]
        public float treeZonePlacementThreshold = 0.6f;

        public int maxTreeHeight = 12;
        public int minTreeHeight = 4;
        [Header("Blocks")]
        public Lode[] lodes;
    }
    [System.Serializable]
    public class Lode
    {
        public BlockType blockType;
        public byte BlockID;
        public int minHeight;
        public int maxHeight;
        public float scale;
        public float octaves;
        public float persistance;
        public float threshold;
        public float offset;
    }
}