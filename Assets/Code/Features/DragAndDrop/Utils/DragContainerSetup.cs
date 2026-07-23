using Code.Features.DragAndDrop;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using Code.Features.DragAndDrop.Utils;
using UnityEditor;
#endif

[DisallowMultipleComponent]
[AddComponentMenu("Static Proto/Drag And Drop/Drag Container Setup")]
public sealed class DragContainerSetup : MonoBehaviour
{
    [Tooltip("Auto picks UI when this object has a RectTransform, otherwise world 2D.")]
    public DragSetupSpace Space = DragSetupSpace.Auto;

    public int Priority;

    [Tooltip("0 means unlimited capacity.")]
    public int Capacity;

    [Tooltip("World 2D only. Adds a BoxCollider2D when none exists on this object or its children.")]
    public bool AddColliderIfMissing = true;

    public DragContainerLayoutKind Layout = DragContainerLayoutKind.None;

    [ShowIf(nameof(Layout), DragContainerLayoutKind.Line)]
    public LineContainerLayout LineLayout = new()
    {
        Spacing = 1.2f,
        LocalOffset = Vector2.zero,
        Direction = Vector2.right,
        Centered = true
    };

    [ShowIf(nameof(Layout), DragContainerLayoutKind.Grid)]
    public GridContainerLayout GridLayout = new()
    {
        ColumnCount = 2,
        CellSpacing = new Vector2(1.2f, 1.2f),
        LocalOffset = Vector2.zero,
        Centered = true
    };

    [ShowIf(nameof(Layout), DragContainerLayoutKind.Free)]
    public FreeContainerLayout FreeLayout = new()
    {
        LocalOffset = Vector2.zero,
        Bounds = Vector2.zero,
        ItemSize = Vector2.zero,
        OverlapTolerance = 0.25f,
        RelaxIterations = 8
    };

    [Button("Apply Container Setup")]
    public void ApplySetup()
    {
#if UNITY_EDITOR
        Undo.RecordObject(gameObject, "Apply Drag Container Setup");

        var hadProvider = gameObject.GetComponent<WTEntityProvider>() != null;
        var provider = DragAndDropSetupUtility.EnsureEntityProvider(gameObject);
        if (!hadProvider)
        {
            Undo.RegisterCreatedObjectUndo(provider, "Apply Drag Container Setup");
        }

        DragAndDropSetupUtility.ApplyContainer(
            gameObject,
            Space,
            Priority,
            Capacity,
            Layout,
            LineLayout,
            GridLayout,
            FreeLayout,
            AddColliderIfMissing);

        EditorUtility.SetDirty(gameObject);
        if (provider != null)
        {
            EditorUtility.SetDirty(provider);
        }

        Undo.DestroyObjectImmediate(this);
#else
        Debug.LogWarning($"{nameof(DragContainerSetup)} is editor-only.", this);
#endif
    }
}
