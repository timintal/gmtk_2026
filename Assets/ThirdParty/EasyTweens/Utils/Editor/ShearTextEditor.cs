using TMPro.EditorUtilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace EasyTweens
{
    [CustomEditor(typeof(ShearText))]
    public class ShearTextEditor : TMP_EditorPanelUI
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
        
            var defaultInspector = new IMGUIContainer(() => { base.OnInspectorGUI(); });
            root.Add(defaultInspector);
        
            var text = (ShearText) target;
        
            var shearX = new Vector2Field("Shear (px)");
            shearX.value = text.shear;
            shearX.RegisterValueChangedCallback(evt =>
            {
                text.shear = evt.newValue;
                text.havePropertiesChanged = true;
                EditorUtility.SetDirty(text);
            });
            root.Add(shearX);

            var textSize = text.TextSize();
            var absolutePivot = new Vector2Field("Shear Pivot (absolute)");
            absolutePivot.value = new Vector2(textSize.x * text.shearPivot.x, textSize.y * text.shearPivot.y);
            absolutePivot.SetEnabled(false);
            
            var shearPivot = new Vector2Field("Shear Pivot (relative)");
            shearPivot.value = text.shearPivot;
            shearPivot.RegisterValueChangedCallback(evt =>
            {
                text.shearPivot = evt.newValue;
                text.havePropertiesChanged = true;
                EditorUtility.SetDirty(text);
                absolutePivot.value = new Vector2(textSize.x * text.shearPivot.x, textSize.y * text.shearPivot.y);
            });
            root.Add(shearPivot);
            root.Add(absolutePivot);

            return root;
        }
    }
    
    
}