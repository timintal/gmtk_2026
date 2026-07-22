using System;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public class TweenLocalRotation : Vector3Tween<Transform>
    {
        protected override Vector3 Property
        {
            get => target.localRotation.eulerAngles;
            set
            {
                Vector3 newAngles = value;

                target.localRotation = Quaternion.Euler(newAngles);
            }
        }
    }

    [Serializable]
    public class TweenLocalRotationQuaternion : QuaternionTween<Transform>
    {
        protected override Quaternion Property
        {
            get => target.localRotation;
            set { target.localRotation = value; }
        }
    }
}