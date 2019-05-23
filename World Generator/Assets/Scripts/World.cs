using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace WorldGenerator
{
    public class World : MonoBehaviour
    {
        public Material material;
        public BlockType[] blockType;
        public void Start()
        {
            world = GameObject.Find("World").GetComponent<World>();

        }
    }
}
