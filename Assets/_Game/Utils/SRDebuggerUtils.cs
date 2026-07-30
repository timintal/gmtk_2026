using SRDebugger;

namespace _Game.Utils
{
    public static class SRDebuggerUtils
    {
        public static void AddOptionContainer(object container)
        {
#if !DISABLE_SRDEBUGGER            
            SRDebug.Instance.AddOptionContainer(container);
#endif            
        }
        
        public static void RemoveOptionContainer(object container)
        {
#if !DISABLE_SRDEBUGGER
            if (SRDebug.Instance != null)
            {
                SRDebug.Instance.RemoveOptionContainer(container);
            }
#endif
        }

        public static OptionDefinition AddDynamicOption(string category, string name, System.Action action)
        {
#if !DISABLE_SRDEBUGGER
            var option = OptionDefinition.FromMethod(name, action, category);
            SRDebug.Instance.AddOption(option);
            return option;
#else
            return null;
#endif
        }

        public static void RemoveDynamicOption(OptionDefinition option)
        {
#if !DISABLE_SRDEBUGGER
            SRDebug.Instance?.RemoveOption(option);
#endif
        }
    }

}