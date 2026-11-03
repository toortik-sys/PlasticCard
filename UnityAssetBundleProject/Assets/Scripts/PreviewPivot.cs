using System;
using UnityEngine;

public class PreviewPivot : MonoBehaviour
{
    public Vector3 pivotPosition = Vector3.zero;
    public Quaternion pivotRotation = Quaternion.identity;
#if UNITY_EDITOR
    public Vector3 pivotRotationEuler;
#endif
    public Vector3 scale = Vector3.one;
    public Vector3 SpawnPosition = Vector3.zero;
    public IconSettings Icon = new();
    private Quaternion _oldRotation;
#if UNITY_EDITOR
    public static bool SkipPivotRotationValidation;

    private void OnValidate()
    {
        if (!SkipPivotRotationValidation)
            pivotRotation = Quaternion.Euler(pivotRotationEuler);
    }
#endif

    [Serializable]
    public class IconSettings
    {
        public Vector3 position = Vector3.zero;
        public bool hasOffset;
        public Quaternion rotation = Quaternion.identity;
#if UNITY_EDITOR
        public Vector3 rotationEuler;
#endif
        public float boundsScale = 1f;
        public float perspective = 15f;
        public bool orthographic;
        public float orthographicSize = 10f;
    }
}
