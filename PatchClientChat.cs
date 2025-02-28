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

            if (messageParts[0] == "/teamcolor")
            {
                if (messageParts.Length < 3)
                {
                    Plugin.chat.AddChatMessage($"You must specify a team color and a hex color to set them to.<br>/teamcolor (red/blue/r/b) #abc123");
                    return false;
                }

                if (messageParts[1].InvariantEqualsIgnoreCase("blue") || messageParts[1].InvariantEqualsIgnoreCase("b"))
                {
                    Color? colorToSet = HexColorStringToColorObject(messageParts[2].Replace("#", ""));
                    if (!colorToSet.HasValue)
                    {
                        Plugin.chat.AddChatMessage($"You did not specify a color correctly.<br>/teamcolor (red/blue/r/b) #abc123");
                        return false;
                    }
                    SetTeamToVisualColor(PlayerTeam.Blue, colorToSet.Value);
                    Plugin.configTeamBlueColor.Value = messageParts[2].Replace("#", "");
                    Plugin.chat.AddChatMessage($"Blue team was set to color #{colorToSet.Value}");
                    return false;
                }
                
                if (messageParts[1].InvariantEqualsIgnoreCase("red") || messageParts[1].InvariantEqualsIgnoreCase("r"))
                {
                    Color? colorToSet = HexColorStringToColorObject(messageParts[2].Replace("#", ""));
                    if (!colorToSet.HasValue)
                    {
                        Plugin.chat.AddChatMessage($"You did not specify a color correctly.<br>/teamcolor (red/blue/r/b) #abc123");
                        return false;
                    }
                    SetTeamToVisualColor(PlayerTeam.Red, colorToSet.Value);
                    Plugin.configTeamRedColor.Value = messageParts[2].Replace("#", "");
                    Plugin.chat.AddChatMessage($"Red team was set to color #{colorToSet.Value}");
                    return false;
                }
                
                Plugin.chat.AddChatMessage($"You must specify a team color and a hex color to set them to.<br>/teamcolor (red/blue/r/b) #abc123");
                return false;
            }

            return true;
        }
    }

    public static Color? HexColorStringToColorObject(string hexColorString)
    {
        bool parsed = ColorUtility.TryParseHtmlString($"#{hexColorString}#", out Color parsedColor);
        return parsed ? parsedColor : null; 
        
    }

    public static void SetTeamToVisualColor(PlayerTeam team, Color color)
    {
        if (team == PlayerTeam.Blue)
        {
            Plugin.uiMinimap.teamBlueColor = color;
            Color selfColor = color;
            selfColor.a = 0.5f;
            Plugin.uiMinimap.teamBlueSelfColor = selfColor;
            Plugin.uiGameState.blueScoreLabel.style.color = color;
            
        }
        else
        {
            Plugin.uiMinimap.teamRedColor = color;
            Color selfColor = color;
            selfColor.a = 0.5f;
            Plugin.uiMinimap.teamRedSelfColor = selfColor;
            Plugin.uiGameState.redScoreLabel.style.color = color;
        }
    }

    [HarmonyPatch(typeof(UIChat), nameof(UIChat.WrapPlayerUsername))]
    public class PatchUIChatWrapPlayerUsername
    {
        [HarmonyPrefix]
        public static bool Prefix(UIChat __instance, string __result, Player player)
        {
            Plugin.Log.LogInfo($"Patch: UIChat.WrapPlayerUsername (Prefix) was called.");
            string username = player.Username.Value.ToString();
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
            
            __result = $"<b><color=#{colorHex}>{username}</color></b>";
            
            return false;
        }
    }
    
    [HarmonyPatch(typeof(UIChat), nameof(UIChat.WrapInTeamColor))]
    public class WrapInTeamColor
    {
        [HarmonyPrefix]
        public static bool Prefix(UIChat __instance, string __result, PlayerTeam team, string message)
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