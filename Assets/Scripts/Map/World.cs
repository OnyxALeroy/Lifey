using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField, Tooltip("Width and Length in chunks")] private int mapSize = 4;

    [Space(10)]

    [Header("Chunk Settings")]
    [SerializeField, Range(4, 32)] private int chunkSize = 16;
    [SerializeField, Range(4, 32)] private int chunkDepth = 16;
    [SerializeField] private GameObject chunkPrefab;

    // --------------------------------------------------------------------------------------------

    private VoxelChunk[,] chunks;

    // --------------------------------------------------------------------------------------------

    private void Start() {
        int[,,] myCustomData = new int[16, 16, 16];

        // Let's just make a solid 16x16 floor that is 3 blocks thick
        for (int x = 0; x < 16; x++) {
            for (int z = 0; z < 16; z++) {
                for (int y = 0; y < 3; y++) {
                    myCustomData[x, y, z] = 1;
                }
            }
        }

        chunks = new VoxelChunk[mapSize, mapSize];
        for (int i = 0; i < mapSize; i++) {
            for (int j = 0; j < mapSize; j++) {
                GameObject instance = Instantiate(chunkPrefab, transform);
                VoxelChunk vc = instance.GetComponent<VoxelChunk>();
                vc.width = chunkSize;
                vc.height = chunkSize;
                vc.depth = chunkDepth;
                vc.Initialize(myCustomData);
                chunks[i, j] = vc;
            }
        }
    }
}
