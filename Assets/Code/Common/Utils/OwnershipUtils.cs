namespace Code.Common.Utils
{
    public static class OwnershipUtils
    {
        public static void AddChild(this W.Entity parent, W.Entity child)
        {
            if (parent.Has<W.Links<Children>>())
            {
                ref var links = ref parent.Ref<W.Links<Children>>();
                links.TryAdd(child.AsLink<Children>());
            }
            else
            {
                ref var links = ref parent.Add<W.Links<Children>>();
                links.Add(child.AsLink<Children>());
            }
            child.Set(new W.Link<Owner>(parent));
        }
        
        public static void RemoveChild(this W.Entity parent, W.Entity child)
        {
            if (parent.Has<W.Links<Children>>())
            {
                ref var links = ref parent.Ref<W.Links<Children>>();
                links.TryRemove(child.AsLink<Children>());
            }
            child.Delete<W.Link<Owner>>();
        }
    }
}