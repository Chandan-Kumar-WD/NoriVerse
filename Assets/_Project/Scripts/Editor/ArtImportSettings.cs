using UnityEditor;
using UnityEngine;

namespace NoriVerse.EditorTools
{
    /// <summary>Imports everything under Art/ as a high-quality single sprite so generated art needs no manual setup.</summary>
    public class ArtImportSettings : AssetPostprocessor
    {
        private const string ArtRoot = "Assets/_Project/Art/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtRoot)) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            ArtSpec.TryGet(assetPath, out float ppu, out Vector2 pivot);
            importer.spritePixelsPerUnit = ppu;
            importer.mipmapEnabled = false;
            importer.isReadable = assetPath.EndsWith("/PizzaFlow/sauce_disc.png");
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;
            // iOS ships compressed GPU textures; keep the CPU-painted sauce readable as RGBA.
            var ios = importer.GetPlatformTextureSettings("iPhone");
            ios.name = "iPhone";
            ios.overridden = true;
            ios.maxTextureSize = 4096;
            ios.format = importer.isReadable ? TextureImporterFormat.RGBA32 : TextureImporterFormat.ASTC_4x4;
            ios.compressionQuality = 100;
            ios.textureCompression = importer.isReadable ? TextureImporterCompression.Uncompressed : TextureImporterCompression.CompressedHQ;
            importer.SetPlatformTextureSettings(ios);
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            importer.spritePivot = pivot;
            settings.spritePivot = pivot;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
        }
    }
}
