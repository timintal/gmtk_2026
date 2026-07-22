using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace EasyTweens
{
    [Serializable, TweenCategoryOverride("UI")]
    public class TweenColorProperty : ColorTween<Graphic>
    {
        protected override Color Property
        {
            get => target.color;
            set
            {
                target.color = value;
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
