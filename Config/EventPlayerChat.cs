using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Commands;
using Chat_Logger_GoldKingZ.Config;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;

namespace Chat_Logger_GoldKingZ;

public class SayText2
{
    public HookResult OnSayText2(CCSPlayerController? player, string message, bool TeamChat)
    {
        if (!player.IsValid())return HookResult.Continue;

        if (!MainPlugin.Instance.g_Main.Player_Data.TryGetValue(player, out var handle)) return HookResult.Continue;
        
        if((DateTime.Now - handle.EventPlayerChat).TotalSeconds < 0.1)return HookResult.Continue;
        handle.EventPlayerChat = DateTime.Now;
        
        Helper.LogLocally(player, message, TeamChat);
        Helper.LogMySql(player, message, TeamChat);
        Helper.LogDiscord(player, message, TeamChat);

        return HookResult.Continue;
    }
}