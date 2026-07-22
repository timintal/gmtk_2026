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
        }
    }
}