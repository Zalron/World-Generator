﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace WorldGenerator
{
    public class Structure
    {
        public static Queue<BlockMod> MakeTree(Vector3 position,  int minTrunkHeight, int maxTrunkHeight)
        {
            Queue<BlockMod> queue = new Queue<BlockMod>();
            int height = (int)(maxTrunkHeight * Terrian.TreeGeneration(new Vector2(position.x, position.z), 250f, 3f));
            if (height < minTrunkHeight)
            {
                height = minTrunkHeight;
            }
            for (int i = 1; i < height; i++)
            {
                queue.Enqueue(new BlockMod(new Vector3(position.x, position.y + i, position.z), 6));
            }
            for (int x = -3; x < 4; x++)
            {
                for (int y = 0; y < 7; y++)
                {
                    for (int z = -3; z < 4; z++)
                    {
                        queue.Enqueue(new BlockMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 11));
                    }
                }
            }
            return queue;
        }
    }
}
