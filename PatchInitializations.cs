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
                PatchClientChat.SetColorsOnEverything();
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
                // PatchClientChat.SetColorsOnEverything();
            }
        }

        [HarmonyPatch(typeof(UITeamSelect), nameof(UITeamSelect.Show))]
        public static class PatchUITeamSelectShow
        {
            [HarmonyPrefix]
            public static void Prefix(UITeamSelect __instance)
            {
                Plugin.Log.LogInfo($"Patch: UITeamSelect.Show (Prefix) was called.");
                Plugin.uiTeamSelect = __instance;
                // PatchClientChat.SetColorsOnEverything();
            }
        }

        [HarmonyPatch(typeof(UIScoreboardController), nameof(UIScoreboardController.Start))]
        public static class PatchUIScoreboardControllerStart
        {
            [HarmonyPostfix]
            public static void Postfix(UIScoreboardController __instance)
            {
                Plugin.Log.LogInfo($"Patch: UIScoreboardController.Start (Postfix) was called.");
                Plugin.uiScoreboard = __instance.uiScoreboard;
            }
        }

        [HarmonyPatch(typeof(UIAnnouncementController), nameof(UIAnnouncementController.Start))]
        public static class PatchUIAnnouncementControllerStart
        {
            [HarmonyPostfix]
            public static void Postfix(UIAnnouncementController __instance)
            {
                Plugin.Log.LogInfo($"Patch: UIAnnouncementController.Start (Postfix) was called.");
                Plugin.uiAnnouncement = __instance.uiAnnouncement;
            }
        }
    }
    
}

