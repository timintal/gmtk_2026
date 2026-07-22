using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace EasyTweens
{
    [Serializable]
    public abstract class TargetedTween<TTarget, TValue>: TweenBase where TTarget : UnityEngine.Object
    {
        public TTarget target;
        public TValue startValue;
        public TValue endValue;

        protected abstract TValue Property
        {
            get;
            set;
        }

        public override void SetFactor(float f)
        {
            Property = Lerp(f);
        }
#if UNITY_EDITOR
        public override void SetCurrentAsEndValue()
        {
            endValue = Property;
        }

        public override void SwapValues()
        {
            (startValue, endValue) = (endValue, startValue);
        }

        public override void SetCurrentAsStartValue()
        {
            startValue = Property;
        }
#endif
        protected abstract TValue Lerp(float factor);
    }

    [Serializable]
    public abstract class FloatTween<T> : TargetedTween<T, float> where T : UnityEngine.Object
    {
        protected override float Lerp(float factor)
        {
            return Mathf.LerpUnclamped(startValue, endValue, factor);
        }
    }
    
    [Serializable]
    public abstract class IntTween<T> : TargetedTween<T, int> where T : UnityEngine.Object
    {
        protected override int Lerp(float factor)
        {
            return Mathf.RoundToInt(Mathf.LerpUnclamped(startValue, endValue, factor));
        }
    }
    
    [Serializable]
    public abstract class Vector2Tween<T> : TargetedTween<T, Vector2> where T : UnityEngine.Object
    {
        protected override Vector2 Lerp(float factor)
        {
            return Vector2.LerpUnclamped(startValue, endValue, factor);
        }
    }
    
    [Serializable]
    public abstract class Vector3Tween<T> : TargetedTween<T, Vector3> where T : UnityEngine.Object
    {
        protected override Vector3 Lerp(float factor)
        {
            return Vector3.LerpUnclamped(startValue, endValue, factor);
        }
    }
    
    [Serializable]
    public abstract class Vector4Tween<T> : TargetedTween<T, Vector4> where T : UnityEngine.Object
    {
        protected override Vector4 Lerp(float factor)
        {
            return Vector4.LerpUnclamped(startValue, endValue, factor);
        }
    }
    
    [Serializable,
     UseCustomEditor("ColorTweenEditor"),
     StructLayout(LayoutKind.Sequential)]
    public abstract class ColorTween<T> : TargetedTween<T, Color> where T : UnityEngine.Object
    {
        [ExposeInEditor(order:-2)]
        public bool useGradient;

        [ExposeInEditor(order:-1)]
        public bool useHDRColor;

        [ExposeInEditor(order:-3)]
        public Gradient gradient;

        protected override Color Lerp(float factor)
        {
            if (useGradient)
                return gradient.Evaluate(factor);
            
            return Color.LerpUnclamped(startValue, endValue, factor);
        }
    }
    
    [Serializable]
    public abstract class RectTween<T> : TargetedTween<T, Rect> where T : UnityEngine.Object
    {
        protected override Rect Lerp(float factor)
        {
            return new Rect(Vector2.LerpUnclamped(startValue.position, endValue.position, factor),
                Vector2.LerpUnclamped(startValue.size, endValue.size, factor));
        }
    }
    
    [Serializable]
    public abstract class QuaternionTween<T> : TargetedTween<T, Quaternion> where T : UnityEngine.Object
    {
        protected override Quaternion Lerp(float factor)
        {
            return Quaternion.LerpUnclamped(startValue, endValue, factor);
        }
    }
}