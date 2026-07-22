using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyTweens
{
    [Serializable,TweenCategoryOverride("TMP")]
    public class TweenTMPMaterialVector4Property : Vector4Tween<TMP_Text>
    {
        [ExposeInEditor, ShaderProperty(ShaderPropertyType.Vector, ShaderPropertyType.Texture)]
        public string PropertyName;

        protected override Vector4 Property
        {
            get
            {
                return target.fontSharedMaterial.GetVector(PropertyName);
            }
            set
            {
                target.fontSharedMaterial.SetVector(PropertyName, value);
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