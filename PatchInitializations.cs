using HarmonyLib;

namespace ToasterTeamColors
{
    public static class PatchInitializations
    {
        [HarmonyPatch(typeof(UIChat), "Start")]
        class PatchUIChatStart
        {
            static void Prefix()
            {
                Plugin.Log.LogInfo($"Patch: UIChatStart (Prefix) was called.");
                Plugin.chat = UIChat.Instance;
            }
        }

        [HarmonyPatch(typeof(UIGameState), nameof(UIGameState.Show))]
        public static class PatchUIGameStateShow
        {
            [HarmonyPrefix]
            public static void Prefix(UIGameState __instance)
            {
                Plugin.Log.LogInfo($"Patch: UIGameState.Show (Prefix) was called.");
                Plugin.uiGameState = __instance;
            }
        }
        
        [HarmonyPatch(typeof(UIMinimap), nameof(UIMinimap.Show))]
        public static class PatchUIMinimapShow
        {
            [HarmonyPrefix]
            public static void Prefix(UIMinimap __instance)
            {
                Plugin.Log.LogInfo($"Patch: UIMinimap.Show (Prefix) was called.");
                Plugin.uiMinimap = __instance;
            }
        }
    }
    
}

