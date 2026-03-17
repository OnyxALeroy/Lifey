using System.Collections.Generic;
using UnityEngine;

namespace Lifey
{
    public class WorldManager : MonoBehaviour
    {
        [SerializeField, Range(4, 32)] private int chunkSize = 16;
        [SerializeField, Range(4, 32)] private int chunkHeight = 16;
        public int ChunkSize => chunkSize;
        public int ChunkHeight => chunkHeight;

        public static WorldManager Instance { get; private set; }

        // A dictionary to store all our generated chunks by their world position
        private Dictionary<Vector3Int, VoxelChunk> chunks = new Dictionary<Vector3Int, VoxelChunk>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        // The chunk calls this to register itself when it spawns
        public void AddChunk(Vector3Int position, VoxelChunk chunk)
        {
            chunks.TryAdd(position, chunk);
        }

        // Chunks ask this to find out what blocks are outside their borders
        public int GetBlockAtGlobalPosition(Vector3Int globalPos)
        {
            // 1. Figure out which chunk this global position belongs to
            int chunkX = Mathf.FloorToInt((float)globalPos.x / chunkSize) * chunkSize;
            int chunkY = Mathf.FloorToInt((float)globalPos.y / chunkHeight) * chunkHeight;
            int chunkZ = Mathf.FloorToInt((float)globalPos.z / chunkSize) * chunkSize;
            Vector3Int chunkPos = new Vector3Int(chunkX, chunkY, chunkZ);

            // 2. Check if that chunk actually exists in the world
            if (chunks.TryGetValue(chunkPos, out VoxelChunk chunk))
            {
                // 3. Convert the global position back into local chunk coordinates (0-15)
                int localX = globalPos.x - chunkX;
                int localY = globalPos.y - chunkY;
                int localZ = globalPos.z - chunkZ;

                // 4. Return the specific block from that neighbor chunk
                return chunk.GetLocalBlock(localX, localY, localZ);
            }

            // If the chunk doesn't exist (it hasn't loaded yet, or it's the edge of the map), pretend it's Air.
            return 0;
        }
    }
}
