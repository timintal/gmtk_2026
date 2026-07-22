using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyTweens
{
    [Serializable]
    public class TweenMaterialFloatProperty : FloatTween<Renderer>
    {
        [ExposeInEditor, ShaderProperty(ShaderPropertyType.Float, ShaderPropertyType.Range)]
        public string PropertyName;

        [ExposeInEditor(fontSize: 10, tooltip: "If true, the property will be set on the MaterialPropertyBlock of the renderer. This is useful if you want to change the property of a shared material without affecting other renderers using the same material.")]
        public bool usePropertyBlock;

        MaterialPropertyBlock _propertyBlock;

        protected override float Property
        {
            get
            {
                if (usePropertyBlock)
                {
                    if (_propertyBlock == null)
                        _propertyBlock = new MaterialPropertyBlock();

                    target.GetPropertyBlock(_propertyBlock);
                    return _propertyBlock.GetFloat(PropertyName);
                }

                if (Application.isPlaying)
                    return target.material.GetFloat(PropertyName);
                else
                    return target.sharedMaterial.GetFloat(PropertyName);
            }
            set
            {
                if (usePropertyBlock)
                {
                    if (_propertyBlock == null)
                        _propertyBlock = new MaterialPropertyBlock();

                    target.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetFloat(PropertyName, value);
                    target.SetPropertyBlock(_propertyBlock);
                }
                else
                {
                    if (Application.isPlaying)
                        target.material.SetFloat(PropertyName, value);
                    else
                        target.sharedMaterial.SetFloat(PropertyName, value);
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