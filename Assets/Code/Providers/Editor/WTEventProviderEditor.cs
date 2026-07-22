using FFS.Libraries.StaticEcs.Unity.Editor;
using UnityEditor;

[CustomEditor(typeof(WTEventProvider)), CanEditMultipleObjects]
public class WTEventProviderEditor : StaticEcsEvenTEntityProviderEditor<WT, WTEventProvider> { }
