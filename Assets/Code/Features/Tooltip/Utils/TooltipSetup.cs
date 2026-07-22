using Code.Features.Tooltip;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using Code.Features.Tooltip.Utils;
using UnityEditor;
#endif

[DisallowMultipleComponent]
[AddComponentMenu("Static Proto/Tooltip/Tooltip Setup")]
public sealed class TooltipSetup : MonoBehaviour
{
    public TooltipType Type = TooltipType.Text;

    [TextArea]
    public string Text = "Tooltip text";

    [Button("Apply Tooltip Setup")]
    public void ApplySetup()
    {
#if UNITY_EDITOR
        Undo.RecordObject(gameObject, "Apply Tooltip Setup");

        var provider = TooltipSetupUtility.EnsureEntityProvider(gameObject);
        TooltipSetupUtility.ApplyTooltip(gameObject, provider, Type, Text);

        EditorUtility.SetDirty(gameObject);
        if (provider != null)
        {
            EditorUtility.SetDirty(provider);
        }

        Undo.DestroyObjectImmediate(this);
#else
        Debug.LogWarning($"{nameof(TooltipSetup)} is editor-only.", this);
#endif
    }
}
