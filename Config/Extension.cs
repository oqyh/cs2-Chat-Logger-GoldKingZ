using CounterStrikeSharp.API.Core;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;


namespace Chat_Logger_GoldKingZ;

public static class Extension
{
    public static bool IsValid([NotNullWhen(true)] this CCSPlayerController? player, bool checkbot = false, bool checkHLTV = false)
    {
        if (player == null || !player.IsValid)
            return false;

        if (!checkbot && player.IsBot)
            return false;

        if (!checkHLTV && player.IsHLTV)
            return false;

        return true;
    }


    public static string ReplaceMessages(this string MessageFormate, string date, string time, string PlayerName, string message, string chatteam, string SteamId, string SteamId3, string SteamId32, string SteamId64, string IPaddress)
    {
        var replacedMessage = MessageFormate
                            .Replace("{DATE}", date)
                            .Replace("{TIME}", time)
                            .Replace("{PLAYER_NAME}", PlayerName.ToString())
                            .Replace("{PLAYER_MESSAGE}", message)
                            .Replace("{PLAYER_TEAM}", chatteam)
                            .Replace("{PLAYER_STEAMID}", SteamId.ToString())
                            .Replace("{PLAYER_STEAMID3}", SteamId3.ToString())
                            .Replace("{PLAYER_STEAMID32}", SteamId32.ToString())
                            .Replace("{PLAYER_STEAMID64}", SteamId64.ToString())
                            .Replace("{PLAYER_IP}", IPaddress.ToString());
        return replacedMessage;
    }
}