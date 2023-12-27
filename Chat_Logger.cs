using System.Text;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace Chat_Logger;

public class ChatLoggerConfig : BasePluginConfig
{
    [JsonPropertyName("SendLogToText")] public bool SendLogToText { get; set; } = false;
    [JsonPropertyName("LogChatFormat")] public string LogChatFormat { get; set; } = "[{TIME}] {TEAM} [{PLAYERNAME}] {MESSAGE} (SteamID: {STEAMID})";
    [JsonPropertyName("LogFileFormat")] public string LogFileFormat { get; set; } = ".txt";
    [JsonPropertyName("LogFileDateFormat")] public string LogFileDateFormat { get; set; } = "MM-dd-yyyy";
    [JsonPropertyName("LogInsideFileTimeFormat")] public string LogInsideFileTimeFormat { get; set; } = "HH:mm:ss";

    [JsonPropertyName("SendLogToWebHook")] public bool SendLogToWebHook { get; set; } = false;
    [JsonPropertyName("WebHookURL")] public string WebHookURL { get; set; } = "https://discord.com/api/webhooks/XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
    [JsonPropertyName("LogDiscordChatFormat")] public string LogDiscordChatFormat { get; set; } = "[{DATE} - {TIME}] {TEAM} {MESSAGE} (IpAddress: {IP})";
}

public class ChatLogger : BasePlugin, IPluginConfig<ChatLoggerConfig>
{
    public override string ModuleName => "Chat Logger";
    public override string ModuleVersion => "1.0.1";
    public override string ModuleAuthor => "Gold KingZ";
    public override string ModuleDescription => "Log Any Chat Discord Or Log Text";
    public ChatLoggerConfig Config { get; set; } = new ChatLoggerConfig();
    private Dictionary<int, bool> BteamChat = new Dictionary<int, bool>();

    public void OnConfigParsed(ChatLoggerConfig config)
    {
        Config = config;
    }
    
    public override void Load(bool hotReload)
    {
        AddCommandListener("say", OnPlayerSayPublic);
        AddCommandListener("say_team", OnPlayerSayTeam);
    }

    private HookResult OnPlayerSayPublic(CCSPlayerController? player, CommandInfo info)
	{
        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV || player.AuthorizedSteamID == null|| player.IpAddress == null)return HookResult.Continue;
        
        string? chatteam = null;

        if (player.UserId.HasValue)
        {
            BteamChat[player.UserId.Value] = false;
            bool isTeamChat = BteamChat[player.UserId.Value];
            chatteam = isTeamChat ? "[TEAM]" : "[ALL]";
        }

        var message = info.GetArg(1);

        if (string.IsNullOrWhiteSpace(message)) return HookResult.Continue;

        string trimmedMessage = message.TrimStart();
        string Fpath = Path.Combine(ModuleDirectory,"../../plugins/Chat_Logger/logs/");
        string Time = DateTime.Now.ToString(Config.LogInsideFileTimeFormat);
        string Date = DateTime.Now.ToString(Config.LogFileDateFormat);
        string fileName = DateTime.Now.ToString(Config.LogFileDateFormat) + Config.LogFileFormat;
        string Tpath = Path.Combine(ModuleDirectory,"../../plugins/Chat_Logger/logs/") + $"{fileName}";

        var vplayername = player.PlayerName;
        var steamId2 = player.AuthorizedSteamID.SteamId2;
        var steamId3 = player.AuthorizedSteamID.SteamId3;
        var steamId32 = player.AuthorizedSteamID.SteamId32;
        var steamId64 = player.AuthorizedSteamID.SteamId64;
        var GetIpAddress = player.IpAddress;
        var ipAddress = GetIpAddress.Split(':')[0];

        if(Config.SendLogToText && !Directory.Exists(Fpath))
        {
            Directory.CreateDirectory(Fpath);
        }

        if(Config.SendLogToText && !File.Exists(Tpath))
        {
            File.Create(Tpath);
        }

        var replacerlog = ReplaceMessages(Config.LogChatFormat,  Time,  Date,  trimmedMessage,  vplayername,  steamId2, steamId3, steamId32.ToString(), steamId64.ToString(), ipAddress.ToString(), chatteam ?? "[----]");
        if (Config.SendLogToText && File.Exists(Tpath)) 
        {
            try
            {
                File.AppendAllLines(Tpath, new[]{replacerlog});
            }catch
            {
                Console.WriteLine("|||||||||||||||||||||||||||||| E R R O R ||||||||||||||||||||||||||||||");
                Console.WriteLine("[Error Cant Write] Please Give Chat_Logger.dll Permissions To Write");
                Console.WriteLine("|||||||||||||||||||||||||||||| E R R O R ||||||||||||||||||||||||||||||");
            }
        }

        var replacerlogDiscord = ReplaceMessages(Config.LogDiscordChatFormat,  Time,  Date,  trimmedMessage,  vplayername,  steamId2, steamId3, steamId32.ToString(), steamId64.ToString(), ipAddress.ToString(), chatteam ?? "[----]");
        if(Config.SendLogToWebHook)
        {
            Task.Run(() => SendToDiscordWebhook(Config.WebHookURL, replacerlog, steamId64.ToString(), vplayername));
        }

