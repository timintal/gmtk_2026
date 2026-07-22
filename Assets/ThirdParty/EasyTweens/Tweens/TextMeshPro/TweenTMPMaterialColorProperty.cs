using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace EasyTweens
{
    [Serializable,TweenCategoryOverride("TMP")]
    public class TweenTMPMaterialColorProperty : ColorTween<TMP_Text>
    {
        [ExposeInEditor, ShaderProperty(ShaderPropertyType.Color)]
        public string PropertyName;

        protected override Color Property
        {
            get
            {
                return target.fontSharedMaterial.GetColor(PropertyName);

            }
            set
            {
                target.fontSharedMaterial.SetColor(PropertyName, value);
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