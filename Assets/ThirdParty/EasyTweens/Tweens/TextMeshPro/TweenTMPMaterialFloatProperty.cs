using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyTweens
{
    [Serializable,TweenCategoryOverride("TMP")]
    public class TweenTMPMaterialFloatProperty : FloatTween<TMP_Text>
    {
        [ExposeInEditor, ShaderProperty(ShaderPropertyType.Float, ShaderPropertyType.Range)]
        public string PropertyName;

        MaterialPropertyBlock _propertyBlock;

        protected override float Property
        {
            get
            {
                return target.fontSharedMaterial.GetFloat(PropertyName);
            }
            set
            {

                target.fontSharedMaterial.SetFloat(PropertyName, value);
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