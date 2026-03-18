using System.Collections.Generic;
using UnityEngine;

namespace Lifey
{
    public class GameManager : MonoBehaviour
    {
        [Header("Map Settings")]
        [SerializeField] private int mapSizeX;
        [SerializeField] private int mapSizeY;
        [SerializeField] private int mapSizeZ;

        [Space(10)]

        [Header("References")]
        [SerializeField] private GameObject world;
        [SerializeField] private GameObject chunkPrefab;

        // ----------------------------------------------------------------------------------------

        private VoxelChunk[,,] chunks;

        private void Start()
        {
            chunks = new VoxelChunk[mapSizeX, mapSizeY, mapSizeZ];

            for (int i = 0; i < mapSizeX; i++)
            {
                for (int j = 0; j < mapSizeY; j++)
                {
                    for (int k = 0; k < mapSizeZ; k++)
                    {
                        // FIXME: temporary solid 16x16 floor that is 3 blocks thick
                        int[,,] myCustomData = new int[16, 16, 16];
                        for (int x = 0; x < 16; x++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                for (int y = 0; y < 3; y++)
                                {
                                    myCustomData[x, y, z] = 1;
                                }
                            }
                        }

                        Vector3Int worldPos = new Vector3Int(
                            i * WorldManager.Instance.ChunkSize,
                            j * WorldManager.Instance.ChunkHeight,
                            k * WorldManager.Instance.ChunkSize
                        );
                        GameObject instance = Instantiate(chunkPrefab, worldPos, Quaternion.identity, transform);
                        VoxelChunk vc = instance.GetComponent<VoxelChunk>();
                        vc.width = WorldManager.Instance.ChunkSize;
                        vc.height = WorldManager.Instance.ChunkHeight;
                        vc.depth = WorldManager.Instance.ChunkSize;

                        vc.Initialize(worldPos, myCustomData);
                        chunks[i, j, k] = vc;
                    }
                }
            }


            for (int i = 0; i < mapSizeX; i++)
            {
                for (int j = 0; j < mapSizeY; j++)
                {
                    for (int k = 0; k < mapSizeZ; k++)
                    {
                        chunks[i, j, k].RegenerateMesh();
                    }
                }
            }
        }
    }
}
