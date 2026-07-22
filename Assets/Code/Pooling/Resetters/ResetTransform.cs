using UnityEngine;

public class ResetTransform : MonoBehaviour,IPoolReset
{
    [SerializeField] private Transform[] _transforms;
    [SerializeField] bool _resetLocalPosition;
    [SerializeField] bool _resetLocalRotation;
    [SerializeField] bool _resetLocalScale;
    public void ResetForReuse()
    {
        foreach (var t in _transforms)
        {
            if (_resetLocalPosition)
            {
                t.localPosition = Vector3.zero;
            }
            if (_resetLocalRotation)
            {
                t.localRotation = Quaternion.identity;
            }
            if (_resetLocalScale)
            {
                t.localScale = Vector3.one;
            }
        }
    }
}
