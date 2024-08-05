using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Commands;
using Chat_Logger_GoldKingZ.Config;

namespace Chat_Logger_GoldKingZ;

public class PlayerChat
{
    public HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo info, bool TeamChat)
	{
        if (player == null || !player.IsValid)return HookResult.Continue;

        var playerid = player.SteamID;
        if (!Globals.TeamChat.ContainsKey(playerid))
        {
            if(TeamChat)
            {
                Globals.TeamChat.Add(playerid, true);
            }else
            {
                Globals.TeamChat.Add(playerid, false);
            }
        }
        if (Globals.TeamChat.ContainsKey(playerid))
        {
            if(TeamChat)
            {
                Globals.TeamChat[playerid] = true;
            }else
            {
                Globals.TeamChat[playerid] = false;
            }
        }
        var eventmessage = info.GetArg(1);
        string Fpath = Path.Combine(Configs.Shared.CookiesModule!,"../../plugins/Chat-Logger-GoldKingZ/logs/");
        string fileName = DateTime.Now.ToString(Configs.GetConfigData().Text_DateFormat) + ".txt";
        string Tpath = Path.Combine(Configs.Shared.CookiesModule!,"../../plugins/Chat-Logger-GoldKingZ/logs/") + $"{fileName}";

        var vplayername = player.PlayerName;
        var steamId2 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId2 : "InvalidSteamID";
        var steamId3 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId3 : "InvalidSteamID";
        var steamId32 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId32.ToString() : "InvalidSteamID";
        var steamId64 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId64.ToString() : "InvalidSteamID";
        var GetIpAddress = player.IpAddress;
        var ipAddress = GetIpAddress?.Split(':')[0] ?? "InValidIpAddress";
        

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
            if(Configs.GetConfigData().Text_PrivateTeamMessagesOnly && !Globals.TeamChat[playerid])return HookResult.Continue;
            string chatteam = Globals.TeamChat[playerid] ? "[TEAM]" : "[ALL]";
            if (Globals.TextExclude.ContainsKey(playerid))return HookResult.Continue;
            if (Globals.TextIncude.ContainsKey(playerid))return HookResult.Continue;
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
            
            if(!string.IsNullOrEmpty(replacerlog))
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
            if(Configs.GetConfigData().Discord_PrivateTeamMessagesOnly && !Globals.TeamChat[playerid])return HookResult.Continue;
            string chatteam = Globals.TeamChat[playerid] ? "[TEAM]" : "[ALL]";
            if (Globals.DiscordExclude.ContainsKey(playerid))return HookResult.Continue;
            if (Globals.DiscordIncude.ContainsKey(playerid))return HookResult.Continue;
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
}