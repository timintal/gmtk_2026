#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTweens
{
    [CustomEditor(typeof(ShearImage))]
    public class ShearImageEditor : ImageEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            
            var root = new VisualElement();
        
            var defaultInspector = new IMGUIContainer(() => { OnInspectorGUI(); });
            root.Add(defaultInspector);
        
            var targetSprite = (ShearImage) this.target;
        
            var shearX = new Vector2Field("Shear (px)");
            shearX.value = targetSprite.shear;
            shearX.RegisterValueChangedCallback(evt =>
            {
                targetSprite.shear = evt.newValue;
                targetSprite.SetVerticesDirty();
                EditorUtility.SetDirty(targetSprite);
            });
            root.Add(shearX);
        
            var imgSize = targetSprite.rectTransform.rect.size;
            var absolutePivot = new Vector2Field("Shear Pivot (absolute)");
            absolutePivot.value = new Vector2(imgSize.x * targetSprite.shearPivot.x, imgSize.y * targetSprite.shearPivot.y);
            absolutePivot.SetEnabled(false);
            
            var shearPivot = new Vector2Field("Shear Pivot (relative)");
            shearPivot.value = targetSprite.shearPivot;
            shearPivot.RegisterValueChangedCallback(evt =>
            {
                targetSprite.shearPivot = evt.newValue;
                targetSprite.SetVerticesDirty();
                absolutePivot.value = new Vector2(imgSize.x * targetSprite.shearPivot.x, imgSize.y * targetSprite.shearPivot.y);
                EditorUtility.SetDirty(targetSprite);
            });
            root.Add(shearPivot);
            root.Add(absolutePivot);
        
            return root;
        
        
        }
    }
}
#endif