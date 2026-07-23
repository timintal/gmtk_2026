using UnityEngine;
using UnityEngine.Rendering;

namespace Code.Common.View.ChildViews
{
    public partial class SortingGroupView : EntityChildView 
    {
        [SerializeField] private SortingGroup _sortingGroup;
        
        [SerializeField] private string _draggingLayer;

        string _originalSortingLayerName;
        
        void PostBind()
        {
            _originalSortingLayerName = _sortingGroup.sortingLayerName;
        }
        
        public void SetDragged(bool isDragged)
        {
            _sortingGroup.sortingLayerName = isDragged ? _draggingLayer : _originalSortingLayerName;
        }
    }
}