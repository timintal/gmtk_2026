using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace EasyTweens
{
    internal static class TweenEditorTypeCache
    {
        private static List<AlternativeEditor> _alternativeEditors;
        private static List<Type> _tweenTypes;
        private static List<string> _availableTweenNames;

        [InitializeOnLoadMethod]
        private static void RegisterInvalidation()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            AssemblyReloadEvents.afterAssemblyReload += Invalidate;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                Invalidate();
            }
        }

        private static void Invalidate()
        {
            _alternativeEditors = null;
            _tweenTypes = null;
            _availableTweenNames = null;
        }

        public static IReadOnlyList<AlternativeEditor> AlternativeEditors => EnsureAlternativeEditors();

        public static IReadOnlyList<Type> TweenTypes => EnsureTweenTypes();

        public static IReadOnlyList<string> AvailableTweenNames => EnsureAvailableTweenNames();

        public static string GetTweenName(Type type)
        {
            EnsureTweenTypes();
            var index = _tweenTypes.IndexOf(type);
            return index >= 0 ? _availableTweenNames[index] : type.Name;
        }

        private static List<AlternativeEditor> EnsureAlternativeEditors()
        {
            if (_alternativeEditors != null)
            {
                return _alternativeEditors;
            }

            _alternativeEditors = new List<AlternativeEditor>();
            var types = Assembly.GetAssembly(typeof(TweenBase)).GetTypes();
            foreach (var type in types)
            {
                var customAttribute = (UseCustomEditorAttribute)type
                    .GetCustomAttribute(typeof(UseCustomEditorAttribute), true);
                if (customAttribute == null)
                {
                    continue;
                }

                var assets = AssetDatabase.FindAssets("t:visualtreeasset " + customAttribute.Type);
                if (assets.Length == 0)
                {
                    continue;
                }

                _alternativeEditors.Add(new AlternativeEditor
                {
                    tweenType = type,
                    tweenAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                        AssetDatabase.GUIDToAssetPath(assets[0]))
                });
            }

            return _alternativeEditors;
        }

        private static List<Type> EnsureTweenTypes()
        {
            if (_tweenTypes != null)
            {
                return _tweenTypes;
            }

            // TypeCache spans every loaded assembly, so tweens written in game code show up
            // in the add menu next to the ones shipped with the package.
            _tweenTypes = TypeCache.GetTypesDerivedFrom<TweenBase>()
                .Where(t => !t.IsAbstract)
                .ToList();

            _availableTweenNames = new List<string>(_tweenTypes.Count);
            foreach (var type in _tweenTypes)
            {
                _availableTweenNames.Add(RetrieveTweenName(type));
            }

            return _tweenTypes;
        }

        private static List<string> EnsureAvailableTweenNames()
        {
            EnsureTweenTypes();
            return _availableTweenNames;
        }

        private static string RetrieveTweenName(Type type)
        {
            var tweenName = type.Name;
            tweenName = tweenName.Replace("Tween", "");

            for (int i = 1; i < tweenName.Length; i++)
            {
                if (char.IsUpper(tweenName[i]) && tweenName[i - 1] != ' ')
                {
                    tweenName = tweenName.Insert(i, " ");
                }
            }

            var categoryOverride = type.GetCustomAttribute<TweenCategoryOverrideAttribute>();
            Type[] genericArguments = type.BaseType.GetGenericArguments();

            if (categoryOverride != null)
            {
                tweenName = categoryOverride.OverrideCategory + "/" + tweenName;
            }
            else if (genericArguments.Length > 0)
            {
                tweenName = genericArguments[0].Name + "/" + tweenName;
            }
            else
            {
                tweenName = "Other/" + tweenName;
            }

            return tweenName;
        }
    }
}
