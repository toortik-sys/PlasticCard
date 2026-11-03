using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class AssetBundleInspector
{
    public static void InspectDollar()
    {
        PreviewPivot.SkipPivotRotationValidation = true;
        Inspect(Path.Combine(OutputRoot, "Mods/dollars/dollar_magnet_card.bundle"));
        EditorApplication.Exit(0);
    }
    private static string GameRoot => Environment.GetEnvironmentVariable("SPT_GAME_PATH") ?? @"C:\SPT";
    private static string OutputRoot => Path.GetFullPath(Path.Combine(Application.dataPath, "../BuildOutput"));
    private static string VanillaRubleBundle => Path.Combine(GameRoot, "EscapeFromTarkov_Data/StreamingAssets/Windows/assets/content/items/money/item_money_ruble_small.bundle");
    private static string VanillaCardBundle => Path.Combine(GameRoot, "EscapeFromTarkov_Data/StreamingAssets/Windows/assets/content/items/money/item_money_bank_card.bundle");
    private static string ModBundle => Path.Combine(OutputRoot, "Mods/roubles/rouble_magnet_card.bundle");

    [MenuItem("Plastic Card/Inspect vanilla and custom bundles")]
    public static void InspectBundles()
    {
        PreviewPivot.SkipPivotRotationValidation = true;
        try
        {
            Inspect(VanillaRubleBundle);
            Inspect(VanillaCardBundle);
            Inspect(ModBundle);
            Inspect(Path.Combine(OutputRoot, "Mods/dollars/dollar_magnet_card.bundle"));
            Inspect(Path.Combine(OutputRoot, "Mods/euros/euro_magnet_card.bundle"));
            var importedModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MagnetCard/MagnetCard.FBX");
            if (importedModel != null)
            {
                Debug.Log("--- INSPECT IMPORTED FBX ---");
                InspectTransform(importedModel.transform, 0);
                foreach (var filter in importedModel.GetComponentsInChildren<MeshFilter>(true))
                    Debug.Log($"FBX MESH: {filter.name}; localBounds={filter.sharedMesh.bounds}; vertices={filter.sharedMesh.vertexCount}");
                foreach (var renderer in importedModel.GetComponentsInChildren<Renderer>(true))
                    Debug.Log($"FBX RENDERER: {renderer.name}; bounds={renderer.bounds.size}; materials={string.Join(",", Array.ConvertAll(renderer.sharedMaterials, m => m == null ? "null" : m.name))}");
            }
        }
        finally
        {
            PreviewPivot.SkipPivotRotationValidation = false;
        }
        EditorApplication.Exit(0);
    }

    private static void Inspect(string path)
    {
        Debug.Log($"--- INSPECT BUNDLE {path} ---");
        var bundle = AssetBundle.LoadFromFile(path);
        if (bundle == null)
        {
            Debug.LogError($"Could not load bundle: {path}");
            return;
        }

        foreach (var assetName in bundle.GetAllAssetNames())
            Debug.Log($"ASSET NAME: {assetName}");

        foreach (var asset in bundle.LoadAllAssets())
        {
            Debug.Log($"ASSET TYPE: {asset.GetType().Name} | {asset.name}");
            if (asset is not GameObject gameObject) continue;
            InspectTransform(gameObject.transform, 0);
            foreach (var component in gameObject.GetComponentsInChildren<Component>(true))
            {
                if (component == null)
                {
                    Debug.Log("COMPONENT: <missing script>");
                    continue;
                }

                Debug.Log($"COMPONENT: {component.GetType().FullName} | {component.name} | layer={component.gameObject.layer} | enabled={(component is Behaviour behaviour ? behaviour.enabled.ToString() : "n/a")}");
                if (component is Collider collider)
                {
                    var localCenter = collider is BoxCollider box ? box.center.ToString() : "n/a";
                    var localSize = collider is BoxCollider box2 ? box2.size.ToString() : "n/a";
                    Debug.Log($"COLLIDER: type={collider.GetType().Name}; enabled={collider.enabled}; trigger={collider.isTrigger}; center={collider.bounds.center}; bounds={collider.bounds.size}; localCenter={localCenter}; localSize={localSize}");
                }
                if (component is PreviewPivot pivot)
                    Debug.Log($"PREVIEW PIVOT: pivotPosition={pivot.pivotPosition}; pivotRotation={pivot.pivotRotation.eulerAngles}; scale={pivot.scale}; SpawnPosition={pivot.SpawnPosition}; IconRotation={pivot.Icon.rotation.eulerAngles}; iconBoundsScale={pivot.Icon.boundsScale}");
            }
            foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null) continue;
                    var mainTexture = material.mainTexture;
                    Debug.Log($"MATERIAL: {material.name}; shader={material.shader?.name}; mainTexture={mainTexture?.name}; renderer={renderer.name}; bounds={renderer.bounds.size}");
                }
            }
        }

        bundle.Unload(false);
    }

    private static void InspectTransform(Transform transform, int depth)
    {
        Debug.Log($"TRANSFORM: {new string(' ', depth * 2)}{transform.name}; localPos={transform.localPosition}; localRot={transform.localEulerAngles}; localScale={transform.localScale}; worldPos={transform.position}; worldScale={transform.lossyScale}");
        foreach (Transform child in transform)
            InspectTransform(child, depth + 1);
    }
}



