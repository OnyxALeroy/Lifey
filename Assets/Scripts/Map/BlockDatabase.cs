using UnityEngine;

namespace Lifey
{
    public class BlockDatabase : MonoBehaviour
    {
        // Singleton pattern
        public static BlockDatabase Instance { get; private set; }
        public static bool IsReady { get; private set; }

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

        private void Start()
        {
            IsReady = true;
        }

        // ----------------------------------------------------------------------------------------

        public static BlockData GetBlock(int blockID)
        {
            if (!IsReady)
            {
                Debug.LogError("BlockDatabase not ready! Call Initialize() first.");
                return null;
            }
            if (blockID < 0 || blockID >= Instance.blocks.Length)
            {
                return Instance.blocks[0];
            }
            return Instance.blocks[blockID];
        }
    }
}
