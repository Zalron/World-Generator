using System.Collections;
using System.Collections.Generic;
using SimplexNoise;
using UnityEngine;
namespace WorldGenerator
{
    public class Terrian
    {
        public static float Get2DSimplex(Vector2 position, float offset, float scale)
        {
            return Noise.Generate((position.x + 0.1f) / Chunk.chunkSize * scale + offset, (position.y + 0.1f) / Chunk.chunkSize * scale + offset);
        }
        public static bool Get3DSimplex(Vector3 position, float offset, float scale, float threshold)
        {
            float x = (position.x + offset + 0.1f) * scale;
            float y = (position.y + offset + 0.1f) * scale;
            float z = (position.z + offset + 0.1f) * scale;

            float AB = Noise.Generate(x, y);
            float BC = Noise.Generate(y, z);
            float AC = Noise.Generate(x, z);
            float BA = Noise.Generate(y, x);
            float CB = Noise.Generate(z, y);
            float CA = Noise.Generate(z, x);
            if ((AB + BC + AC + BA + CB + CA) / 6f > threshold)
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
