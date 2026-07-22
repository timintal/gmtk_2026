using System;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyTweens
{
#if UNITY_2021_1_OR_NEWER
    [Serializable]
    public class TweenMaterialIntProperty : IntTween<Renderer>
    {
        [ExposeInEditor, ShaderProperty(ShaderPropertyType.Int)]
        public string PropertyName;

        [ExposeInEditor(fontSize: 10,
            tooltip:
            "If true, the property will be set on the MaterialPropertyBlock of the renderer. This is useful if you want to change the property of a shared material without affecting other renderers using the same material.")]
        public bool usePropertyBlock;

        MaterialPropertyBlock _propertyBlock;

        protected override int Property
        {
            get
            {
                if (usePropertyBlock)
                {
                    if (_propertyBlock == null)
                        _propertyBlock = new MaterialPropertyBlock();

                    target.GetPropertyBlock(_propertyBlock);
                    return _propertyBlock.GetInt(PropertyName);
                }

                if (Application.isPlaying)
                    return target.material.GetInt(PropertyName);
                else
                    return target.sharedMaterial.GetInt(PropertyName);
            }
            set
            {
                if (usePropertyBlock)
                {
                    if (_propertyBlock == null)
                        _propertyBlock = new MaterialPropertyBlock();

                    target.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetInt(PropertyName, value);
                    target.SetPropertyBlock(_propertyBlock);
                }
                else
                {
                    if (Application.isPlaying)
                        target.material.SetInt(PropertyName, value);
                    else
                        target.sharedMaterial.SetInt(PropertyName, value);
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
#endif
}