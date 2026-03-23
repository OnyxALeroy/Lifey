using System.Collections.Generic;
using UnityEngine;

namespace Lifey
{
    public static class IsometricIconGenerator
    {
        private static readonly Vector3[][] faceVertices = {
            new Vector3[] { new Vector3(0,1,0), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0) }, // Top -> 0
            new Vector3[] { new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1) }, // Bottom -> 1
            new Vector3[] { new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(1,0,1) }, // Right -> 2
            new Vector3[] { new Vector3(0,0,0), new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0) }, // Left -> 3
            new Vector3[] { new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1) }, // Forward -> 4
            new Vector3[] { new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,0,0) }  // Back -> 5
        };

        public static Sprite GenerateBlockIcon(BlockData blockData, Material voxelMaterial, int resolution = 256)
        {
            // 1. Move the "Studio" far away from your actual game world terrain
            Vector3 studioOffset = new Vector3(10000f, 10000f, 10000f);

            // 2. Setup Off-screen Camera
            GameObject camGo = new GameObject("IconCamera");
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 1.15f; // Zoomed out slightly to prevent edge clipping
            cam.aspect = 1f; // CRUCIAL: Forces the camera to be a perfect square
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0);

            // Position camera looking at the center of the block
            cam.transform.position = studioOffset + new Vector3(10.5f, 10.5f, -9.5f);
            cam.transform.LookAt(studioOffset + new Vector3(0.5f, 0.5f, 0.5f));

            // 3. Add a temporary Light so the block has 3D depth and edges
            GameObject lightGo = new GameObject("IconLight");
            Light dirLight = lightGo.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            // Angle the light so the top is brightest and sides have shadows
            dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 4. Generate the Mesh
            GameObject blockGo = new GameObject("TempSingleBlock");
            blockGo.transform.position = studioOffset; // Move block to the studio

            MeshFilter meshFilter = blockGo.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = blockGo.AddComponent<MeshRenderer>();

            meshRenderer.sharedMaterial = voxelMaterial;
            meshFilter.mesh = CreateSingleBlockMesh(blockData);

            // 5. Render to RenderTexture
            RenderTexture rt = new RenderTexture(resolution, resolution, 24);
            cam.targetTexture = rt;

            Texture2D finalTexture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);

            cam.Render();
            RenderTexture.active = rt;
            finalTexture.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
            finalTexture.Apply();

            // 6. Convert to Sprite
            Sprite finalSprite = Sprite.Create(
                finalTexture,
                new Rect(0, 0, resolution, resolution),
                new Vector2(0.5f, 0.5f)
            );

            // 7. Cleanup everything
            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(camGo);
            Object.DestroyImmediate(lightGo); // Don't forget to delete the light!
            Object.DestroyImmediate(blockGo);

            return finalSprite;
        }

        private static Mesh CreateSingleBlockMesh(BlockData blockData)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> uvs = new List<Vector3>();

            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
                int vertexCount = vertices.Count;

                for (int i = 0; i < 4; i++)
                {
                    vertices.Add(faceVertices[faceIndex][i]);
                }

                int textureID = blockData.GetTextureID(faceIndex);

                uvs.Add(new Vector3(0, 0, textureID));
                uvs.Add(new Vector3(0, 1, textureID));
                uvs.Add(new Vector3(1, 1, textureID));
                uvs.Add(new Vector3(1, 0, textureID));

                triangles.Add(vertexCount);
                triangles.Add(vertexCount + 1);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount);
                triangles.Add(vertexCount + 2);
                triangles.Add(vertexCount + 3);
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}
