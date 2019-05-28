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

        public int solidGroundHeight;
        public int terrainHeightFromSoild;
        public float terrainOffset;
        public float terrainOctaves;
        public float terrainSmooth;
        public float terrainScale;
        public float terrainPersistance;
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