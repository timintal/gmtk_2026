using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common.View
{
    public class CleanupViewSystem : ISystem
    {
        public void Update()
        {
            W.Query<All<ViewLink, Destroyed>>().For(entity =>
            {
                var view = entity.Read<ViewLink>().View;
                view.Unbind();
                Object.Destroy(view.gameObject);
                entity.Delete<ViewLink>();
            });

            foreach (var e in W.Query<All<ViewLink, NeedCleanupView>, None<Destroyed>>().Entities())
            {
                e.Read<ViewLink>().View.Unbind();
                Object.Destroy(e.Read<ViewLink>().View.gameObject);
                
                e.Delete<NeedCleanupView>();
                e.Delete<ViewLink>();
                if (e.Has<ViewPrefab>())
                {
                    e.Delete<ViewPrefab>();
                }
            }
        }
    }
}