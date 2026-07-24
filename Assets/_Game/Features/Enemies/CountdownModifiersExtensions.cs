using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace _Game.Features.Enemies
{
    public static class CountdownModifiersExtensions
    {
        public static bool HasModifier<T>(this W.Entity entity) where T : struct, IComponentOrTag
        {
            if (!entity.Has<W.Links<CountdownModifiers>>())
                return false;

            foreach (var modifier in entity.Read<W.Links<CountdownModifiers>>())
            {
                if (modifier.Value.TryUnpack<WT>(out var modifierEntity) && modifierEntity.Has<T>())
                {
                    return true;
                }
            }

            return false;
        }
        
        public static bool TryGetModifier<T>(this W.Entity entity, out T modifierEntity) where T : struct, IComponent
        {
            modifierEntity = default;

            if (!entity.Has<W.Links<CountdownModifiers>>())
                return false;

            foreach (var modifier in entity.Read<W.Links<CountdownModifiers>>())
            {
                if (modifier.Value.TryUnpack<WT>(out var modEntity) && modEntity.Has<T>())
                {
                    modifierEntity = modEntity.Read<T>();
                    return true;
                }
            }

            return false;
        }
        
        public static List<W.Entity> GetAllModifierEntities(this W.Entity entity) 
        {
            var modifiers = new List<W.Entity>();

            if (!entity.Has<W.Links<CountdownModifiers>>())
                return modifiers;
            
            foreach (var modifier in entity.Read<W.Links<CountdownModifiers>>())
            {
                if (modifier.Value.TryUnpack<WT>(out var modEntity))
                {
                    modifiers.Add(modEntity);
                }
            }

            return modifiers;
        }
    }
}