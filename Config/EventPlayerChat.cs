using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Commands;
using Chat_Logger_GoldKingZ.Config;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;

namespace Chat_Logger_GoldKingZ;

public class PlayerChat
{
    public HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo info, bool TeamChat)
	{
        var g_Main = ChatLoggerGoldKingZ.Instance.g_Main;
        if (!player.IsValid())return HookResult.Continue;

        Helper.AddPlayerInGlobals(player);

        var eventmessage = info.ArgString;
        eventmessage = eventmessage.TrimStart('"');
        eventmessage = eventmessage.TrimEnd('"');
        if (string.IsNullOrWhiteSpace(eventmessage)) return HookResult.Continue;

        string trimmedMessageStart = eventmessage.TrimStart();
        string message = trimmedMessageStart.TrimEnd();
        
        Helper.LogLocally(player, message, TeamChat);
        Helper.LogMySql(player, message, TeamChat);
        Helper.LogDiscord(player, message, TeamChat);

        return HookResult.Continue;
    }
}