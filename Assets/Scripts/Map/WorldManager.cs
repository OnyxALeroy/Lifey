    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    namespace Lifey
    {
        public enum WorldType
        {
            SuperFlat
        }

        public class WorldManager : MonoBehaviour
        {
            [Header("Chunks")]
            [SerializeField] private GameObject chunkPrefab;
            [SerializeField, Range(4, 32)] private int chunkSize = 16;
            [SerializeField, Range(4, 32)] private int chunkHeight = 16;
            public int ChunkSize => chunkSize;
            public int ChunkHeight => chunkHeight;

            // A dictionary to store all our generated chunks by their world position
            private Dictionary<Vector3Int, VoxelChunk> chunks = new Dictionary<Vector3Int, VoxelChunk>();

            // ----------------------------------------------------------------------------------------

            // Singleton Pattern
            public static WorldManager Instance { get; private set; }
            private void Awake()
            {
                if (Instance == null) Instance = this;
                else Destroy(gameObject);
            }

            // ----------------------------------------------------------------------------------------

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


            // ----------------------------------------------------------------------------------------

            public VoxelChunk[,,] GenerateDefaultWorld(Vector3Int mapSize, WorldType worldType)
            {
                return worldType switch
                {
                    WorldType.SuperFlat => GenerateDefaultSuperFlatWorld(mapSize),
                    _ => throw new InvalidOperationException($"{worldType} not supported.")
                };
            }

            private VoxelChunk[,,] GenerateDefaultSuperFlatWorld(Vector3Int mapSize)
            {
                VoxelChunk[,,] res = new VoxelChunk[mapSize.x, mapSize.y, mapSize.z];
                for (int i = 0; i < mapSize.x; i++)
                {
                    for (int j = 0; j < mapSize.y; j++)
                    {
                        for (int k = 0; k < mapSize.z; k++)
                        {
                            int[,,] data = new int[chunkSize, chunkHeight, chunkSize];
                            if (j == 0)
                            {
                                for (int x = 0; x < chunkSize; x++)
                                {
                                    for (int z = 0; z < chunkSize; z++)
                                    {
                                        for (int y = 0; y < 3; y++)
                                        {
                                            data[x, y, z] = 1;
                                        }
                                    }
                                }
                            }

                            Vector3Int worldPos = new Vector3Int(
                                i * chunkSize,
                                j * chunkHeight,
                                k * chunkSize
                            );
                            GameObject instance = Instantiate(chunkPrefab, worldPos, Quaternion.identity, transform);
                            VoxelChunk vc = instance.GetComponent<VoxelChunk>();
                            vc.width = chunkSize;
                            vc.height = chunkHeight;
                            vc.depth = chunkSize;

                            vc.Initialize(worldPos, data);
                            res[i, j, k] = vc;
                        }
                    }
                }

                return res;
            }
        }
    }
