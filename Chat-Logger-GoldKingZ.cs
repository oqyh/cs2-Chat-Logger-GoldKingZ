using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Chat_Logger_GoldKingZ.Config;

namespace Chat_Logger_GoldKingZ;

public class ChatLoggerGoldKingZ : BasePlugin
{
    public override string ModuleName => "Chat Logger (Log Chat To Text Or Discord WebHook)";
    public override string ModuleVersion => "1.0.9";
    public override string ModuleAuthor => "Gold KingZ";
    public override string ModuleDescription => "https://github.com/oqyh";
    private readonly PlayerChat _PlayerChat = new();
	
	

    public override void Load(bool hotReload)
    {
        Configs.Load(ModuleDirectory);
        Configs.Shared.CookiesModule = ModuleDirectory;
        AddCommandListener("say", OnPlayerChat, HookMode.Post);
		AddCommandListener("say_team", OnPlayerChatTeam, HookMode.Post);
        RegisterEventHandler<EventPlayerConnectFull>(OnEventPlayerConnectFull);
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
    }
    public void OnMapStart(string Map)
    {
        if(Configs.GetConfigData().Text_AutoDeleteLogsMoreThanXdaysOld > 0)
        {
            string Fpath = Path.Combine(ModuleDirectory,"../../plugins/Chat-Logger-GoldKingZ/logs");
            Helper.DeleteOldFiles(Fpath, "*" + ".txt", TimeSpan.FromDays(Configs.GetConfigData().Text_AutoDeleteLogsMoreThanXdaysOld));
        }
    }
    private HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo info)
	{
        if (player == null || !player.IsValid)return HookResult.Continue;
        _PlayerChat.OnPlayerChat(player, info, false);
        return HookResult.Continue;
    }
    private HookResult OnPlayerChatTeam(CCSPlayerController? player, CommandInfo info)
	{
        if (player == null || !player.IsValid)return HookResult.Continue;
        _PlayerChat.OnPlayerChat(player, info, true);
        return HookResult.Continue;
    }
    
    public HookResult OnEventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event == null)return HookResult.Continue;
        var player = @event.Userid;

        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return HookResult.Continue;
        var playerid = player.SteamID;

        if (!string.IsNullOrEmpty(Configs.GetConfigData().Text_IncludeFlagsMessagesOnly) && Helper.IsPlayerInGroupPermission(player, Configs.GetConfigData().Text_IncludeFlagsMessagesOnly))
        {
            if (!Globals.TextIncude.ContainsKey(playerid))
            {
                Globals.TextIncude.Add(playerid, true);
            }

            if (Globals.TextIncude.ContainsKey(playerid))
            {
                Globals.TextIncude[playerid] = true;
            }
        }

        if (!string.IsNullOrEmpty(Configs.GetConfigData().Text_ExcludeFlagsMessages) && Helper.IsPlayerInGroupPermission(player, Configs.GetConfigData().Text_ExcludeFlagsMessages))
        {
            if (!Globals.TextExclude.ContainsKey(playerid))
            {
                Globals.TextExclude.Add(playerid, true);
            }

            if (Globals.TextExclude.ContainsKey(playerid))
            {
                Globals.TextExclude[playerid] = true;
            }
        }

        if (!string.IsNullOrEmpty(Configs.GetConfigData().Discord_IncludeFlagsMessagesOnly) && Helper.IsPlayerInGroupPermission(player, Configs.GetConfigData().Discord_IncludeFlagsMessagesOnly))
        {
            if (!Globals.DiscordIncude.ContainsKey(playerid))
            {
                Globals.DiscordIncude.Add(playerid, true);
            }

            if (Globals.DiscordIncude.ContainsKey(playerid))
            {
                Globals.DiscordIncude[playerid] = true;
            }
        }

        if (!string.IsNullOrEmpty(Configs.GetConfigData().Discord_ExcludeFlagsMessages) && Helper.IsPlayerInGroupPermission(player, Configs.GetConfigData().Discord_ExcludeFlagsMessages))
        {
            if (!Globals.DiscordExclude.ContainsKey(playerid))
            {
                Globals.DiscordExclude.Add(playerid, true);
            }

            if (Globals.DiscordExclude.ContainsKey(playerid))
            {
                Globals.DiscordExclude[playerid] = true;
            }
        }

        return HookResult.Continue;
    }

    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;
        var player = @event.Userid;
        
        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return HookResult.Continue;
        var playerid = player.SteamID;

        if (Globals.Client_Text1.ContainsKey(playerid)) Globals.Client_Text1.Remove(playerid);
        if (Globals.Client_Text2.ContainsKey(playerid)) Globals.Client_Text2.Remove(playerid);
        if (Globals.TextIncude.ContainsKey(playerid)) Globals.TextIncude.Remove(playerid);
        if (Globals.TextExclude.ContainsKey(playerid)) Globals.TextExclude.Remove(playerid);
        if (Globals.DiscordIncude.ContainsKey(playerid)) Globals.DiscordIncude.Remove(playerid);
        if (Globals.DiscordExclude.ContainsKey(playerid)) Globals.DiscordExclude.Remove(playerid);
        if (Globals.TeamChat.ContainsKey(playerid)) Globals.TeamChat.Remove(playerid);

        return HookResult.Continue;
    }
    public void OnMapEnd()
    {
        Helper.ClearVariables();
    }
    public override void Unload(bool hotReload)
    {
        Helper.ClearVariables();
    }
}