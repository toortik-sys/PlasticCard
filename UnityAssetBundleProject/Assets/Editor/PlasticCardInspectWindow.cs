using UnityEditor;
using UnityEngine;

public sealed class PlasticCardInspectWindow : EditorWindow
{
    [SerializeField] private string prefabPath = "Assets/MagnetCard/item_money_ruble_small.prefab";
    private PreviewRenderUtility preview;
    private GameObject item;
    private Quaternion savedRotation;
    private Vector3 adjustment;
    private bool saved;

    [MenuItem("Plastic Card/Inspect camera — live editor")]
    public static void Open()
    {
        var window = GetWindow<PlasticCardInspectWindow>("Просмотр карты");
        window.prefabPath = "Assets/MagnetCard/item_money_ruble_small.prefab";
        window.Reload();
        window.minSize = new Vector2(650, 540);
        window.Show();
    }

    [MenuItem("Plastic Card/Inspect dollar card")]
    public static void OpenDollar()
    {
        var window = GetWindow<PlasticCardInspectWindow>("Просмотр карты USD");
        window.prefabPath = "Assets/MagnetCard/item_money_dollar_card.prefab";
        window.Reload();
        window.minSize = new Vector2(650, 540);
        window.Show();
    }

    private void OnEnable() { Reload(); }
    private void OnDisable()
    {
        if (preview != null) preview.Cleanup();
        preview = null;
        item = null;
    }

    private void Reload()
    {
        OnDisable();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) return;
        savedRotation = prefab.GetComponent<PreviewPivot>().Icon.rotation;
        adjustment = Vector3.zero;
        preview = new PreviewRenderUtility();
        preview.camera.fieldOfView = 15f;
        preview.camera.nearClipPlane = 0.001f;
        preview.camera.farClipPlane = 10f;
        preview.camera.clearFlags = CameraClearFlags.SolidColor;
        preview.camera.backgroundColor = new Color(0.025f, 0.025f, 0.025f);
        preview.ambientColor = new Color(0.35f, 0.35f, 0.35f);
        preview.lights[0].intensity = 1.1f;
        preview.lights[0].transform.rotation = Quaternion.Euler(25f, -20f, 0f);
        preview.lights[1].intensity = 0.5f;
        preview.lights[1].transform.rotation = Quaternion.Euler(340f, 160f, 0f);
        item = Instantiate(prefab);
        item.hideFlags = HideFlags.HideAndDontSave;
        // The scene rotation handle from the old editor may have altered this
        // child. Match the canonical model transform produced by the builder.
        item.transform.Find("model_LOD0").localRotation =
            Quaternion.Euler(270f, 0f, 0f) * Quaternion.Euler(0f, 90f, 0f);
        preview.AddSingleGO(item);
    }

    private Quaternion Rotation =>
        Quaternion.AngleAxis(adjustment.z, Vector3.forward) *
        Quaternion.AngleAxis(adjustment.y, Vector3.up) *
        Quaternion.AngleAxis(adjustment.x, Vector3.right) * savedRotation;

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("Это поворот Icon.rotation, который читает просмотр предмета в игре. Камера смотрит вдоль Z, перспектива 15°. Освещение здесь может отличаться от EFT.", MessageType.Info);
        if (item == null || preview == null)
        {
            if (GUILayout.Button("Загрузить карту")) Reload();
            return;
        }
        EditorGUI.BeginChangeCheck();
        adjustment.x = EditorGUILayout.Slider("Наклон верхнего края", adjustment.x, -60f, 60f);
        adjustment.y = EditorGUILayout.Slider("Разворот влево / вправо", adjustment.y, -60f, 60f);
        adjustment.z = EditorGUILayout.Slider("Наклон в плоскости экрана", adjustment.z, -90f, 90f);
        if (EditorGUI.EndChangeCheck()) { saved = false; Repaint(); }
        EditorGUILayout.LabelField("Итоговый Icon Rotation", Rotation.eulerAngles.ToString("F2"));
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Сбросить к сохранённому")) { adjustment = Vector3.zero; saved = false; }
        if (GUILayout.Button("Сохранить поворот для сборки")) Save();
        EditorGUILayout.EndHorizontal();
        if (saved) EditorGUILayout.HelpBox("Поворот сохранён в префаб. Сборщик использует именно его.", MessageType.Info);

        var rect = GUILayoutUtility.GetRect(100, 10000, 300, 10000, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        if (Event.current.type != EventType.Repaint) return;
        item.transform.SetPositionAndRotation(Vector3.zero, Rotation);
        var renderers = item.GetComponentsInChildren<Renderer>();
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        // A bounding sphere keeps zoom fixed while changing the tilt.
        var radius = 0.09f;
        var aspect = Mathf.Max(0.1f, rect.width / rect.height);
        var distance = radius / Mathf.Tan(7.5f * Mathf.Deg2Rad) * Mathf.Max(1f, 1f / aspect);
        preview.camera.transform.SetPositionAndRotation(bounds.center + new Vector3(0, 0, -distance), Quaternion.identity);
        preview.BeginPreview(rect, GUIStyle.none);
        preview.camera.Render();
        var texture = preview.EndPreview();
        GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, false);
    }

    private void Save()
    {
        var rotation = Rotation;
        var prefab = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            var pivot = prefab.GetComponent<PreviewPivot>();
            pivot.Icon.rotation = rotation;
            pivot.Icon.rotationEuler = rotation.eulerAngles;
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
        savedRotation = rotation;
        adjustment = Vector3.zero;
        saved = true;
    }
}
