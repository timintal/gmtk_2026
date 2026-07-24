using Code.Common.View;
using UnityEngine;

namespace _Game.Features.Run.View
{
    public class EnemiesContainer : ResourceMonoBehaviour<EnemiesContainer>
    {
        [SerializeField] Transform _container;
        
        public Transform Container => _container;
    }
}