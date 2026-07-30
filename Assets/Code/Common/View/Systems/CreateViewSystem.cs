using FFS.Libraries.StaticEcs;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Code.Common.View
{
    
    public class CreateViewSystem : ISystem
    {
        private readonly IObjectResolver _resolver;
        public CreateViewSystem(IObjectResolver resolver)
        {
            _resolver = resolver;
        }
        
        public void Update()
        {
            foreach (var entity in W.Query<All<ViewPrefab>,None<ViewLink>>().Entities())
            {
                var prefab = entity.Read<ViewPrefab>().Prefab;
                Transform parent = null;
                if (entity.Has<ParentTransform>())
                {
                    parent = entity.Read<ParentTransform>().Value;
                    entity.Delete<ParentTransform>();
                }
                
                var view = _resolver.Instantiate(prefab, parent);
                view.Bind(entity);
                entity.Set(new ViewLink { View = view });
                if (entity.Has<Position>())
                {
                    var position = entity.Read<Position>().Value;
                    ViewTransformUtility.WritePosition(
                        view.transform,
                        position,
                        ViewTransformUtility.ReadPreservedZ(view.transform));
                }
            }
        }
    }
}