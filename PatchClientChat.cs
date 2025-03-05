using System;
using System.IO;
using System.Linq;
using System.Text;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Linq;
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
                // this does not work
                // teamBlueButton is a UnityEngine.UIElements.Button
                Plugin.uiTeamSelect.teamBlueButton.style.backgroundColor = color;
                
                // this also does not work
                var buttonBackground = Plugin.uiTeamSelect.teamBlueButton.Q<VisualElement>("unity-button-background");
                if (buttonBackground != null)
                {
                    buttonBackground.style.backgroundColor = new StyleColor(color);
                }
                else
                {
                    Debug.LogWarning("Button background element not found!");
                }
                
                // Option 1: Use StyleColor consistently
                Plugin.uiTeamSelect.teamBlueButton.style.backgroundColor = new StyleColor(color);
    
                // // Option 2: Try setting the background color with USS class
                // Plugin.uiTeamSelect.teamBlueButton.AddToClassList("blue-team-button");
                //
                // // Create a StyleSheet programmatically
                // var sheet = ScriptableObject.CreateInstance<StyleSheet>();
                // var rule = new StyleRule();
                // rule.selectors.Add(new StyleSelector(".blue-team-button", StyleSelectorType.Class));
                // rule.properties.Add(new StyleProperty("background-color", new StyleColor(color).ToString()));
                // sheet.rules.Add(rule);
                //
                // // Apply the stylesheet
                // Plugin.uiTeamSelect.teamBlueButton.styleSheets.Add(sheet);
    
                // Option 3: Try manipulating the visual hierarchy more specifically
                var buttonVisualElement = Plugin.uiTeamSelect.teamBlueButton;
                for(int i = 0; i < buttonVisualElement.Children().Count(); i++)
                {
                    Plugin.Log.LogInfo($"Adding stylecolor to {buttonVisualElement.m_Children._items[i].name}");
                    buttonVisualElement.m_Children._items[i].style.backgroundColor = new StyleColor(color);
                }
                
                // For UIElements with custom styling
                var visualElement = Plugin.uiTeamSelect.teamBlueButton as VisualElement;
                if (visualElement != null)
                {
                    // Try to find all nested visual elements and set their colors
                    var allChildren = visualElement.Query<VisualElement>().ToList();
                    foreach (var child in allChildren)
                    {
                        child.style.backgroundColor = new StyleColor(color);
                    }
                }
                
                // Option 4: Force a visual update
                Plugin.uiTeamSelect.teamBlueButton.style.backgroundColor = new StyleColor(color);
                Plugin.uiTeamSelect.teamBlueButton.MarkDirtyRepaint();
                
            }
            
            if (Plugin.uiScoreboard != null)
            {
                Plugin.uiScoreboard.teamBlueContainer.style.backgroundColor = color;
            }
            
            if (Plugin.uiAnnouncement != null)
            {
                var textElement = Plugin.uiAnnouncement.blueTeamScoreAnnouncement.Q<Label>();
                if (textElement != null)
                {
                    textElement.style.color = new StyleColor(color);
                }
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
                // this does not work
                Plugin.uiTeamSelect.teamRedButton.style.backgroundColor = color;
                
                // this also does not work
                var buttonBackground = Plugin.uiTeamSelect.teamRedButton.Q<VisualElement>("unity-button-background");
                if (buttonBackground != null)
                {
                    buttonBackground.style.backgroundColor = new StyleColor(color);
                }
                else
                {
                    Debug.LogWarning("Button background element not found!");
                }

                ExportVisualElementHierarchy(Plugin.uiTeamSelect.teamRedButton, "teamRedButton");
            }

            if (Plugin.uiScoreboard != null)
            {
                Plugin.uiScoreboard.teamRedContainer.style.backgroundColor = color;
            }
            
            if (Plugin.uiAnnouncement != null)
            {
                // Plugin.uiAnnouncement.redTeamScoreAnnouncement.style.color = color;
                var textElement = Plugin.uiAnnouncement.redTeamScoreAnnouncement.Q<Label>();
                if (textElement != null)
                {
                    textElement.style.color = new StyleColor(color);
                }
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

    // These don't work because they are modifying it on the server-host
    // [HarmonyPatch(typeof(UIChat), nameof(UIChat.WrapPlayerUsername))]
    // public class PatchUIChatWrapPlayerUsername
    // {
    //     [HarmonyPrefix]
    //     public static bool Prefix(UIChat __instance, out string __result, Player player)
    //     {
    //         Plugin.Log.LogInfo($"Patch: UIChat.WrapPlayerUsername (Prefix) was called.");
    //         string username = player.Username.Value.ToString();
    //         string playerNumber = player.Number.Value.ToString();
    //         string colorHex = "";
    //         switch (player.Team.Value)
    //         {
    //             case PlayerTeam.Blue:
    //                 colorHex = Plugin.configTeamBlueColor.Value.ToString();
    //                 break;
    //             case PlayerTeam.Red:
    //                 colorHex = Plugin.configTeamRedColor.Value.ToString();
    //                 break;
    //             case PlayerTeam.Spectator:
    //                 colorHex = "dddddd";
    //                 break;
    //             case PlayerTeam.None:
    //                 colorHex = "dddddd";
    //                 break;
    //         }
    //         
    //         __result = $"<b><color=#{colorHex}>#{playerNumber} {username}</color></b>";
    //         
    //         return false;
    //     }
    // }
    
    // [HarmonyPatch(typeof(UIChat), nameof(UIChat.WrapInTeamColor))]
    // public class WrapInTeamColor
    // {
    //     [HarmonyPrefix]
    //     public static bool Prefix(UIChat __instance, out string __result, PlayerTeam team, string message)
    //     {
    //         Plugin.Log.LogInfo($"Patch: UIChat.WrapInTeamColor (Prefix) was called.");
    //         string colorHex = "";
    //         switch (team)
    //         {
    //             case PlayerTeam.Blue:
    //                 colorHex = Plugin.configTeamBlueColor.Value.ToString();
    //                 break;
    //             case PlayerTeam.Red:
    //                 colorHex = Plugin.configTeamRedColor.Value.ToString();
    //                 break;
    //             case PlayerTeam.Spectator:
    //                 colorHex = "787879";
    //                 break;
    //             case PlayerTeam.None:
    //                 colorHex = "787879";
    //                 break;
    //         }
    //         
    //         __result = $"<b><color=#{colorHex}>{message}</color></b>";
    //         
    //         return false;
    //     }
    // }

    // this will change the messages coming into the client
    [HarmonyPatch(typeof(UIChat), nameof(UIChat.AddChatMessage))]
    public static class AddChatMessage
    {
        [HarmonyPrefix]
        public static bool Prefix(UIChat __instance, string content)
        {
            Plugin.Log.LogInfo($"Patch: UIChat.AddChatMessage (Prefix) was called.");
            Plugin.Log.LogInfo($"Message: {content}");
            if (content.Contains("color=#E51717") || content.Contains("color=#175CE6"))
            {
                string contentWithReplacedColors = content
                    .Replace("color=#E51717", $"color=#{Plugin.configTeamRedColor.Value.ToString()}")
                    .Replace("color=#175CE6", $"color=#{Plugin.configTeamBlueColor.Value.ToString()}");
                __instance.AddChatMessage(contentWithReplacedColors);
                return false;
            }

            return true;
        }
    }
    
    public static void ExportVisualElementHierarchy(VisualElement root, string filePath)
    {
        StringBuilder sb = new StringBuilder();
        TraverseVisualElement(root, sb, 0);

        // Write the hierarchy to a file
        System.IO.File.WriteAllText(filePath, sb.ToString());
        Debug.Log($"VisualElement hierarchy exported to: {filePath}");
    }

    private static void TraverseVisualElement(VisualElement element, StringBuilder sb, int depth)
    {
        // Indent based on depth
        sb.AppendLine($"{new string(' ', depth * 2)}- {element.name} ({element.GetType().Name})");

        // Log style properties (optional)
        sb.AppendLine($"{new string(' ', (depth + 1) * 2)}Style: {element.resolvedStyle}");

        // Recursively traverse children
        for (int i = 0; i < element.Children().Count(); i++)
        {
            TraverseVisualElement(element.m_Children._items[i], sb, depth + 1);
        }
    }
}