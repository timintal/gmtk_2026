using Code.Common.View;
using UnityEngine;

namespace _Game.Features.Enemies
{
    public partial class EnemyView : EntityChildView
    {
        [SerializeField] private Transform _visualRoot;
        
        public void SetVisual(GameObject visual)
        {
            if (_visualRoot.childCount > 0)
            {
                foreach (Transform child in _visualRoot)
                {
                    Destroy(child.gameObject);
                }
            }

            if (visual != null)
            {
                Instantiate(visual, _visualRoot);
            }
        }
    }
}