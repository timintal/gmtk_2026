using Code.Common;
using Code.Utils;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common.View
{
    
    public class CreateViewSystem : ISystem
    {
        public void Update()
        {
            W.Query<All<ViewPrefab>,None<ViewLink>>().For(entity =>
            {
                var prefab = entity.Read<ViewPrefab>().Prefab;
                Transform parent = null;
                if (entity.Has<ParentTransform>())
                {
                    parent = entity.Read<ParentTransform>().Value;
                    entity.Delete<ParentTransform>();
                }
                
                var view = Object.Instantiate(prefab, parent);
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
            });
        }
    }
}