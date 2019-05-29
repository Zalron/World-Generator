using System.Collections;
using System.Collections.Generic;
using SimplexNoise;
using UnityEngine;
namespace WorldGenerator
{
    public class Terrian
    {
        public static float TreeGeneration(Vector2 position, float offset, float scale)
        {
            return Noise.Generate((position.x + 0.1f)/ Chunk.chunkSize * scale + offset, (position.y + 0.1f) / Chunk.chunkSize * scale + offset);
        }
        public static int GenerateHeight(Vector2 pos, int terrianGround, int terrianHeight, float offset, float smooth, float octaves, float scale) // generates the stone height map using fractal brownian motion
        {
            float height = Map(terrianGround, terrianHeight, 0, 1, FBM(pos.x * smooth, pos.y * smooth, offset, (int)octaves, scale));
            return Mathf.FloorToInt(height);
        }
        static float Map(float newmin, float newmax, float originalmin, float originalmax, float value) //generates a map for the fractal brownian motion to map on to
        {
            return Mathf.Lerp(newmin, newmax, Mathf.InverseLerp(originalmin, originalmax, value));
        }
        static float FBM(float x, float z, float offset, int octaves, float pers) // creates fractal brownian motion with PerlinNoise
        {
            float total = 0;
            float frequency = 1;
            float amplitude = 1;
            float maxValue = 0;
            for (int i = 0; i < octaves; i++)
            {
                total += Noise.Generate((x + offset) * frequency, (z + offset) * frequency) * amplitude;
                maxValue += amplitude;
                amplitude *= pers;
                frequency *= 2;
            }
            return total / maxValue;
        }
        public static bool FBM3D(float x, float y, float z, float offset, float smooth, int octaves, float pers, float threshold) // creates a 3D version of a fractal brownian motion with PerlinNoise
        {
            float XY = FBM(x * smooth, y * smooth, offset, octaves, pers);
            float YZ = FBM(y * smooth, z * smooth, offset, octaves, pers);
            float XZ = FBM(x * smooth, z * smooth, offset, octaves, pers);
            float YX = FBM(y * smooth, x * smooth, offset, octaves, pers);
            float ZY = FBM(z * smooth, y * smooth, offset, octaves, pers);
            float ZX = FBM(z * smooth, x * smooth, offset, octaves, pers);
            if ((XY + YZ + XZ + YX + ZY + ZX) / 6.0f > threshold)
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
