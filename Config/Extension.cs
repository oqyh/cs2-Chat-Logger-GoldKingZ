using CounterStrikeSharp.API.Core;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;


namespace Chat_Logger_GoldKingZ;

public static class Extension
{
    public static bool IsValid([NotNullWhen(true)] this CCSPlayerController? player, bool IncludeBots = false, bool IncludeHLTV = false)
    {
        if (player == null || !player.IsValid)
            return false;

        if (!IncludeBots && player.IsBot)
            return false;

        if (!IncludeHLTV && player.IsHLTV)
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
    
    private const ulong Steam64Offset = 76561197960265728UL;
    public static (string steam2, string steam3, string steam32, string steam64)GetPlayerSteamID(this ulong steamId64)
    {
        uint id32 = (uint)(steamId64 - Steam64Offset);
        var steam32 = id32.ToString();
        uint y = id32 & 1;
        uint z = id32 >> 1;
        var steam2 = $"STEAM_0:{y}:{z}";
        var steam3 = $"[U:1:{steam32}]";
        var steam64 = steamId64.ToString();
        return (steam2, steam3, steam32, steam64);
    }
}