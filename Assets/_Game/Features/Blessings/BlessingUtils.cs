namespace _Game.Features.Blessings
{
    public static class BlessingUtils
    {
        public static void DiscardBlessing(this W.Entity e)
        {
            if (!e.Has<Blessing>()) return;

            e.Delete<Hand>();
            e.Delete<DrawPile>();
            e.Set<DiscardPile>();
        }
        
        public static void DrawBlessing(this W.Entity e)
        {
            if (!e.Has<Blessing>()) return;

            e.Delete<DiscardPile>();
            e.Delete<DrawPile>();
            e.Set<Hand>();
        }
        
        public static void PutBlessingInDrawPile(this W.Entity e)
        {
            if (!e.Has<Blessing>()) return;

            e.Delete<Hand>();
            e.Delete<DiscardPile>();
            e.Set<DrawPile>();
        }
    }
}