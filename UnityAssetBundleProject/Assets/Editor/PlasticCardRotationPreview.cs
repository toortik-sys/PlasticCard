using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PlasticCardRotationPreview
{
    private const string ModelPath = "Assets/MagnetCard/MagnetCard.FBX";
    private const string MaterialPath = "Assets/MagnetCard/PlasticCardTest.mat";
    private const float ModelScale = 1.6f;

    [MenuItem("Plastic Card/Render inspect rotation candidates")]
    public static void Render()
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (model == null || material == null)
            throw new InvalidOperationException("Card model or material is missing.");

        var baseIcon = Quaternion.Euler(1.43f, 221.70f, 76.82f);
        var modelRotation = Quaternion.Euler(270f, 0f, 0f) * Quaternion.Euler(0f, 90f, 0f);
        var faceNormal = baseIcon * (modelRotation * Vector3.up);
        var candidates = new (string name, Quaternion rotation)[]
        {
            ("base", baseIcon),
            ("world_z_plus90", Quaternion.AngleAxis(90f, Vector3.forward) * baseIcon),
            ("world_z_minus90", Quaternion.AngleAxis(-90f, Vector3.forward) * baseIcon),
            ("world_x_plus90", Quaternion.AngleAxis(90f, Vector3.right) * baseIcon),
            ("world_y_plus90", Quaternion.AngleAxis(90f, Vector3.up) * baseIcon),
            ("local_z_plus90", baseIcon * Quaternion.AngleAxis(90f, Vector3.forward)),
            ("local_z_minus90", baseIcon * Quaternion.AngleAxis(-90f, Vector3.forward)),
            ("calculated_normal_plus90", Quaternion.AngleAxis(90f, faceNormal) * baseIcon),
            ("euler_z_plus90", Quaternion.Euler(new Vector3(1.43f, 221.70f, 166.82f)) )
        };

        const int tile = 384;
        const int columns = 3;
        int rows = Mathf.CeilToInt(candidates.Length / (float)columns);
        var sheet = new Texture2D(columns * tile, rows * tile, TextureFormat.RGB24, false);
        var readback = new Texture2D(tile, tile, TextureFormat.RGB24, false);
        var renderTexture = new RenderTexture(tile, tile, 24, RenderTextureFormat.ARGB32);
        var root = new GameObject("PlasticCardRotationPreviewRoot");
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
        instance.transform.SetParent(root.transform, false);
        instance.transform.localRotation = modelRotation;
        instance.transform.localScale = Vector3.one * ModelScale;
        foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
            renderer.sharedMaterial = material;

        var cameraObject = new GameObject("PreviewCamera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.transform.position = new Vector3(0f, 0f, -0.7f);
        camera.transform.rotation = Quaternion.identity;
        camera.orthographic = true;
        camera.orthographicSize = 0.22f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.035f, 0.035f, 0.035f, 1f);
        camera.targetTexture = renderTexture;
        camera.allowHDR = false;
        camera.allowMSAA = false;

        var lightObject = new GameObject("PreviewLight");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = Color.white;
        light.transform.rotation = Quaternion.Euler(25f, -20f, 0f);

        var previousActive = RenderTexture.active;
        var previousTarget = camera.targetTexture;
        var previousBackground = camera.backgroundColor;
        try
        {
            for (var i = 0; i < candidates.Length; i++)
            {
                root.transform.localRotation = candidates[i].rotation;
                camera.Render();
                RenderTexture.active = renderTexture;
                readback.ReadPixels(new Rect(0, 0, tile, tile), 0, 0);
                readback.Apply();
                var x = (i % columns) * tile;
                var y = (rows - 1 - i / columns) * tile;
                sheet.SetPixels(x, y, tile, tile, readback.GetPixels());
                Debug.Log($"Rotation preview {i}: {candidates[i].name}, Euler={candidates[i].rotation.eulerAngles}");
            }

            sheet.Apply();
            var output = Path.GetFullPath(Path.Combine(Application.dataPath, "../BuildOutput/plastic_card_rotation_candidates.png"));
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllBytes(output, sheet.EncodeToPNG());
            Debug.Log($"Rotation preview saved: {output}");
        }
        finally
        {
            RenderTexture.active = previousActive;
            camera.targetTexture = previousTarget;
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(lightObject);
            UnityEngine.Object.DestroyImmediate(sheet);
            UnityEngine.Object.DestroyImmediate(readback);
            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(renderTexture);
            EditorApplication.Exit(0);
        }
    }
}
