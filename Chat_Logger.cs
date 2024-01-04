using System.Text;
using System.Drawing;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Config;

namespace Chat_Logger;

public class ChatLoggerConfig : BasePluginConfig
{
    [JsonPropertyName("ExcludeMessageGroups")] public string ExcludeMessageGroups { get; set; } = "#css/vip1,#css/vip2,#css/vip3";
    [JsonPropertyName("ExcludeMessageContains")] public string ExcludeMessageContains { get; set; } = "!./";
    [JsonPropertyName("ExcludeMessageContainsLessThanXLetters")] public int ExcludeMessageContainsLessThanXLetters { get; set; } = 0;
    [JsonPropertyName("ExcludeMessageDuplicate")] public bool ExcludeMessageDuplicate { get; set; } = false;


    [JsonPropertyName("SendLogToText")] public bool SendLogToText { get; set; } = true;
    [JsonPropertyName("LogChatFormat")] public string LogChatFormat { get; set; } = "[{TIME}] [{STEAMID}] {TEAM} ({PLAYERNAME}) {MESSAGE}";
    [JsonPropertyName("LogFileFormat")] public string LogFileFormat { get; set; } = ".txt";
    [JsonPropertyName("LogFileDateFormat")] public string LogFileDateFormat { get; set; } = "MM-dd-yyyy";
    [JsonPropertyName("LogInsideFileTimeFormat")] public string LogInsideFileTimeFormat { get; set; } = "HH:mm:ss";
    [JsonPropertyName("AutoDeleteLogsMoreThanXdaysOld")] public int AutoDeleteLogsMoreThanXdaysOld { get; set; } = 0;


    [JsonPropertyName("SendLogToWebHook")] public int SendLogToWebHook { get; set; } = 0;
    [JsonPropertyName("SideColorMessage")] public string SideColorMessage { get; set; } = "00FFFF";
    [JsonPropertyName("SteamApi")] public string SteamApi { get; set; } = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
    [JsonPropertyName("WebHookURL")] public string WebHookURL { get; set; } = "https://discord.com/api/webhooks/XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
    [JsonPropertyName("LogDiscordChatFormat")] public string LogDiscordChatFormat { get; set; } = "[{DATE} - {TIME}] {TEAM} {MESSAGE} (IpAddress: {IP})";
}

public class ChatLogger : BasePlugin, IPluginConfig<ChatLoggerConfig>
{
    public override string ModuleName => "Chat Logger";
    public override string ModuleVersion => "1.0.4";
    public override string ModuleAuthor => "Gold KingZ";
    public override string ModuleDescription => "Log Any InGame Chat To Log Text Or Discord WebHook";
    private static readonly HttpClient _httpClient = new HttpClient();
    public ChatLoggerConfig Config { get; set; } = new ChatLoggerConfig();
    private Dictionary<int, bool> BteamChat = new Dictionary<int, bool>();
    static string firstMessage = "";
    static string secondMessage = "";
    public void OnConfigParsed(ChatLoggerConfig config)
    {
        Config = config;

        if (Config.SendLogToWebHook < 0 || Config.SendLogToWebHook > 3)
        {
            config.SendLogToWebHook = 0;
            Console.WriteLine("SendLogToWebHook: is invalid, setting to default value (0) Please Choose 0 or 1 or 2 or 3.");
        }
    }
    
