using UnityEngine;

namespace EasyTweens
{
    public interface ITargetSetter<T> where T : Object
    {
        void SetTarget(T target);
    }
}