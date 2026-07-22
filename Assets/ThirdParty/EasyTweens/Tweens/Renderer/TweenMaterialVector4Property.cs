using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyTweens
{
    [Serializable]
    public class TweenMaterialVector4Property : Vector4Tween<Renderer>
    {
        [ExposeInEditor, ShaderProperty(ShaderPropertyType.Vector, ShaderPropertyType.Texture)]
        public string PropertyName;

        [ExposeInEditor(fontSize: 10)] public bool usePropertyBlock;

        MaterialPropertyBlock _propertyBlock;

        protected override Vector4 Property
        {
            get
            {
                if (usePropertyBlock)
                {
                    if (_propertyBlock == null)
                        _propertyBlock = new MaterialPropertyBlock();

                    target.GetPropertyBlock(_propertyBlock);
                    return _propertyBlock.GetVector(PropertyName);
                }

                if (Application.isPlaying)
                    return target.material.GetVector(PropertyName);
                else
                    return target.sharedMaterial.GetVector(PropertyName);
            }
            set
            {
                if (usePropertyBlock)
                {
                    if (_propertyBlock == null)
                        _propertyBlock = new MaterialPropertyBlock();

                    target.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetVector(PropertyName, value);
                    target.SetPropertyBlock(_propertyBlock);
                }
                else
                {
                    if (Application.isPlaying)
                        target.material.SetVector(PropertyName, value);
                    else
                        target.sharedMaterial.SetVector(PropertyName, value);
                }
#if UNITY_EDITOR

                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(target);
                }
#endif
            }
        }
    }
}