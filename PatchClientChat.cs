using System;
using System.IO;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ToasterTeamColors;

public class PatchClientChat
{
    [HarmonyPatch(typeof(UIChat), nameof(UIChat.Client_SendClientChatMessage))]
    class PatchUIChatClientSendClientChatMessage
    {
        [HarmonyPrefix]
        static bool Prefix(UIChat __instance, string message)
        {
            Plugin.Log.LogInfo($"Patch: UIChat.Client_SendClientChatMessage (Prefix) was called.");
            Plugin.chat = __instance;
            string[] messageParts = message.Split(' ');

            if (messageParts[0].InvariantEqualsIgnoreCase("/teamcolor"))
            {
                if (messageParts.Length < 3)
                {
                    Plugin.chat.AddChatMessage($"You must specify a team color and a hex color to set them to.<br>/teamcolor (red/blue/r/b) #abc123");
                    return false;
                }

                if (messageParts[1].InvariantEqualsIgnoreCase("blue") || messageParts[1].InvariantEqualsIgnoreCase("b"))
                {
                    string potentialHexColor = messageParts[2].Replace("#", "");
                    Plugin.Log.LogInfo($"Potential hex color to set: {potentialHexColor}");
                    Color? colorToSet = HexColorStringToColorObject(potentialHexColor);
                    if (!colorToSet.HasValue)
                    {
                        Plugin.chat.AddChatMessage($"You did not specify a color correctly.<br>/teamcolor (red/blue/r/b) #abc123");
                        return false;
                    }
                    SetTeamToVisualColor(PlayerTeam.Blue, colorToSet.Value);
                    Plugin.configTeamBlueColor.Value = potentialHexColor;
                    Plugin.Log.LogInfo($"Blue team config color was set to color #{potentialHexColor}");
                    Plugin.chat.AddChatMessage($"Blue team was set to color <color=#{potentialHexColor}>#{colorToSet.Value}</color>");
                    return false;
                }
                
                if (messageParts[1].InvariantEqualsIgnoreCase("red") || messageParts[1].InvariantEqualsIgnoreCase("r"))
                {
                    string potentialHexColor = messageParts[2].Replace("#", "");
                    Plugin.Log.LogInfo($"Potential hex color to set: {potentialHexColor}");
                    Color? colorToSet = HexColorStringToColorObject(potentialHexColor);
                    if (!colorToSet.HasValue)
                    {
                        Plugin.chat.AddChatMessage($"You did not specify a color correctly.<br>/teamcolor (red/blue/r/b) #abc123");
                        return false;
                    }
                    SetTeamToVisualColor(PlayerTeam.Red, colorToSet.Value);
                    Plugin.configTeamRedColor.Value = potentialHexColor;
                    Plugin.Log.LogInfo($"Red team config color was set to color #{potentialHexColor}");
                    Plugin.chat.AddChatMessage($"Red team was set to color <color=#{potentialHexColor}>#{colorToSet.Value}</color>");
                    return false;
                }
                
                Plugin.chat.AddChatMessage($"You must specify a team color and a hex color to set them to.<br>/teamcolor (red/blue/r/b) #abc123");
                return false;
            }

            if (messageParts[0].InvariantEqualsIgnoreCase("/resetteamcolors"))
            {
                // reset blue team color
                Color? colorToSetBlue = HexColorStringToColorObject("1d69e5"); // blue color
                SetTeamToVisualColor(PlayerTeam.Blue, colorToSetBlue.Value);
                Plugin.configTeamBlueColor.Value = "1d69e5";
                
                // reset red team color
                Color? colorToSetRed = HexColorStringToColorObject("e21a18"); // red color
                SetTeamToVisualColor(PlayerTeam.Red, colorToSetRed.Value);
                Plugin.configTeamRedColor.Value = "e21a18";
                
                Plugin.chat.AddChatMessage($"Team colors have been reset.");
                return false;
            }

            return true;
        }
    }

    public static Color? HexColorStringToColorObject(string hexColorString)
    {
        // this had a # at the end of the string for some reason, I think the AI autocomplete did that but I think that was the issue
        bool parsed = ColorUtility.TryParseHtmlString($"#{hexColorString}", out Color parsedColor);
        return parsed ? parsedColor : null; 
    }

