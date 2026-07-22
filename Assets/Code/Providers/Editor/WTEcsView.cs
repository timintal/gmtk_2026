using FFS.Libraries.StaticEcs.Unity.Editor;
using UnityEditor;

public class WTEcsView : StaticEcsView<WT, WTEntityProvider, WTEventProvider> {
    [MenuItem("Window/WT ECS")]
    public static void OpenWindow() {
        var window = GetWindow<WTEcsView>();
        window.Show();
        window.Focus();
    }
}
