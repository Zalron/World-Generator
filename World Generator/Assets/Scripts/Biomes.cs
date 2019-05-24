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
        public int terrainHeight;
        public float terrainScale;
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
        public float threshold;
        public float noiseOffset;
    }
}