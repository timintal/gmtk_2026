#if UNITY_SPLINES
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace EasyTweens
{
    [Serializable]
    public class UnitySplineMover : FloatTween<Transform>
    {
        [ExposeInEditor] public SplineContainer Spline;
        [ExposeInEditor] public int splineIndex;
        
        [ExposeInEditor, Tooltip("The coordinate space that the GameObject's up and forward axes align to.")]
        public SplineAnimate.AlignmentMode m_AlignmentMode = SplineAnimate.AlignmentMode.SplineElement;

        [ExposeInEditor, Tooltip("Which axis of the GameObject is treated as the forward axis.")]
        public AlignAxis m_ObjectForwardAxis = AlignAxis.ZAxis;

        [ExposeInEditor, Tooltip("Which axis of the GameObject is treated as the up axis.")]
        public AlignAxis m_ObjectUpAxis = AlignAxis.YAxis;
        private float factor;
        
        protected override float Property
        {
            get => factor;
            set
            {
                factor = value;
                EvaluatePositionAndRotation(out var pos, out var rotation, factor);
                target.position = pos;
                target.rotation = rotation;
            }
        }
        
        void EvaluatePositionAndRotation(out Vector3 position, out Quaternion rotation, float t)
        {
            position = Spline.transform.TransformPoint(Spline.Splines[splineIndex].EvaluatePosition(t));
            rotation = Quaternion.identity;

            // Correct forward and up vectors based on axis remapping parameters
            var remappedForward = GetAxis(m_ObjectForwardAxis);
            var remappedUp = GetAxis(m_ObjectUpAxis);
            var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(remappedForward, remappedUp));

            if (m_AlignmentMode != SplineAnimate.AlignmentMode.None)
            {
                var forward = Vector3.forward;
                var up = Vector3.up;

                switch (m_AlignmentMode)
                {
                    case SplineAnimate.AlignmentMode.SplineElement:
                        forward = Spline.Splines[splineIndex].EvaluateTangent(t);
                        if (Vector3.Magnitude(forward) <= Mathf.Epsilon)
                        {
                            if (t < 1f)
                                forward = Spline.Splines[splineIndex].EvaluateTangent( Mathf.Min(1f, t + 0.01f));
                            else
                                forward = Spline.Splines[splineIndex].EvaluateTangent(t - 0.01f);
                        }
                        forward.Normalize();
                        up = Spline.Splines[splineIndex].EvaluateUpVector(t);
                        break;

                    case SplineAnimate.AlignmentMode.SplineObject:
                       //skip

                    default:
                        Debug.Log($"{m_AlignmentMode} animation alignment mode is not supported!");
                        break;
                }

                rotation = Quaternion.LookRotation(forward, up) * axisRemapRotation;
            }
            else
                rotation = target.rotation;
        }
        
        public enum AlignAxis
        {
            /// <summary> Object space X axis. </summary>
            [InspectorName("Object X+")]
            XAxis,
            /// <summary> Object space Y axis. </summary>
            [InspectorName("Object Y+")]
            YAxis,
            /// <summary> Object space Z axis. </summary>
            [InspectorName("Object Z+")]
            ZAxis,
            /// <summary> Object space negative X axis. </summary>
            [InspectorName("Object X-")]
            NegativeXAxis,
            /// <summary> Object space negative Y axis. </summary>
            [InspectorName("Object Y-")]
            NegativeYAxis,
            /// <summary> Object space negative Z axis. </summary>
            [InspectorName("Object Z-")]
            NegativeZAxis
        }
                
        readonly float3[] m_AlignAxisToVector = new float3[] {math.right(), math.up(), math.forward(), math.left(), math.down(), math.back()};

        /// <summary>
        /// Transform a AlignAxis to the associated float3 direction. 
        /// </summary>
        /// <param name="axis">The AlignAxis to transform</param>
        /// <returns></returns>
        protected float3 GetAxis(AlignAxis axis)
        {
            return m_AlignAxisToVector[(int) axis];
        }
    }
}
#endif