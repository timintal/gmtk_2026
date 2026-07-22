using System;
using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace Code.Common 
{
    [Serializable] public struct Speed : IComponent { public float Value; }
    [Serializable] public struct RotationSpeed : IComponent { public float Value; }
    [Serializable] public struct Direction : IComponent { public Vector2 Value; }
    [Serializable] public struct Position : IComponent { public Vector2 Value; }
    [Serializable] public struct Rotation : IComponent { public float Value; }
    [Serializable] public struct MoveTarget : IComponent { public Vector2 Value; }
    [Serializable] public struct LerpTarget : IComponent { public Vector2 Value; }
    [Serializable] public struct ClickToMoveListener : ITag { }
    [Serializable] public struct MoveAlongDirection : ITag { }
}
