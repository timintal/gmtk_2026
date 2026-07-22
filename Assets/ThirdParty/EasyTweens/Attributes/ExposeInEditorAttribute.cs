using System;

namespace EasyTweens
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ExposeInEditorAttribute : Attribute
    {
        private readonly int fontSize;
        private string tooltip;
        private readonly int order;

        public int FontSize => fontSize;
        public string Tooltip => tooltip;

        public int Order => order;

        public ExposeInEditorAttribute(int fontSize = 12, string tooltip = null, int order = 0)
        {
            this.fontSize = fontSize;
            this.tooltip = tooltip;
            this.order = order;
        }
    }
}