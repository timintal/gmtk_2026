using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common
{
    public struct CollisionEnter2D : IComponent { public Collision2D Value; }
    public struct CollisionExit2D : IComponent { public Collision2D Value; }
    
    public struct TriggerEnter2D : IComponent { public Collider2D Value; }
    public struct TriggerExit2D : IComponent { public Collider2D Value; }
    
    public struct OtherEntity : ILinkType{}
}