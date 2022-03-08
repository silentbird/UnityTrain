using UnityEditor;
using UnityEngine;
namespace YuoTools
{
    public class TextureToSprite : Editor
    {
        static Object[] selects;
        [MenuItem("Tools/通用工具/切换选中图片为精灵 &p")]
        static void EditTexture()
        {
            selects = Selection.objects;
            foreach (var item in selects)
            {
                if (item && item is Texture)
                {
                    string path = AssetDatabase.GetAssetPath(item);
                    TextureImporter texture = AssetImporter.GetAtPath(path) as TextureImporter;
                    texture.textureType = TextureImporterType.Sprite;
                    texture.alphaIsTransparency = true;
                    texture.spritePixelsPerUnit = 1;
                    texture.spriteImportMode = SpriteImportMode.Single;
                    texture.filterMode = FilterMode.Trilinear;
                    texture.mipmapEnabled = false;
                }
            }
        }
    }

}