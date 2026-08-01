using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace EasyTweens
{
    public class ShearImage : Image
    {
        public Vector2 shear = Vector2.zero;
        public Vector2 shearPivot = Vector2.zero;

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);

            UIVertex tempVertex = new UIVertex();
            var rect = rectTransform.rect;
            Vector2 absoluteShearPivot = new Vector2(rect.x + shearPivot.x * rect.width, rect.y + shearPivot.y * rect.height);
            
            for (int i = 0; i < toFill.currentVertCount; i++)
            {
                toFill.PopulateUIVertex(ref tempVertex, i);
                Vector3 pos = tempVertex.position;
                pos.x += shear.x * (pos.y - absoluteShearPivot.y) / rect.height;
                pos.y += shear.y * (pos.x - absoluteShearPivot.x) / rect.width;
                tempVertex.position = pos;
                toFill.SetUIVertex(tempVertex, i);
            }
        }
    }
}