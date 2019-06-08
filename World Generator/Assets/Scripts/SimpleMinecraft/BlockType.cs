using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace WorldGenerator
{
    [CreateAssetMenu(fileName ="BlockType", menuName = "ScriptableObject/BlockType", order = 1)]
    public class BlockType : ScriptableObject
    {
        public string blockName;
        public bool isSolid;
        public bool IsTransparent;
        public Sprite icon;

        [Header("Texture Values")]
        public int backFaceTexture;
        public int frontFaceTexture;
        public int topFaceTexture;
        public int bottomFaceTexture;
        public int leftFaceTexture;
        public int rightFaceTexture;
        public int GetTextureID(int faceIndex)
        {
            switch (faceIndex)
            {
                case 0:
                    return backFaceTexture;
                case 1:
                    return frontFaceTexture;
                case 2:
                    return topFaceTexture;
                case 3:
                    return bottomFaceTexture;
                case 4:
                    return leftFaceTexture;
                case 5:
                    return rightFaceTexture;
                default:
                    Debug.Log("Error in GetTextureId: invalid face Index");
                    return 0;
            }
        }
    }
}
