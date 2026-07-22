using Code.Features.DragAndDrop;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using Code.Features.DragAndDrop.Utils;
using UnityEditor;
#endif

[DisallowMultipleComponent]
[AddComponentMenu("Static Proto/Drag And Drop/Draggable Setup")]
public sealed class DraggableSetup : MonoBehaviour
{
    [Tooltip("Auto picks UI when this object has a RectTransform, otherwise world 2D.")]
    public DragSetupSpace Space = DragSetupSpace.Auto;

    public int Priority;

    [Tooltip("World 2D only. Adds a BoxCollider2D when none exists on this object or its children.")]
    public bool AddColliderIfMissing = true;

    [Button("Apply Draggable Setup")]
    public void ApplySetup()
    {
#if UNITY_EDITOR
        Undo.RecordObject(gameObject, "Apply Draggable Setup");

        var hadProvider = gameObject.GetComponent<WTEntityProvider>() != null;
        var provider = DragAndDropSetupUtility.EnsureEntityProvider(gameObject);
        if (!hadProvider)
        {
            Undo.RegisterCreatedObjectUndo(provider, "Apply Draggable Setup");
        }

        DragAndDropSetupUtility.ApplyDraggable(gameObject, Space, Priority, AddColliderIfMissing);

        EditorUtility.SetDirty(gameObject);
        if (provider != null)
        {
            EditorUtility.SetDirty(provider);
        }

        Undo.DestroyObjectImmediate(this);
#else
        Debug.LogWarning($"{nameof(DraggableSetup)} is editor-only.", this);
#endif
    }
}
