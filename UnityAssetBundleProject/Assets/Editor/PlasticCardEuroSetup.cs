using System;
using UnityEditor;
using UnityEngine;

public static class PlasticCardEuroSetup
{
    [MenuItem("Plastic Card/Prepare euro card assets")]
    public static void Prepare()
    {
        const string texturePath = "Assets/MagnetCard/EuroCardAlbedo.png";
        const string materialPath = "Assets/MagnetCard/PlasticCardEuro.mat";
        const string prefabPath = "Assets/MagnetCard/item_money_euro_card.prefab";
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture == null) throw new InvalidOperationException("EuroCardAlbedo.png is missing.");
        var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/MagnetCard/PlasticCardTest.mat"));
            material.name = "PlasticCard_Euro_Material";
            material.mainTexture = texture;
            AssetDatabase.CreateAsset(material, materialPath);
        }
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MagnetCard/item_money_ruble_small.prefab");
            var item = UnityEngine.Object.Instantiate(source);
            try
            {
                item.name = "item_money_euro_card";
                foreach (var renderer in item.GetComponentsInChildren<Renderer>(true)) renderer.sharedMaterial = material;
                PrefabUtility.SaveAsPrefabAsset(item, prefabPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(item); }
        }
        var contents = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            var visual = contents.transform.Find("model_LOD0");
            visual.localRotation = Quaternion.Euler(270f, 0f, 0f) * Quaternion.Euler(0f, 90f, 0f);
            PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(contents); }
        AssetImporter.GetAtPath(prefabPath).assetBundleName = "Mods/euros/euro_magnet_card.bundle";
        AssetDatabase.SaveAssets();
        Debug.Log("Euro card assets prepared; EUR prefab ready.");
    }

    public static void PrepareAndBuild()
    {
        PlasticCardDollarSetup.Prepare();
        Prepare();
        PlasticCardBundleBuilder.Build();
    }
}




