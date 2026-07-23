using FFS.Libraries.StaticEcs;

namespace Code.Common.View
{
    public class BindViewSystem : ISystem
    {
        public void Update()
        {
            W.Query<All<ViewLink, NeedBindView>>().For(entity =>
            {
                entity.Delete<NeedBindView>();
                entity.Read<ViewLink>().View.Bind(entity);
            });
        }
    }
}