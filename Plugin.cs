using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using HarmonyLib.Tools;

namespace ToasterTeamColors;

[BepInPlugin("pw.stellaric.plugins.toasterteamcolors", "Toaster Team Colors", "1.0.0.0")]
public class Plugin : BasePlugin
{
    // the "configurable" things
    private readonly Harmony _harmony = new Harmony("pw.stellaric.plugins.toasterteamcolors");
    
    // plugin managers
    public static new ManualLogSource Log;
    public static UIChat chat;
    public static UIMinimap uiMinimap;
    public static UIGameState uiGameState;
    public static UITeamSelect uiTeamSelect;
    public static UIScoreboard uiScoreboard;
    public static UIAnnouncement uiAnnouncement;

    public static PlayerManager playerManager;

    // config values
    public static ConfigEntry<string> configTeamBlueColor;
    public static ConfigEntry<string> configTeamRedColor;
    
    public override void Load()
    {
        HarmonyFileLog.Enabled = true;
        
        // Load config values
        configTeamBlueColor = Config.Bind("General",      // The section under which the option is shown
            "teamBlueColor",  // The key of the configuration option in the configuration file
            "1d69e5", // The default value
            "The HEX color of the Blue team"); // Description of the option to show in the config file
        configTeamRedColor = Config.Bind("General",      // The section under which the option is shown
            "teamRedColor",  // The key of the configuration option in the configuration file
            "e21a18", // The default value
            "The HEX color of the Red team"); // Description of the option to show in the config file
        
        // Plugin startup logic
        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded! Patching methods...");
        _harmony.PatchAll();
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is all patched! Patched methods:");
        
        var originalMethods = Harmony.GetAllPatchedMethods();
        foreach (var method in originalMethods)
        {
            Log.LogInfo($" - {method.DeclaringType.FullName}.{method.Name}");
        }
    }
}