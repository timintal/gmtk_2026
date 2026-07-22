using FFS.Libraries.StaticEcs;

namespace Code.Common.View
{

    public class InitializeViewSystem : ISystem
{
    public void Update()
    {
        W.Query<All<NeedInitializeView>>().For(static (W.Entity e, in ViewLink view) =>
        {
            e.Delete<NeedInitializeView>();
            view.View.Bind(e);
        });
    }
}
}