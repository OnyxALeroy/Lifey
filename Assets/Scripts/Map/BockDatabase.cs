using UnityEngine;

namespace Lifey
{
    public class BlockDatabase : MonoBehaviour
    {
        // Singleton pattern
        public static BlockDatabase Instance { get; private set; }

        public BlockData[] blocks;
        private void Awake()
        {
            // Set up the Singleton
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static BlockData GetBlock(int blockID)
        {
            if (blockID < 0 || blockID >= Instance.blocks.Length)
            {
                return Instance.blocks[0]; // Return Air (or an "Error" block) if out of bounds
            }
            return Instance.blocks[blockID];
        }
    }
}
