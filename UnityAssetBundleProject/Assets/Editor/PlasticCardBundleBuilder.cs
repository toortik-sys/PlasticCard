using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public static class PlasticCardBundleBuilder
{
    private const string ModelPath = "Assets/MagnetCard/MagnetCard.FBX";
    private const string AlbedoPath = "Assets/MagnetCard/MagnetCardAlbedo.png";
    private const string NormalPath = "Assets/MagnetCard/MagnetCardNormal.png";
    private const string MaterialPath = "Assets/MagnetCard/PlasticCardTest.mat";
    private const string PrefabPath = "Assets/MagnetCard/item_money_ruble_small.prefab";
    private const string BundleName = "Mods/roubles/rouble_magnet_card.bundle";
    private const float ModelScale = 1.6f;
    private static readonly Vector3 DefaultPreviewRotationEuler = new(172f, 0f, -8f);
    private static readonly Vector3 VanillaRubleIconRotationEuler = new(1.43f, 221.70f, 76.82f);

    [MenuItem("Plastic Card/Build ruble test bundle")]
    public static void Build()
    {
        try
        {
            ConfigureTextureImport(AlbedoPath, TextureImporterType.Default, true);
            ConfigureTextureImport(NormalPath, TextureImporterType.NormalMap, false);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoPath);
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(NormalPath);
            if (model == null || albedo == null || normal == null)
                throw new InvalidOperationException("Model or required source textures were not imported.");

            var shader = Shader.Find("Standard");
            if (shader == null)
                throw new InvalidOperationException("Built-in Standard shader was not found.");

            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            material.shader = shader;
            material.name = "MagnetCard_Test_Material";
            material.SetTexture("_MainTex", albedo);
            material.SetTexture("_BumpMap", normal);
            material.SetFloat("_BumpScale", 1.0f);
            material.SetFloat("_Metallic", 0.0f);
            material.SetFloat("_Glossiness", 0.28f);
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);

            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var existingPreviewPivot = existingPrefab != null ? existingPrefab.GetComponent<PreviewPivot>() : null;
            var previewRotationEuler = existingPreviewPivot != null
                ? existingPreviewPivot.pivotRotationEuler
                : DefaultPreviewRotationEuler;
            Debug.Log($"Plastic Card preview rotation input: {previewRotationEuler}; source prefab found={existingPreviewPivot != null}.");
            // EFT's inspect camera looks along the preview's Z axis. Rotate
            // around that camera axis to turn the portrait card landscape
            // while keeping its face and existing tilt toward the camera.
            var cardModelRotation = Quaternion.Euler(270f, 0f, 0f) * Quaternion.Euler(0f, 90f, 0f);
            var baseIconRotation = Quaternion.Euler(VanillaRubleIconRotationEuler);
            var iconRotation = existingPreviewPivot != null
                ? existingPreviewPivot.Icon.rotation
                : Quaternion.AngleAxis(90f, Vector3.forward) * baseIconRotation;
            var iconRotationEuler = iconRotation.eulerAngles;

            var root = new GameObject("item_money_ruble_small");
            var previewPivot = root.AddComponent<PreviewPivot>();
            previewPivot.pivotPosition = Vector3.zero;
            // Keep the user's adjusted game-world preview orientation.
            previewPivot.pivotRotationEuler = previewRotationEuler;
            previewPivot.pivotRotation = Quaternion.Euler(previewPivot.pivotRotationEuler);
            previewPivot.scale = Vector3.one;
            previewPivot.SpawnPosition = Vector3.zero;
            previewPivot.Icon = new PreviewPivot.IconSettings
            {
                position = Vector3.zero,
                hasOffset = false,
                // EFT's inspect camera consumes Icon.rotation. Use the native RUB
                // preview angle; pivotRotation is used by other world/stash paths.
                rotation = iconRotation,
                rotationEuler = iconRotationEuler,
                boundsScale = 0.65f,
                perspective = 15f,
                orthographic = false,
                orthographicSize = 10f
            };
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            instance.name = "model_LOD0";
            instance.transform.SetParent(root.transform, false);
            instance.transform.localPosition = Vector3.zero;
            // Keep the item root unrotated like EFT's money prefabs. Bake the old
            // root orientation into the visual child so EFT can set world rotation
            // without also turning the physical collider edge-on.
            instance.transform.localRotation = cardModelRotation;
            instance.transform.localScale = Vector3.one * ModelScale;
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterial = material;

            var lodGroup = root.AddComponent<LODGroup>();
            var modelRenderers = instance.GetComponentsInChildren<Renderer>(true);
            lodGroup.SetLODs(new[] { new LOD(0.01f, modelRenderers) });
            lodGroup.RecalculateBounds();

            // EFT money prefabs have a separate child named "colider". Keep the
            // collision volume aligned with the card plane and give it enough
            // thickness to remain stable when the dropped item rotates/falls.
            var colliderObject = new GameObject("colider");
            colliderObject.transform.SetParent(root.transform, false);
            colliderObject.transform.localRotation = instance.transform.localRotation;
            var boxCollider = colliderObject.AddComponent<BoxCollider>();
            boxCollider.center = Vector3.zero;
            boxCollider.size = new Vector3(0.02f, 0.10f, 0.14f);

            var oldPrefabImporter = AssetImporter.GetAtPath("Assets/MagnetCard/PlasticCardRouble.prefab");
            if (oldPrefabImporter != null)
                oldPrefabImporter.assetBundleName = string.Empty;

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            if (prefab == null)
                throw new InvalidOperationException("Could not save the generated prefab.");
            var savedPreviewPivot = prefab.GetComponent<PreviewPivot>();
            Debug.Log($"Plastic Card saved preview rotation: euler={savedPreviewPivot.pivotRotationEuler}; quaternion={savedPreviewPivot.pivotRotation.eulerAngles}.");

            AssetDatabase.SaveAssets();
            var prefabImporter = AssetImporter.GetAtPath(PrefabPath);
            prefabImporter.assetBundleName = BundleName;
            prefabImporter.assetBundleVariant = string.Empty;
            AssetDatabase.SaveAssets();

            var output = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "BuildOutput"));
            Directory.CreateDirectory(output);
            var manifest = BuildPipeline.BuildAssetBundles(output, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
            if (manifest == null)
                throw new InvalidOperationException("BuildPipeline returned no AssetBundleManifest.");

            var bounds = new Bounds();
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
                bounds.Encapsulate(renderer.bounds);
            Debug.Log($"PlasticCard bundle built: {Path.Combine(output, BundleName)}; prefab={prefab.name}, scale={ModelScale:F2}, renderer bounds={bounds.size}.");
            EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }

    private static void ConfigureTextureImport(string path, TextureImporterType type, bool srgb)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException($"Texture importer not found for {path}.");
        if (importer.textureType != type || importer.sRGBTexture != srgb || importer.maxTextureSize < 2048)
        {
            importer.textureType = type;
            importer.sRGBTexture = srgb;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.SaveAndReimport();
        }
    }
}



