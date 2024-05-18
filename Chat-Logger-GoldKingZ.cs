using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using Chat_Logger_GoldKingZ.Config;

namespace Chat_Logger_GoldKingZ;


public class ChatLoggerGoldKingZ : BasePlugin
{
    public override string ModuleName => "Chat Logger (Log Chat To Text Or Discord WebHook)";
    public override string ModuleVersion => "1.0.7";
    public override string ModuleAuthor => "Gold KingZ";
    public override string ModuleDescription => "https://github.com/oqyh";
	
	

    public override void Load(bool hotReload)
    {
        Configs.Load(ModuleDirectory);
        Configs.Shared.CookiesModule = ModuleDirectory;
        RegisterEventHandler<EventPlayerChat>(OnEventPlayerChat, HookMode.Post);
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
    public HookResult OnEventPlayerChat(EventPlayerChat @event, GameEventInfo info)
    {
        if(@event == null)return HookResult.Continue;
        var eventplayer = @event.Userid;
        var eventmessage = @event.Text;
        var eventteamonly = @event.Teamonly;
        var player = Utilities.GetPlayerFromUserid(eventplayer);
        
        if (player == null || !player.IsValid)return HookResult.Continue;
        string Fpath = Path.Combine(ModuleDirectory,"../../plugins/Chat-Logger-GoldKingZ/logs/");
        string fileName = DateTime.Now.ToString(Configs.GetConfigData().Text_DateFormat) + ".txt";
        string Tpath = Path.Combine(ModuleDirectory,"../../plugins/Chat-Logger-GoldKingZ/logs/") + $"{fileName}";

        var vplayername = player.PlayerName;
        var steamId2 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId2 : "InvalidSteamID";
        var steamId3 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId3 : "InvalidSteamID";
        var steamId32 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId32.ToString() : "InvalidSteamID";
        var steamId64 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId64.ToString() : "InvalidSteamID";
        var GetIpAddress = player.IpAddress;
        var ipAddress = GetIpAddress?.Split(':')[0] ?? "InValidIpAddress";
        var playerid = player.SteamID;

        if (string.IsNullOrWhiteSpace(eventmessage)) return HookResult.Continue;
        string trimmedMessageStart = eventmessage.TrimStart();
        string message = trimmedMessageStart.TrimEnd();

        if (!Globals.Client_Text1.ContainsKey(playerid))
        {
            Globals.Client_Text1.Add(playerid, message);
        }
        if (!Globals.Client_Text2.ContainsKey(playerid))
        {
            Globals.Client_Text2.Add(playerid, string.Empty);
        }

        if (Globals.Client_Text1.ContainsKey(playerid))
        {
            Globals.Client_Text1[playerid] = Globals.Client_Text2[playerid];
        }
        if (Globals.Client_Text2.ContainsKey(playerid))
        {
            Globals.Client_Text2[playerid] = message;
        }

        if(Configs.GetConfigData().Text_EnableLoggingMessages)
        {
            if(Configs.GetConfigData().Text_PrivateTeamMessagesOnly && !eventteamonly)return HookResult.Continue;
            string chatteam = eventteamonly ? "[TEAM]" : "[ALL]";
            if (!string.IsNullOrEmpty(Configs.GetConfigData().Text_ExcludeFlagsMessages) && Helper.IsPlayerInGroupPermission(player,Configs.GetConfigData().Text_ExcludeFlagsMessages))return HookResult.Continue;
            if (!string.IsNullOrEmpty(Configs.GetConfigData().Text_IncludeFlagsMessagesOnly) && !Helper.IsPlayerInGroupPermission(player,Configs.GetConfigData().Text_IncludeFlagsMessagesOnly))return HookResult.Continue;
            if(!string.IsNullOrEmpty(Configs.GetConfigData().Text_ExcludeMessageContains) && Helper.IsStringValid(message)) return HookResult.Continue;
            if (Configs.GetConfigData().Text_ExcludeMessageContainsLessThanXLetters > 0 && Helper.CountLetters(message) <= Configs.GetConfigData().Text_ExcludeMessageContainsLessThanXLetters) return HookResult.Continue;
            
            if(Configs.GetConfigData().Text_ExcludeMessageDuplicate && Globals.Client_Text2[playerid] == Globals.Client_Text1[playerid]) return HookResult.Continue;
            string Time = DateTime.Now.ToString(Configs.GetConfigData().Text_TimeFormat);
            string Date = DateTime.Now.ToString(Configs.GetConfigData().Text_DateFormat);
            var replacerlog = !string.IsNullOrEmpty(Configs.GetConfigData().Text_MessageFormat)
            ? Helper.ReplaceMessages(
                Configs.GetConfigData().Text_MessageFormat,  
                Time,  
                Date,  
                message,  
                vplayername,  
                steamId2, 
                steamId3, 
                steamId32.ToString(), 
                steamId64.ToString(), 
                ipAddress.ToString(), 
                chatteam ?? "[----]"
            ): string.Empty;
            
            if(!string.IsNullOrEmpty(Configs.GetConfigData().Text_MessageFormat))
            {
                if(!Directory.Exists(Fpath))
                {
                    Directory.CreateDirectory(Fpath);
                }

                if(!File.Exists(Tpath))
                {
                    using (File.Create(Tpath)) { }
                }
                try
                {
                    File.AppendAllLines(Tpath, new[]{replacerlog});
                }catch
                {

                }
            }
            
        }

        if(Configs.GetConfigData().Discord_EnableLoggingMessagesOnMode != 0)
        {
            if(Configs.GetConfigData().Discord_PrivateTeamMessagesOnly && !eventteamonly)return HookResult.Continue;
            string chatteam = eventteamonly ? "[TEAM]" : "[ALL]";
            if (!string.IsNullOrEmpty(Configs.GetConfigData().Discord_ExcludeFlagsMessages) && Helper.IsPlayerInGroupPermission(player,Configs.GetConfigData().Discord_ExcludeFlagsMessages))return HookResult.Continue;
            if (!string.IsNullOrEmpty(Configs.GetConfigData().Discord_IncludeFlagsMessagesOnly) && !Helper.IsPlayerInGroupPermission(player,Configs.GetConfigData().Discord_IncludeFlagsMessagesOnly))return HookResult.Continue;
            if(!string.IsNullOrEmpty(Configs.GetConfigData().Discord_ExcludeMessageContains) && Helper.IsStringValid(message)) return HookResult.Continue;
            if (Configs.GetConfigData().Discord_ExcludeMessageContainsLessThanXLetters > 0 && Helper.CountLetters(message) <= Configs.GetConfigData().Discord_ExcludeMessageContainsLessThanXLetters) return HookResult.Continue;
            if(Configs.GetConfigData().Discord_ExcludeMessageDuplicate && Globals.Client_Text2[playerid] == Globals.Client_Text1[playerid]) return HookResult.Continue;

            string Time = DateTime.Now.ToString(Configs.GetConfigData().Discord_TimeFormat);
            string Date = DateTime.Now.ToString(Configs.GetConfigData().Discord_DateFormat);
            int hostPort = ConVar.Find("hostport")!.GetPrimitiveValue<int>();
            var replacerDiscord = !string.IsNullOrEmpty(Configs.GetConfigData().Discord_MessageFormat)
            ? Helper.ReplaceMessages(
                Configs.GetConfigData().Discord_MessageFormat,  
                Time,  
                Date,  
                message,  
                vplayername,  
                steamId2, 
                steamId3, 
                steamId32.ToString(), 
                steamId64.ToString(), 
                ipAddress.ToString(), 
                chatteam ?? "[----]"
            ): string.Empty;
            
            if(!string.IsNullOrEmpty(replacerDiscord))
            {
                if(Configs.GetConfigData().Discord_EnableLoggingMessagesOnMode == 1)
                {
                    Task.Run(() =>
                    {
                        _ = Helper.SendToDiscordWebhookNormal(Configs.GetConfigData().Discord_WebHookURL, replacerDiscord);
                    });
                }else if(Configs.GetConfigData().Discord_EnableLoggingMessagesOnMode == 2)
                {
                    Task.Run(() =>
                    {
                        _ = Helper.SendToDiscordWebhookNameLink(Configs.GetConfigData().Discord_WebHookURL, replacerDiscord, playerid.ToString(), vplayername);
                    });
                }else if(Configs.GetConfigData().Discord_EnableLoggingMessagesOnMode == 3)
                {
                    Task.Run(() =>
                    {
                        _ = Helper.SendToDiscordWebhookNameLinkWithPicture(Configs.GetConfigData().Discord_WebHookURL, replacerDiscord, playerid.ToString(), vplayername);
                    });
                }else if(Configs.GetConfigData().Discord_EnableLoggingMessagesOnMode == 4)
                {
                    Task.Run(() =>
                    {
                        _ = Helper.SendToDiscordWebhookNameLinkWithPicture2(Configs.GetConfigData().Discord_WebHookURL, replacerDiscord, playerid.ToString(), vplayername);
                    });
                }else if(Configs.GetConfigData().Discord_EnableLoggingMessagesOnMode == 5)
                {
                    Task.Run(() =>
                    {
                        string serverIp = Helper.GetServerPublicIPAsync().Result;
                        _ = Helper.SendToDiscordWebhookNameLinkWithPicture3(Configs.GetConfigData().Discord_WebHookURL, replacerDiscord, playerid.ToString(), vplayername, $"{serverIp}:{hostPort}");
                    });
                }
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

        Globals.Client_Text1.Remove(playerid);
        Globals.Client_Text2.Remove(playerid);

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