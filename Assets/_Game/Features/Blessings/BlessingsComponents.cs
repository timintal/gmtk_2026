using FFS.Libraries.StaticEcs;

namespace _Game.Features.Blessings
{
    public partial struct Blessing : ITag, ITrackableAdded, ITrackableDeleted {  }
    public partial struct DiceBlessing : ITag { }
    public partial struct PlayerBlessing : ITag { }
    public partial struct BlessingTarget : ITag {  }
    public partial struct BlessingValue : IComponent { public float Value; }
    public partial struct BlessingId : IComponent { public string Value; }
    
    public partial struct Hand : ITag, ITrackableAdded, ITrackableDeleted {  }
    public partial struct DrawPile : ITag, ITrackableAdded, ITrackableDeleted {  }
    public partial struct DiscardPile : ITag, ITrackableAdded, ITrackableDeleted {  }
    
    public partial struct AddValueBlessing : ITag {  }
    public partial struct MultiplyValueBlessing : ITag {  }
    public partial struct RerollBlessing : ITag {  }
    
    public partial struct OddBlessing : ITag {  }
    public partial struct EvenBlessing : ITag {  }
    
    public partial struct AffectSameValueDicesBlessing : ITag {  }
    public partial struct AffectAllBlessing : ITag {  }
}