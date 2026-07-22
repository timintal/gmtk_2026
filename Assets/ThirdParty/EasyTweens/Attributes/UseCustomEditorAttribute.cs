using System;

namespace EasyTweens
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UseCustomEditorAttribute : Attribute
    {
        private readonly string type;

        public string Type => type;

        public UseCustomEditorAttribute(string type)
        {
            this.type = type;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomTweenEditorAttribute : Attribute
    {
        private readonly Type type;

        public Type Type => type;

        public CustomTweenEditorAttribute(Type type)
        {
            this.type = type;
        }
    }
}