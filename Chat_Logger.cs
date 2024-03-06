using System.Text;
using System.Drawing;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API;

namespace Chat_Logger;

public class ChatLoggerConfig : BasePluginConfig
{
    [JsonPropertyName("IncludeMessageGroups")] public string IncludeMessageGroups { get; set; } = "";

    [JsonPropertyName("ExcludeMessageGroups")] public string ExcludeMessageGroups { get; set; } = "#css/vip1,@css/vip1";
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
    [JsonPropertyName("WebHookURL")] public string WebHookURL { get; set; } = "https://discord.com/api/webhooks/XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
    [JsonPropertyName("LogDiscordChatFormat")] public string LogDiscordChatFormat { get; set; } = "[{DATE} - {TIME}] {TEAM} {MESSAGE} (IpAddress: {IP})";
    [JsonPropertyName("UsersWithNoAvatarImage")] public string UsersWithNoAvatarImage { get; set; } = "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b5/b5bd56c1aa4644a474a2e4972be27ef9e82e517e_full.jpg";
}

public class ChatLogger : BasePlugin, IPluginConfig<ChatLoggerConfig>
{
    public override string ModuleName => "Chat Logger (Log Chat To Text Or Discord WebHook)";
    public override string ModuleVersion => "1.0.6";
    public override string ModuleAuthor => "Gold KingZ";
    public override string ModuleDescription => "https://github.com/oqyh/cs2-Chat-Logger";
    private static readonly HttpClient _httpClient = new HttpClient();
    private static readonly HttpClient httpClient = new HttpClient();
    public ChatLoggerConfig Config { get; set; } = new ChatLoggerConfig();
    private Dictionary<int, bool> BteamChat = new Dictionary<int, bool>();
    private Dictionary<ulong, bool> PlayerIsExcluded = new Dictionary<ulong, bool>();
    private Dictionary<ulong, bool> PlayerIsIncluded = new Dictionary<ulong, bool>();
    static string firstMessage = "";
    static string secondMessage = "";
    public void OnConfigParsed(ChatLoggerConfig config)
    {
        Config = config;

        if (Config.SendLogToWebHook < 0 || Config.SendLogToWebHook > 3)
        {
            config.SendLogToWebHook = 0;
            Console.WriteLine("|||||||||||||||||||||||||||||||||||| I N V A L I D ||||||||||||||||||||||||||||||||||||");
            Console.WriteLine("SendLogToWebHook: is invalid, setting to default value (0) Please Choose 0 or 1 or 2 or 3.");
            Console.WriteLine("SendLogToWebHook (0) = Disableis invalid");
            Console.WriteLine("SendLogToWebHook (1) = Text Only");
            Console.WriteLine("SendLogToWebHook (2) = Text With + Name + Hyperlink To Steam Profile");
            Console.WriteLine("SendLogToWebHook (3) = Text With + Name + Hyperlink To Steam Profile + Profile Picture");
            Console.WriteLine("|||||||||||||||||||||||||||||||||||| I N V A L I D ||||||||||||||||||||||||||||||||||||");
        }
        
        if (Config.SideColorMessage.StartsWith("#"))
        {
            Config.SideColorMessage = Config.SideColorMessage.Substring(1);
            //Console.WriteLine("SideColorMessage: # Detect At Start No Need For That");
        }
    }
    
    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
        AddCommandListener("say", OnPlayerSayPublic, HookMode.Post);
        AddCommandListener("say_team", OnPlayerSayTeam, HookMode.Post);
        RegisterListener<Listeners.OnMapStart>(OnMapStart);
    }
    private void OnMapStart(string Map)
    {
        if(Config.AutoDeleteLogsMoreThanXdaysOld > 0)
        {
            string Fpath = Path.Combine(ModuleDirectory,"../../plugins/Chat_Logger/logs");
            DeleteOldFiles(Fpath, "*" + Config.LogFileFormat, TimeSpan.FromDays(Config.AutoDeleteLogsMoreThanXdaysOld));
        }
    }
    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event == null)return HookResult.Continue;
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV) return HookResult.Continue;
        var playerid = player.SteamID;

        if(!string.IsNullOrEmpty(Config.ExcludeMessageGroups))
        {
            if(IsPlayerInGroupPermissionExcluded(player))
            {
                if (!PlayerIsExcluded.ContainsKey(playerid))
                {
                    PlayerIsExcluded.Add(playerid, true);
                }
            }
        }

        if(!string.IsNullOrEmpty(Config.IncludeMessageGroups))
        {
            if(IsPlayerInGroupPermissionIncluded(player))
            {
                if (!PlayerIsIncluded.ContainsKey(playerid))
                {
                    PlayerIsIncluded.Add(playerid, true);
                }
            }
        }

        return HookResult.Continue;
    }
    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV)return HookResult.Continue;
        var playerid = player.SteamID;

        if (PlayerIsExcluded.ContainsKey(playerid))
        {
            PlayerIsExcluded.Remove(playerid);
        }

        if (PlayerIsIncluded.ContainsKey(playerid))
        {
            PlayerIsIncluded.Remove(playerid);
        }
        return HookResult.Continue;
    }

    private HookResult OnPlayerSayPublic(CCSPlayerController? player, CommandInfo info)
	{
        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV)return HookResult.Continue;
        
        string? chatteam = null;
        if (player.UserId.HasValue)
        {
            BteamChat[player.UserId.Value] = false;
            bool isTeamChat = BteamChat[player.UserId.Value];
            chatteam = isTeamChat ? "[TEAM]" : "[ALL]";
        }

        var message = info.GetArg(1);
        var playerid = player.SteamID;

        if (string.IsNullOrWhiteSpace(message)) return HookResult.Continue;
        string trimmedMessage1 = message.TrimStart();
        string trimmedMessage = trimmedMessage1.TrimEnd();
        if (!string.IsNullOrEmpty(Config.ExcludeMessageGroups) && PlayerIsExcluded.ContainsKey(playerid))
        {
            return HookResult.Continue;
        }
        if (!string.IsNullOrEmpty(Config.IncludeMessageGroups) && !PlayerIsIncluded.ContainsKey(playerid))
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
        var steamId2 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId2 : "InvalidSteamID";
        var steamId3 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId3 : "InvalidSteamID";
        var steamId32 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId32.ToString() : "InvalidSteamID";
        var steamId64 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId64.ToString() : "InvalidSteamID";
        var GetIpAddress = player.IpAddress;
        var ipAddress = GetIpAddress?.Split(':')[0] ?? "InValidIpAddress";
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
                Console.WriteLine("|||||||||||||||||||||||||||||| E R R O R |||||||||||||||||||||");
                Console.WriteLine("[Chat_Logger] Please Give Chat_Logger.dll Permissions To Write");
                Console.WriteLine("[Chat_Logger] If You Already Did Restart The Server");
                Console.WriteLine("|||||||||||||||||||||||||||||| E R R O R |||||||||||||||||||||");
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
        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV)return HookResult.Continue;
        
        string? chatteam = null;
        if (player.UserId.HasValue)
        {
            BteamChat[player.UserId.Value] = true;
            bool isTeamChat = BteamChat[player.UserId.Value];
            chatteam = isTeamChat ? "[TEAM]" : "[ALL]";
        }

        var message = info.GetArg(1);
        var playerid = player.SteamID;

        if (string.IsNullOrWhiteSpace(message)) return HookResult.Continue;
        string trimmedMessage1 = message.TrimStart();
        string trimmedMessage = trimmedMessage1.TrimEnd();
        if (!string.IsNullOrEmpty(Config.ExcludeMessageGroups) && PlayerIsExcluded.ContainsKey(playerid))
        {
            return HookResult.Continue;
        }
        if (!string.IsNullOrEmpty(Config.IncludeMessageGroups) && !PlayerIsIncluded.ContainsKey(playerid))
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
        var steamId2 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId2 : "InvalidSteamID";
        var steamId3 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId3 : "InvalidSteamID";
        var steamId32 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId32.ToString() : "InvalidSteamID";
        var steamId64 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId64.ToString() : "InvalidSteamID";
        var GetIpAddress = player.IpAddress;
        var ipAddress = GetIpAddress?.Split(':')[0] ?? "InValidIpAddress";
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
                Console.WriteLine("|||||||||||||||||||||||||||||| E R R O R |||||||||||||||||||||");
                Console.WriteLine("[Chat_Logger] Please Give Chat_Logger.dll Permissions To Write");
                Console.WriteLine("[Chat_Logger] If You Already Did Restart The Server");
                Console.WriteLine("|||||||||||||||||||||||||||||| E R R O R |||||||||||||||||||||");
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
        }
        catch
        {

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
            }
        }
        catch
        {

        }
    }
    public async Task SendToDiscordWebhookNameLinkWithPicture(string webhookUrl, string message, string steamUserId, string STEAMNAME)
    {
        try
        {
            string profileLink = GetSteamProfileLink(steamUserId);
            string profilePictureUrl = await GetProfilePictureAsync(steamUserId, Config.UsersWithNoAvatarImage);
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
            }
        }
        catch
        {

        }
    }
    public static async Task<string> GetProfilePictureAsync(string steamId64, string defaultImage)
    {
        try
        {
            string apiUrl = $"https://steamcommunity.com/profiles/{steamId64}/?xml=1";

            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string xmlResponse = await response.Content.ReadAsStringAsync();
                int startIndex = xmlResponse.IndexOf("<avatarFull><![CDATA[") + "<avatarFull><![CDATA[".Length;
                int endIndex = xmlResponse.IndexOf("]]></avatarFull>", startIndex);

                if (endIndex >= 0)
                {
                    string profilePictureUrl = xmlResponse.Substring(startIndex, endIndex - startIndex);
                    return profilePictureUrl;
                }
                else
                {
                    return defaultImage;
                }
            }
            else
            {
                return null!;
            }
        }
        catch
        {
            return null!;
        }
    }
    static string GetSteamProfileLink(string userId)
    {
        return $"https://steamcommunity.com/profiles/{userId}";
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
                //Console.WriteLine($"Directory not found: {folderPath}");
            }
        }
        catch
        {
            
        }
    }
    private bool IsPlayerInGroupPermissionExcluded(CCSPlayerController player)
    {
        string[] excludedGroups = Config.ExcludeMessageGroups.Split(',');
        foreach (string group in excludedGroups)
        {
            if (group.StartsWith("#"))
            {
                if (AdminManager.PlayerInGroup(player, group))
                {
                    return true;
                }
            }else if (group.StartsWith("@"))
            {
                if (AdminManager.PlayerHasPermissions(player, group))
                {
                    return true;
                }
            }
        }
        return false;
    }
    private bool IsPlayerInGroupPermissionIncluded(CCSPlayerController player)
    {
        string[] excludedGroups = Config.IncludeMessageGroups.Split(',');
        foreach (string group in excludedGroups)
        {
            if (group.StartsWith("#"))
            {
                if (AdminManager.PlayerInGroup(player, group))
                {
                    return true;
                }
            }else if (group.StartsWith("@"))
            {
                if (AdminManager.PlayerHasPermissions(player, group))
                {
                    return true;
                }
            }
        }
        return false;
    }
    private void OnMapEnd()
    {
        PlayerIsIncluded.Clear();
        PlayerIsExcluded.Clear();
    }

    public override void Unload(bool hotReload)
    {
        PlayerIsIncluded.Clear();
        PlayerIsExcluded.Clear();
    }
}