    public static void SetTeamToVisualColor(PlayerTeam team, Color color)
    {
        if (team == PlayerTeam.Blue)
        {
            if (Plugin.uiMinimap != null)
            {
                Plugin.uiMinimap.teamBlueColor = color;
                Color selfColor = color;
                selfColor.a = 0.5f;
                Plugin.uiMinimap.teamBlueSelfColor = selfColor;
            }

            if (Plugin.uiGameState != null)
            {
                Plugin.uiGameState.blueScoreLabel.style.color = color;
            }
            
            if (Plugin.uiTeamSelect != null)
            {
                Plugin.uiTeamSelect.teamBlueButton.style.backgroundColor = color;
            }
            
            if (Plugin.uiScoreboard != null)
            {
                Plugin.uiScoreboard.teamBlueContainer.style.backgroundColor = color;
            }
            
            if (Plugin.uiAnnouncement != null)
            {
                Plugin.uiAnnouncement.blueTeamScoreAnnouncement.style.color = color;
            }
        }
        else
        {
            if (Plugin.uiMinimap != null)
            {
                Plugin.uiMinimap.teamRedColor = color;
                Color selfColor = color;
                selfColor.a = 0.5f;
                Plugin.uiMinimap.teamRedSelfColor = selfColor;
            }

            if (Plugin.uiGameState != null)
            {
                Plugin.uiGameState.redScoreLabel.style.color = color;
            }

            if (Plugin.uiTeamSelect != null)
            {
                Plugin.uiTeamSelect.teamRedButton.style.backgroundColor = color;
            }

            if (Plugin.uiScoreboard != null)
            {
                Plugin.uiScoreboard.teamRedContainer.style.backgroundColor = color;
            }
            
            if (Plugin.uiAnnouncement != null)
            {
                Plugin.uiAnnouncement.redTeamScoreAnnouncement.style.color = color;
            }
        }
    }

    public static void SetColorsOnEverything()
    {
        try
        {
            SetTeamToVisualColor(PlayerTeam.Blue, HexColorStringToColorObject(Plugin.configTeamBlueColor.Value).Value);
            SetTeamToVisualColor(PlayerTeam.Red, HexColorStringToColorObject(Plugin.configTeamRedColor.Value).Value);
        }
        catch (System.InvalidOperationException e)
        {
            Plugin.Log.LogInfo($"Error setting everything team colors: {e.Message}");
        }
        
    }

    [HarmonyPatch(typeof(UIChat), nameof(UIChat.WrapPlayerUsername))]
    public class PatchUIChatWrapPlayerUsername
    {
        [HarmonyPrefix]
        public static bool Prefix(UIChat __instance, out string __result, Player player)
        {
            Plugin.Log.LogInfo($"Patch: UIChat.WrapPlayerUsername (Prefix) was called.");
            string username = player.Username.Value.ToString();
            string playerNumber = player.Number.Value.ToString();
            string colorHex = "";
            switch (player.Team.Value)
            {
                case PlayerTeam.Blue:
                    colorHex = Plugin.configTeamBlueColor.Value.ToString();
                    break;
                case PlayerTeam.Red:
                    colorHex = Plugin.configTeamRedColor.Value.ToString();
                    break;
                case PlayerTeam.Spectator:
                    colorHex = "dddddd";
                    break;
                case PlayerTeam.None:
                    colorHex = "dddddd";
                    break;
            }
            
            __result = $"<b><color=#{colorHex}>#{playerNumber} {username}</color></b>";
            
            return false;
        }
    }
    
    [HarmonyPatch(typeof(UIChat), nameof(UIChat.WrapInTeamColor))]
    public class WrapInTeamColor
    {
        [HarmonyPrefix]
        public static bool Prefix(UIChat __instance, out string __result, PlayerTeam team, string message)
        {
            Plugin.Log.LogInfo($"Patch: UIChat.WrapInTeamColor (Prefix) was called.");
            string colorHex = "";
            switch (team)
            {
                case PlayerTeam.Blue:
                    colorHex = Plugin.configTeamBlueColor.Value.ToString();
                    break;
                case PlayerTeam.Red:
                    colorHex = Plugin.configTeamRedColor.Value.ToString();
                    break;
                case PlayerTeam.Spectator:
                    colorHex = "787879";
                    break;
                case PlayerTeam.None:
                    colorHex = "787879";
                    break;
            }
            
            __result = $"<b><color=#{colorHex}>{message}</color></b>";
            
            return false;
        }
    }
}