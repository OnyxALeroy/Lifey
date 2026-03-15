using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VoxelChunk : MonoBehaviour
{
    public int width = 16;
    public int height = 16;
    public int depth = 16;

    private int[,,] blocks;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

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

    public void Initialize(int[,,] incomingBlocks)
    {
        blocks = incomingBlocks;
        width = blocks.GetLength(0);
        height = blocks.GetLength(1);
        depth = blocks.GetLength(2);

        RegenerateMesh();
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

    private void RegenerateMesh()
    {
        vertices.Clear();
        triangles.Clear();
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
                    // If it's air, skip it
                    if (blocks[x, y, z] == 0) continue;

                    // It's a solid block. Check all 6 directions around it.
                    for (int i = 0; i < 6; i++)
                    {
                        int neighborX = x + faceChecks[i].x;
                        int neighborY = y + faceChecks[i].y;
                        int neighborZ = z + faceChecks[i].z;

                        // If the neighbor is out of bounds, OR the neighbor is air (0), draw this face!
                        if (!IsBlockInBounds(neighborX, neighborY, neighborZ) || blocks[neighborX, neighborY, neighborZ] == 0)
                        {
                            AddFace(i, new Vector3(x, y, z));
                        }
                    }
                }
            }
        }
    }

    private void AddFace(int faceIndex, Vector3 blockPosition)
    {
        int vertexCount = vertices.Count;

        // Add the 4 vertices for this face, offset by the block's position in the chunk
        for (int i = 0; i < 4; i++)
        {
            vertices.Add(faceVertices[faceIndex][i] + blockPosition);
        }

        // Add the 6 indices to form the two triangles that make up the square face
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
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals(); // Crucial for lighting to work correctly!

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
