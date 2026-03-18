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

        // ----------------------------------------------------------------------------------------

        private VoxelChunk[,,] chunks;

        private void Start()
        {
            Vector3Int mapSize = new Vector3Int(mapSizeX, mapSizeY, mapSizeZ);
            chunks = WorldManager.Instance.GenerateDefaultWorld(mapSize, WorldType.SuperFlat);
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
