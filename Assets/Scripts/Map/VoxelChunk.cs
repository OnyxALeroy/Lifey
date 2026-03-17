using System.Collections.Generic;
using UnityEngine;

namespace Lifey
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class VoxelChunk : MonoBehaviour
    {
        public int width = 16;
        public int height = 16;
        public int depth = 16;

        // How many blocks wide/tall your texture atlas is (e.g., a 4x4 grid of textures)
        private const int TextureAtlasSizeInBlocks = 4;

        private int[,,] blocks;
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<int> triangles = new List<int>();

        // NEW: List to hold our UV coordinates for texturing
        private readonly List<Vector3> uvs = new List<Vector3>();

        // Lookup table for the 6 faces of a cube.
        private readonly Vector3[][] faceVertices = {
            new Vector3[] { new Vector3(0,1,0), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0) }, // Top (+y)
            new Vector3[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1) }, // Bottom (-y)
            new Vector3[] { new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(1,0,1) }, // Right (+x)
            new Vector3[] { new Vector3(0,0,0), new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0) }, // Left (-x)
            new Vector3[] { new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1) }, // Forward (+z)
            new Vector3[] { new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,0,0) }  // Back (-z)
        };

        // Lookup table for the direction to check for neighboring blocks
        private readonly Vector3Int[] faceChecks = {
            new Vector3Int(0, 1, 0),  // Top
            new Vector3Int(0, -1, 0), // Bottom
            new Vector3Int(1, 0, 0),  // Right
            new Vector3Int(-1, 0, 0), // Left
            new Vector3Int(0, 0, 1),  // Forward
            new Vector3Int(0, 0, -1)  // Back
        };

        public void Initialize(Vector3Int pos, int[,,] incomingBlocks)
        {
            blocks = incomingBlocks;
            width = blocks.GetLength(0);
            height = blocks.GetLength(1);
            depth = blocks.GetLength(2);
            WorldManager.Instance.AddChunk(pos, this);
        }

        public int GetLocalBlock(int x, int y, int z)
        {
            if (IsBlockInBounds(x, y, z))
                return blocks[x, y, z];
            return 0;
        }

        // Change a single block and update the visual mesh
        public void SetBlock(int x, int y, int z, int blockID)
        {
            if (IsBlockInBounds(x, y, z))
            {
                blocks[x, y, z] = blockID;
                RegenerateMesh();
            }
        }

        private int GetBlockID(int x, int y, int z)
        {
            if (IsBlockInBounds(x, y, z))
            {
                return blocks[x, y, z];
            }

            // If it's outside our bounds, ask the WorldManager
            // We calculate the global position by adding the chunk's transform position to the local coordinates.
            Vector3Int globalPos = new Vector3Int(
                x + Mathf.RoundToInt(transform.position.x),
                y + Mathf.RoundToInt(transform.position.y),
                z + Mathf.RoundToInt(transform.position.z)
            );

            return WorldManager.Instance.GetBlockAtGlobalPosition(globalPos);
        }

        public void RegenerateMesh()
        {
            vertices.Clear();
            triangles.Clear();
            uvs.Clear();

            GenerateMesh();
            ApplyMesh();
        }

        private void GenerateMesh()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        int currentBlockID = blocks[x, y, z];
                        if (currentBlockID == 0) continue;

                        // It's a solid/visible block. Check all 6 directions around it.
                        for (int i = 0; i < 6; i++)
                        {
                            int neighborX = x + faceChecks[i].x;
                            int neighborY = y + faceChecks[i].y;
                            int neighborZ = z + faceChecks[i].z;

                            // Use our new smart method to get the neighbor ID, whether it's inside or outside the chunk!
                            int neighborID = GetBlockID(neighborX, neighborY, neighborZ);

                            // Draw the face if the neighbor is air OR if it's transparent
                            if (neighborID == 0 || IsTransparent(neighborID))
                            {
                                AddFace(i, new Vector3(x, y, z), currentBlockID);
                            }
                        }
                    }
                }
            }
        }

        private void AddFace(int faceIndex, Vector3 blockPosition, int blockID)
        {
            int vertexCount = vertices.Count;
            for (int i = 0; i < 4; i++)
            {
                vertices.Add(faceVertices[faceIndex][i] + blockPosition);
            }

            int textureID = GetTextureID(blockID, faceIndex);

            // X and Y are just standard 0-to-1 coordinates for a square.
            // Z is the textureID (the index in your Texture Array).
            uvs.Add(new Vector3(0, 0, textureID)); // Bottom Left
            uvs.Add(new Vector3(0, 1, textureID)); // Top Left
            uvs.Add(new Vector3(1, 1, textureID)); // Top Right
            uvs.Add(new Vector3(1, 0, textureID)); // Bottom Right

            // Add Triangles
            triangles.Add(vertexCount);
            triangles.Add(vertexCount + 1);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount);
            triangles.Add(vertexCount + 2);
            triangles.Add(vertexCount + 3);
        }

        // Helper: Check if a coordinate is inside our chunk boundaries
        bool IsBlockInBounds(int x, int y, int z)
        {
            return x >= 0 && x < width && y >= 0 && y < height && z >= 0 && z < depth;
        }

        void ApplyMesh()
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            GetComponent<MeshFilter>().mesh = mesh;
        }

        // ----------------------------------------------------------------------------------------

        private int GetTextureID(int blockID, int faceIndex)
        {
            return BlockDatabase.GetBlock(blockID).GetTextureID(faceIndex);
        }

        private bool IsTransparent(int blockID)
        {
            return BlockDatabase.GetBlock(blockID).isTransparent;
        }

        // ----------------------------------------------------------------------------------------

        // Unity automatically calls this method in the Editor
        private void OnDrawGizmos()
        {
            if (width <= 0 || height <= 0 || depth <= 0) return;

            Gizmos.color = Color.yellow;
            Vector3 size = new Vector3(width, height, depth);
            Vector3 center = transform.position + (size / 2f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
