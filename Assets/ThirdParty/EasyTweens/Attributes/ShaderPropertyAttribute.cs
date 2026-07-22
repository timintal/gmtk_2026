using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace EasyTweens
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ShaderPropertyAttribute : Attribute
    {
        public List<ShaderPropertyType> Types;

        public ShaderPropertyAttribute(ShaderPropertyType type, params ShaderPropertyType[] types)
        {
            Types = new List<ShaderPropertyType>(types.Length + 1) { type };
            Types.AddRange(types);
        }
    }
}