using Code.Common;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(Canvas))]
public class SetMainCameraForCanvas : MonoBehaviour
{
    [Inject] internal MainCamera mainCamera;
    
    private void Start()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = mainCamera.Value;
        }
    }
}
