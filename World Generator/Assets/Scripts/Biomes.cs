using System.Collections;
using System.Collections.Generic;
using SimplexNoise;
using UnityEngine;
namespace WorldGenerator
{
    public class Biomes
    {
        public static float Get2DSimplex(Vector2 position, float offset, float scale)
        {
            return Noise.Generate((position.x + 0.1f) / Chunk.chunkSize * scale + offset, (position.y + 0.1f) / Chunk.chunkSize * scale + offset);
        }
    }
}