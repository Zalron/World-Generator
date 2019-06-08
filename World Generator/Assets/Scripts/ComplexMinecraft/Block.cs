using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Block : ushort
{
    Air = 0x0000,
    Filled = 0x001
}
public static class Block_Extentions
{
    public static bool IsTransparent(this Block block)
    {
        return block == Block.Air;
    }
}
