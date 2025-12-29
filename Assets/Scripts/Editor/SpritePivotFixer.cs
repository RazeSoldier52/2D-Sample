using UnityEngine;
using UnityEditor;

public class SpritePivotFixer : Editor
{
    [MenuItem("Assets/Tools/Fix Sprite Boundaries & Pivot (Smart Crop)")]
    static void FixBoundariesAndPivot()
    {
        ProcessSprites();
    }

    static void ProcessSprites()
    {
        foreach (Object obj in Selection.objects)
        {
            if (obj is Texture2D)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null)
                {
                    // 1. Make readable to scan pixels
                    importer.isReadable = true;
                    importer.SaveAndReimport();

                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    Rect visibleRect = GetVisibleBounds(texture);

                    // 2. Prepare the Smart Crop (MetaData)
                    SpriteMetaData metaData = new SpriteMetaData();
                    metaData.name = obj.name; // Keep the name same as the file
                    metaData.rect = visibleRect;

                    // 3. Set Pivot to Bottom Center of the CROPPED area
                    // (0.5 means center of the slime, not center of the file)
                    metaData.alignment = (int)SpriteAlignment.Custom;
                    metaData.pivot = new Vector2(0.5f, 0.0f);

                    // 4. Force "Multiple" mode to allow cropping
                    importer.spriteImportMode = SpriteImportMode.Multiple;
                    importer.spritesheet = new SpriteMetaData[] { metaData };

                    // 5. Cleanup
                    importer.isReadable = false;
                    importer.SaveAndReimport();

                    Debug.Log($"Cropped {obj.name} to {visibleRect}");
                }
            }
        }
    }

    static Rect GetVisibleBounds(Texture2D tex)
    {
        int minX = tex.width, maxX = 0, minY = tex.height, maxY = 0;
        bool foundPixel = false;

        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                if (tex.GetPixel(x, y).a > 0.01f)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                    foundPixel = true;
                }
            }
        }

        if (!foundPixel) return new Rect(0, 0, tex.width, tex.height);
        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
}