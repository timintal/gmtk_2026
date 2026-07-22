using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace EasyTweens
{
    public enum AlignVector
    {
        Forward,
        Up,
        Right
    }
    
    [Serializable]
    public class SplineMover : FloatTween<Transform>
    {
        [ExposeInEditor] public BezierSpline Spline;

        [ExposeInEditor] public bool AlignWithPath;
        [ExposeInEditor] public AlignVector AlignVector = AlignVector.Forward;
        
        protected override float Property
        {
            get => 0;
            set
            {
                target.position = Spline.GetWorldPoint(value);
                if (AlignWithPath)
                {
                    var worldDirection = Spline.GetWorldDirection(value);
                    Vector3 forwardVector = AlignVector == AlignVector.Forward ? Vector3.forward : AlignVector == AlignVector.Up ? Vector3.up : Vector3.right;
                    
                    Quaternion toDirection = Quaternion.FromToRotation(forwardVector, worldDirection);
                    target.rotation = toDirection;
                }
            }
        }
    }
}