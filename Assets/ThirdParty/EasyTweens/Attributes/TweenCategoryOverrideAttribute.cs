using System;

namespace EasyTweens
{
    public class TweenCategoryOverrideAttribute : Attribute
    {
        private readonly string _overrideCategory;

        public string OverrideCategory => _overrideCategory;

        public TweenCategoryOverrideAttribute(string overrideCategory)
        {
            _overrideCategory = overrideCategory;
        }
    }
}