    public override void Load(bool hotReload)
    {
        AddCommandListener("say", OnPlayerSayPublic, HookMode.Post);
        AddCommandListener("say_team", OnPlayerSayTeam, HookMode.Post);

        if(Config.AutoDeleteLogsMoreThanXdaysOld > 0)
        {
            string Fpath = Path.Combine(ModuleDirectory,"../../plugins/Chat_Logger/logs");
            DeleteOldFiles(Fpath, "*" + Config.LogFileFormat, TimeSpan.FromDays(Config.AutoDeleteLogsMoreThanXdaysOld));
        }
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
        string trimmedMessage1 = message.TrimStart();
        string trimmedMessage = trimmedMessage1.TrimEnd();
        if (!string.IsNullOrEmpty(Config.ExcludeMessageGroups) && AdminManager.PlayerInGroup(player, IsAnyGroupValid(Config.ExcludeMessageGroups).ToString()))
        {
            return HookResult.Continue;
        }
        if(!string.IsNullOrEmpty(Config.ExcludeMessageContains) && IsStringValid(trimmedMessage)) return HookResult.Continue;
        if (Config.ExcludeMessageContainsLessThanXLetters > 0 && CountLetters(trimmedMessage) <= Config.ExcludeMessageContainsLessThanXLetters)
        {
            return HookResult.Continue;
        }
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
        secondMessage = firstMessage;
        firstMessage = trimmedMessage;

        if(Config.ExcludeMessageDuplicate && secondMessage == firstMessage) return HookResult.Continue;
        
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

        if(Config.SendLogToWebHook == 1)
        {
            _ = SendToDiscordWebhookNormal(Config.WebHookURL, replacerlogDiscord);
        }else if(Config.SendLogToWebHook == 2)
        {
            _ = SendToDiscordWebhookNameLink(Config.WebHookURL, replacerlogDiscord, steamId64.ToString(), vplayername);
        }else if(Config.SendLogToWebHook == 3)
        {
            _ = SendToDiscordWebhookNameLinkWithPicture(Config.WebHookURL, replacerlogDiscord, steamId64.ToString(), vplayername);
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
        string trimmedMessage1 = message.TrimStart();
        string trimmedMessage = trimmedMessage1.TrimEnd();
        if (!string.IsNullOrEmpty(Config.ExcludeMessageGroups) && AdminManager.PlayerInGroup(player, IsAnyGroupValid(Config.ExcludeMessageGroups).ToString()))
        {
            return HookResult.Continue;
        }
        if(!string.IsNullOrEmpty(Config.ExcludeMessageContains) && IsStringValid(trimmedMessage)) return HookResult.Continue;
        if (Config.ExcludeMessageContainsLessThanXLetters > 0 && CountLetters(trimmedMessage) <= Config.ExcludeMessageContainsLessThanXLetters)
        {
            return HookResult.Continue;
        }
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
        secondMessage = firstMessage;
        firstMessage = trimmedMessage;

        if(Config.ExcludeMessageDuplicate && secondMessage == firstMessage) return HookResult.Continue;

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
        
        if(Config.SendLogToWebHook == 1)
        {
            _ = SendToDiscordWebhookNormal(Config.WebHookURL, replacerlogDiscord);
        }else if(Config.SendLogToWebHook == 2)
        {
            _ = SendToDiscordWebhookNameLink(Config.WebHookURL, replacerlogDiscord, steamId64.ToString(), vplayername);
        }else if(Config.SendLogToWebHook == 3)
        {
            _ = SendToDiscordWebhookNameLinkWithPicture(Config.WebHookURL, replacerlogDiscord, steamId64.ToString(), vplayername);
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
    
    public async Task SendToDiscordWebhookNormal(string webhookUrl, string message)
    {
        try
        {
            var payload = new { content = message };
            var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(webhookUrl, content).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to send message. Status code: {response.StatusCode}, Response: {await response.Content.ReadAsStringAsync()}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }

    public async Task SendToDiscordWebhookNameLink(string webhookUrl, string message, string steamUserId, string STEAMNAME)
    {
        try
        {
            string profileLink = GetSteamProfileLink(steamUserId);
            int colorss = int.Parse(Config.SideColorMessage, System.Globalization.NumberStyles.HexNumber);
            Color color = Color.FromArgb(colorss >> 16, (colorss >> 8) & 0xFF, colorss & 0xFF);
            using (var httpClient = new HttpClient())
            {
                var embed = new
                {
                    type = "rich",
                    title = STEAMNAME,
                    url = profileLink,
                    description = message,
                    color = color.ToArgb() & 0xFFFFFF
                };

                var payload = new
                {
                    embeds = new[] { embed }
                };

                var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(webhookUrl, content).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to send message. Status code: {response.StatusCode}, Response: {await response.Content.ReadAsStringAsync()}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }
    public async Task SendToDiscordWebhookNameLinkWithPicture(string webhookUrl, string message, string steamUserId, string STEAMNAME)
    {
        try
        {
            string profileLink = GetSteamProfileLink(steamUserId);
            string profilePictureUrl = await GetSteamProfilePicture(steamUserId, Config.SteamApi);
            int colorss = int.Parse(Config.SideColorMessage, System.Globalization.NumberStyles.HexNumber);
            Color color = Color.FromArgb(colorss >> 16, (colorss >> 8) & 0xFF, colorss & 0xFF);
            using (var httpClient = new HttpClient())
            {
                var embed = new
                {
                    type = "rich",
                    description = message,
                    color = color.ToArgb() & 0xFFFFFF,
                    author = new
                    {
                        name = STEAMNAME,
                        url = profileLink,
                        icon_url = profilePictureUrl
                    }
                };

                var payload = new
                {
                    embeds = new[] { embed }
                };

                var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(webhookUrl, content).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to send message. Status code: {response.StatusCode}, Response: {await response.Content.ReadAsStringAsync()}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }
    static string GetSteamProfileLink(string userId)
    {
        return $"https://steamcommunity.com/profiles/{userId}";
    }
    static async Task<string> GetSteamProfilePicture(string userId, string APIKEY)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={APIKEY}&steamids={userId}";
                var response = await client.GetStringAsync(apiUrl);
                dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(response)!;

                string profilePictureUrl = jsonResponse.response.players[0].avatarfull;
                return profilePictureUrl;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            return string.Empty;
        }
    }
    static int CountLetters(string input)
    {
        int count = 0;

        foreach (char c in input)
        {
            if (char.IsLetter(c))
            {
                count++;
            }
        }

        return count;
    }
    
    bool IsStringValid(string input)
    {
        if (!string.IsNullOrEmpty(input) && !input.Contains(" ") && input.Any(c => Config.ExcludeMessageContains.Contains(c)) && !char.IsWhiteSpace(input.Last()))
        {
            return true;
        }
        return false;
    }
    bool IsAnyGroupValid(string input)
    {
        if (!string.IsNullOrEmpty(input) && !input.Contains(" ") && !char.IsWhiteSpace(input.Last()))
        {
            string[] inputGroups = input.Split(',').Select(Group => Group.Trim()).ToArray();

            if (inputGroups.Any(group => Config.ExcludeMessageContains.Contains(group)))
            {
                return true;
            }
        }

        return false;
    }
    static void DeleteOldFiles(string folderPath, string searchPattern, TimeSpan maxAge)
    {
        try
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

            if (directoryInfo.Exists)
            {
                FileInfo[] files = directoryInfo.GetFiles(searchPattern);
                DateTime currentTime = DateTime.Now;
                
                foreach (FileInfo file in files)
                {
                    TimeSpan age = currentTime - file.LastWriteTime;

                    if (age > maxAge)
                    {
                        file.Delete();
                        //Console.WriteLine($"Deleted file: {file.FullName}");
                    }
                }

                //Console.WriteLine("Deletion process completed.");
            }
            else
            {
                Console.WriteLine($"Directory not found: {folderPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}