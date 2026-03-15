using UnityEngine;

namespace Lifey
{
    [CreateAssetMenu(fileName = "NewBlock", menuName = "Voxel Engine/Block Data")]
    public class BlockData : ScriptableObject
    {
        [Header("Basic Info")]
        public string blockName;
        public byte blockID; // We use a byte (0-255) to save memory in massive arrays

        [Header("Properties")]
        public bool isSolid = true;
        public bool isTransparent = false;

        [Header("Textures (Atlas IDs)")]
        public int textureTop;
        public int textureSide;
        public int textureBottom;

        // Helper method to get the right texture based on the face being drawn
        // 0=Top, 1=Bottom, 2=Right, 3=Left, 4=Forward, 5=Back
        public int GetTextureID(int faceIndex)
        {
            if (faceIndex == 0) return textureTop;
            if (faceIndex == 1) return textureBottom;
            return textureSide;
        }
    }
}
