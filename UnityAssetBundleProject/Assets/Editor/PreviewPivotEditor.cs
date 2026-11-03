using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PreviewPivot))]
public sealed class PreviewPivotEditor : Editor
{
    private static readonly Quaternion ModelBaseRotation =
        Quaternion.Euler(270f, 0f, 0f) * Quaternion.Euler(0f, 90f, 0f);

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();

        var pivot = (PreviewPivot)target;
        ApplyPreviewRotation(pivot);

        EditorGUILayout.HelpBox(
            "Для живого предпросмотра выбери корневой item_money_ruble_small и перетаскивай поворотный манипулятор в окне Scene. Он вращает модель и записывает углы в Pivot Rotation Euler.",
            MessageType.Info);
    }

    private void OnSceneGUI()
    {
        var pivot = (PreviewPivot)target;
        var handlePosition = pivot.transform.TransformPoint(pivot.pivotPosition);
        var currentRotation = pivot.transform.rotation * pivot.pivotRotation;
        EditorGUI.BeginChangeCheck();
        var newRotation = Handles.RotationHandle(currentRotation, handlePosition);
        if (!EditorGUI.EndChangeCheck()) return;

        Undo.RecordObject(pivot, "Rotate item preview pivot");
        pivot.pivotRotation = Quaternion.Inverse(pivot.transform.rotation) * newRotation;
        pivot.pivotRotationEuler = pivot.pivotRotation.eulerAngles;
        EditorUtility.SetDirty(pivot);
        ApplyPreviewRotation(pivot);
        SceneView.RepaintAll();
    }

    private static void ApplyPreviewRotation(PreviewPivot pivot)
    {
        var model = pivot.transform.Find("model_LOD0");
        if (model == null) return;

        var previewRotation = pivot.pivotRotation * ModelBaseRotation;
        if (Quaternion.Angle(model.localRotation, previewRotation) < 0.01f) return;

        Undo.RecordObject(model, "Preview item pivot rotation");
        model.localRotation = previewRotation;
        EditorUtility.SetDirty(model);
    }
}