        return HookResult.Continue;
    }

    private HookResult OnPlayerSayTeam(CCSPlayerController? player, CommandInfo info)
	{
        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV || player.AuthorizedSteamID == null|| player.IpAddress == null)return HookResult.Continue;
        
        string? chatteam = null;

        if (player.UserId.HasValue)
        {
            BteamChat[player.UserId.Value] = true;
            bool isTeamChat = BteamChat[player.UserId.Value];
            chatteam = isTeamChat ? "[TEAM]" : "[ALL]";
        }

        var message = info.GetArg(1);

        if (string.IsNullOrWhiteSpace(message)) return HookResult.Continue;

        string trimmedMessage = message.TrimStart();
        string Fpath = Path.Combine(ModuleDirectory,"../../plugins/Chat_Logger/logs/");
        string Time = DateTime.Now.ToString(Config.LogInsideFileTimeFormat);
        string Date = DateTime.Now.ToString(Config.LogFileDateFormat);
        string fileName = DateTime.Now.ToString(Config.LogFileDateFormat) + Config.LogFileFormat;
        string Tpath = Path.Combine(ModuleDirectory,"../../plugins/Chat_Logger/logs/") + $"{fileName}";

        var vplayername = player.PlayerName;
        var steamId2 = player.AuthorizedSteamID.SteamId2;
        var steamId3 = player.AuthorizedSteamID.SteamId3;
        var steamId32 = player.AuthorizedSteamID.SteamId32;
        var steamId64 = player.AuthorizedSteamID.SteamId64;
        var GetIpAddress = player.IpAddress;
        var ipAddress = GetIpAddress.Split(':')[0];

        if(Config.SendLogToText && !Directory.Exists(Fpath))
        {
            Directory.CreateDirectory(Fpath);
        }

        if(Config.SendLogToText && !File.Exists(Tpath))
        {
            File.Create(Tpath);
        }

        var replacerlog = ReplaceMessages(Config.LogChatFormat,  Time,  Date,  trimmedMessage,  vplayername,  steamId2, steamId3, steamId32.ToString(), steamId64.ToString(), ipAddress.ToString(), chatteam ?? "[----]");
        if (Config.SendLogToText && File.Exists(Tpath)) 
        {
            try
            {
                File.AppendAllLines(Tpath, new[]{replacerlog});
            }catch
            {
                Console.WriteLine("|||||||||||||||||||||||||||||| E R R O R ||||||||||||||||||||||||||||||");
                Console.WriteLine("[Error Cant Write] Please Give Chat_Logger.dll Permissions To Write");
                Console.WriteLine("|||||||||||||||||||||||||||||| E R R O R ||||||||||||||||||||||||||||||");
            }
        }

        var replacerlogDiscord = ReplaceMessages(Config.LogDiscordChatFormat,  Time,  Date,  trimmedMessage,  vplayername,  steamId2, steamId3, steamId32.ToString(), steamId64.ToString(), ipAddress.ToString(), chatteam ?? "[----]");
        if(Config.SendLogToWebHook)
        {
            Task.Run(() => SendToDiscordWebhook(Config.WebHookURL, replacerlog, steamId64.ToString(), vplayername));
        }

        return HookResult.Continue;
    }

    private string ReplaceMessages(string Message, string time, string date, string message, string PlayerName, string SteamId, string SteamId3, string SteamId32, string SteamId64, string IPaddress, string chatteam)
    {
        var replacedMessage = Message
                                    .Replace("{TIME}", time)
                                    .Replace("{DATE}", date)
                                    .Replace("{MESSAGE}", message)
                                    .Replace("{PLAYERNAME}", PlayerName.ToString())
                                    .Replace("{STEAMID}", SteamId.ToString())
                                    .Replace("{STEAMID3}", SteamId3.ToString())
                                    .Replace("{STEAMID32}", SteamId32.ToString())
                                    .Replace("{STEAMID64}", SteamId64.ToString())
                                    .Replace("{IP}", IPaddress.ToString())
                                    .Replace("{TEAM}", chatteam);
        return replacedMessage;
    }

    static async Task SendToDiscordWebhook(string webhookUrl, string message, string steamUserId, string STEAMNAME)
    {
        string profileLink = GetSteamProfileLink(steamUserId);

        using (HttpClient client = new HttpClient())
        {
            var embed = new
            {
                description = message,
                author = new
                {
                    name = STEAMNAME,
                    url = profileLink
                }
            };

            var payload = new
            {
                embeds = new[] { embed }
            };

            var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(webhookUrl, content);

            if (response.IsSuccessStatusCode)
            {
                
            }
            else
            {
                Console.WriteLine($"Failed to send message. Status code: {response.StatusCode}, Response: {await response.Content.ReadAsStringAsync()}");
            }
        }
    }

    static string GetSteamProfileLink(string userId)
    {
        return $"https://steamcommunity.com/profiles/{userId}";
    }
    
}