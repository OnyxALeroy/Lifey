using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Lifey.EditorTools
{
    public class TextureArrayGenerator
    {
        // This adds a button to the top toolbar in Unity!
        [MenuItem("Voxel Engine/1. Build Texture Array")]
        public static void BuildArray()
        {
            // The folder where you keep your individual PNGs
            string folderPath = "Assets/Textures/Blocks";

            // Check if the folder exists
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError($"Cannot find folder: {folderPath}. Please create it and add your PNGs.");
                return;
            }

            // 1. Find all textures in that folder and sort them alphabetically
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
            if (guids.Length == 0)
            {
                Debug.LogWarning("No textures found in the Blocks folder!");
                return;
            }

            var paths = guids.Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToArray();

            // 2. Load the first texture to get our baseline resolution (e.g., 16x16)
            Texture2D firstTex = AssetDatabase.LoadAssetAtPath<Texture2D>(paths[0]);
            int width = firstTex.width;
            int height = firstTex.height;

            // Create the empty Texture2DArray in memory
            Texture2DArray textureArray = new Texture2DArray(width, height, paths.Length, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Repeat
            };

            // 3. Loop through all PNGs, fix their settings, and copy them into the array
            for (int i = 0; i < paths.Length; i++)
            {
                // Force Unity to make the texture readable and uncompressed automatically
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(paths[i]);
                if (importer != null && (!importer.isReadable || importer.textureCompression != TextureImporterCompression.Uncompressed || importer.filterMode != FilterMode.Point))
                {
                    importer.isReadable = true;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.filterMode = FilterMode.Point;
                    importer.SaveAndReimport();
                }

                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(paths[i]);

                // Safety check to ensure you didn't accidentally put a 32x32 image in a 16x16 folder
                if (tex.width != width || tex.height != height)
                {
                    Debug.LogError($"Skipping {tex.name}! It is {tex.width}x{tex.height} but should be {width}x{height}.");
                    continue;
                }

                // Copy the pixel data into the array at index 'i'
                textureArray.SetPixels(tex.GetPixels(0), i, 0);
            }

            textureArray.Apply();

            // 4. Save the generated array as a real asset file in your project
            string savePath = "Assets/Textures/BlockTextureArray.asset";
            AssetDatabase.CreateAsset(textureArray, savePath);

            Debug.Log($"<color=green><b>SUCCESS!</b></color> Built Texture Array with {paths.Length} textures at {savePath}.");
        }
    }
}
