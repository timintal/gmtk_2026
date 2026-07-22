using FFS.Libraries.StaticEcs.Unity.Editor;
using UnityEditor;

[CustomEditor(typeof(WTEntityProvider)), CanEditMultipleObjects]
public class WTEntityProviderEditor : StaticEcsEntityProviderEditor<WT, WTEntityProvider> { }
