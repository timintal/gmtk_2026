using Code.Common;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class SetMainCameraForCanvas : MonoBehaviour
{
    private void Start()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = W.GetResource<MainCamera>().Value;
        }
    }
}
