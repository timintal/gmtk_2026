using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyTweens
{
    [Serializable]
    public class TweenMaterialColorProperty : ColorTween<Renderer>
    {
        [ExposeInEditor, ShaderProperty(ShaderPropertyType.Color)]
        public string PropertyName;

        [ExposeInEditor(fontSize: 10)]
        public bool usePropertyBlock;

        MaterialPropertyBlock _propertyBlock;

        protected override Color Property
        {
            get
            {
                if (Application.isPlaying && usePropertyBlock)
                {
                    if (_propertyBlock == null)
                        _propertyBlock = new MaterialPropertyBlock();

                    target.GetPropertyBlock(_propertyBlock);
                    return _propertyBlock.GetColor(PropertyName);
                }

                if (Application.isPlaying)
                    return target.material.GetColor(PropertyName);
                else
                    return target.sharedMaterial.GetColor(PropertyName);
            }
            set
            {
                if (usePropertyBlock)
                {
                    if (_propertyBlock == null)
                        _propertyBlock = new MaterialPropertyBlock();

                    target.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor(PropertyName, value);
                    target.SetPropertyBlock(_propertyBlock);
                }
                else
                {
                    if (Application.isPlaying)
                        target.material.SetColor(PropertyName, value);
                    else
                        target.sharedMaterial.SetColor(PropertyName, value);
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